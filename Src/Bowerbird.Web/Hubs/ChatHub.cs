﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System;
using System.Linq;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Infrastructure;
using SignalR.Hubs;
using Bowerbird.Core.DesignByContract;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Web.Factories;
using System.Collections.Generic;
using Bowerbird.Core.Indexes;
using Bowerbird.Core.Commands;
using Bowerbird.Core.Config;
using System.Dynamic;

namespace Bowerbird.Web.Hubs
{
    public class ChatHub : Hub//, IDisconnect
    {
        #region Members

        private readonly IUserViewFactory _userViewFactory;
        private readonly IGroupViewFactory _groupViewFactory;
        private readonly IDocumentSession _documentSession;
        private readonly IMessageBus _messageBus;
        private readonly IPermissionManager _permissionManager;

        #endregion

        #region Constructors

        public ChatHub(
            IUserViewFactory userViewFactory,
            IGroupViewFactory groupViewFactory,
            IDocumentSession documentSession,
            IMessageBus messageBus,
            IPermissionManager permissionManager)
        {
            Check.RequireNotNull(userViewFactory, "userViewFactory");
            Check.RequireNotNull(groupViewFactory, "groupViewFactory");
            Check.RequireNotNull(documentSession, "documentSession");
            Check.RequireNotNull(messageBus, "messageBus");
            Check.RequireNotNull(permissionManager, "permissionManager");

            _userViewFactory = userViewFactory;
            _groupViewFactory = groupViewFactory;
            _documentSession = documentSession;
            _messageBus = messageBus;
            _permissionManager = permissionManager;
        }

        #endregion

        #region Methods

        public dynamic GetChat(string chatId)
        {
            var chat = _documentSession.Load<Chat>(chatId);
            var users = _documentSession.Load<User>(chat.Users.Select(x => x.Id));

            dynamic chatDetails = new ExpandoObject();

            chatDetails.ChatId = chat.Id;
            chatDetails.Users = users.Select(_userViewFactory.Make);

            if (chat.ChatType == "group")
            {
                var group = _documentSession
                    .Query<All_Groups.Result, All_Groups>()
                    .AsProjection<All_Groups.Result>()
                    .Where(x => x.GroupId == chat.Group.Id)
                    .ToList()
                    .First();

                chatDetails.Group = _groupViewFactory.Make(group);

                var chatMessageUsers = _documentSession.Load<User>(chat.Messages.Select(x => x.User.Id).Distinct());

                // Group chat users get some historic messages for context
                chatDetails.Messages = chat
                                        .Messages
                                        .OrderByDescending(x => x.Timestamp)
                                        .Take(30)
                                        .Select(x => new
                                        {
                                            x.Id,
                                            Type = "usermessage",
                                            ChatId = chat.Id,
                                            x.Timestamp,
                                            x.Message,
                                            FromUser = _userViewFactory.Make(chatMessageUsers.Single(y => y.Id == x.User.Id))
                                        });
            }

            return chatDetails;
        }

        public void JoinChat(string chatId, string[] inviteeUserIds, string groupId)
        {
            // Only allow users who have permission to chat (in group or app-wide)
            if(!_permissionManager.HasGroupPermission(PermissionNames.Chat, Context.User.Identity.Name, string.IsNullOrWhiteSpace(groupId) ? Constants.AppRootId : groupId))
            {
                return;
            }

            // Get chat
            var chat = _documentSession.Load<Chat>(chatId);

            // Check if chat exists
            if (chat == null)
            {
                // If not, make it and add users to chat
                _messageBus.SendAsync(new ChatCreateCommand()
                {
                    ChatId = chatId,
                    CreatedByUserId = Context.User.Identity.Name,
                    CreatedDateTime = DateTime.UtcNow,
                    UserIds = inviteeUserIds,
                    GroupId = groupId
                });
            }
            else
            {
                // Just add users to existing chat
                _messageBus.SendAsync(new ChatUpdateCommand()
                {
                    ChatId = chatId,
                    AddUserIds = inviteeUserIds,
                    RemoveUserIds = new string[] { }
                });
            }
        }

        public void ExitChat(string chatId)
        {
            // Remove user
            _messageBus.SendAsync(new ChatUpdateCommand()
            {
                ChatId = chatId,
                AddUserIds = new string[] { },
                RemoveUserIds = new[] { Context.User.Identity.Name }
            });
        }

        public void Typing(string chatId, bool isTyping)
        {
            // Notify all users of typing status
            Clients["chat-" + chatId].userIsTyping(
                new
                {
                    ChatId = chatId,
                    Timestamp = DateTime.UtcNow,
                    IsTyping = isTyping,
                    UserId = Context.User.Identity.Name
                });
        }

        public void SendChatMessage(string chatId, string messageId, string message)
        {
            // Add message to chat
            _messageBus.SendAsync(new ChatMessageCreateCommand()
            {
                ChatId = chatId,
                UserId = Context.User.Identity.Name,
                Timestamp = DateTime.UtcNow,
                MessageId = messageId,
                Message = message
            });
        }

        private string GenerateHash(string value)
        {
            var hash = 0;
            if (value.Length == 0) return hash.ToString();
            for (int i = 0; i < value.Length; i++)
            {
                var ch = value[i];
                hash = ((hash << 5) - hash) + ch;
                hash = hash & hash; // Convert to 32bit integer
            }
            return hash.ToString();
        }

        private string GenerateChatId(IEnumerable<string> ids)
        {
            var sortedIds = ids.OrderBy(x => x);
            return "chats/" + GenerateHash(string.Join(string.Empty, sortedIds.ToArray()));
        }

        //public Task Disconnect()
        //{
        //    // Get user by connection id
        //    var user = GetUserByConnectionId(Context.ConnectionId);

        //    // Get chats where connection exists
        //    var chats = _documentSession
        //                    .Query<Chat>()
        //                    .Where(x => x.Users.Any(y => y.Id == user.Id))
        //                    .ToList();

        //    foreach (var chat in chats)
        //    {
        //        chat.RemoveUser(user.Id);
        //        _documentSession.Store(chat);
        //    }

        //    _documentSession.SaveChanges();

        //    return Task.Factory.StartNew(() => { });
        //}

        //private IEnumerable<User> GetUsersByConnectionIds(params string[] connectionIds)
        //{
        //    return _documentSession
        //        .Query<All_Users.Result, All_Users>()
        //        .AsProjection<All_Users.Result>()
        //        .Where(x => x.ConnectionIds.Any(y => y.In(connectionIds)))
        //        .ToList()
        //        .Select(x => x.User);
        //}

        //private User GetUserByConnectionId(string connectionId)
        //{
        //    return _documentSession
        //        .Query<All_Users.Result, All_Users>()
        //        .AsProjection<All_Users.Result>()
        //        .Where(x => x.ConnectionIds.Any(y => y == connectionId))
        //        .ToList()
        //        .Select(x => x.User)
        //        .First();
        //}

        #endregion
    }
}