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
using System.Threading.Tasks;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Extensions;
using Bowerbird.Web.Services;
using SignalR.Hubs;
using Bowerbird.Core.DesignByContract;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Web.Factories;
using System.Collections.Generic;
using Bowerbird.Core.Indexes;

namespace Bowerbird.Web.Hubs
{
    public class ChatHub : Hub, IDisconnect
    {
        #region Members

        private readonly IUserViewFactory _userViewFactory;
        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public ChatHub(
            IUserViewFactory userViewFactory,
            IDocumentSession documentSession)
        {
            Check.RequireNotNull(userViewFactory, "userViewFactory");
            Check.RequireNotNull(documentSession, "documentSession");

            _userViewFactory = userViewFactory;
            _documentSession = documentSession;
        }

        #endregion

        #region Methods

        public void JoinChat(string chatId)
        {
            var chat = _documentSession.Load<Chat>(chatId);
            var user = GetUserByConnectionId(Context.ConnectionId);

            chat.AddUser(user);
            _documentSession.Store(chat);

            _documentSession.SaveChanges();
            
            //var chatUsers = _documentSession
            //    .Load<User>(((List<User>)chat.Users).Select(x => x.Id))
            //    .Select(_userViewFactory.Make);

            //var chatMessageUsers = _documentSession
            //    .Load<User>(((List<ChatMessage>)chat.Messages).Select(x => x.User.Id));

            //var chatMessages = ((List<ChatMessage>)chat.Messages)
            //    .OrderByDescending(x => x.Timestamp)
            //    .Take(30)
            //    .Select(x => new
            //    {
            //        Id = x.Id,
            //        ChatId = chat.Id,
            //        FromUser = _userViewFactory.Make(chatMessageUsers.Single(y => y.Id == x.User.Id)),
            //        Timestamp = x.Timestamp,
            //        Message = x.Message
            //    });

            //// Add user connection to signalr chat group
            //Groups.Add(Context.ConnectionId, chatId);

            //// Return current chat state to user joining chat
            //Caller.groupChatJoined(new
            //    {
            //        ChatId = chatId,
            //        Users = chatUsers,
            //        Messages = chatMessages
            //    });

            //// Let connected users know another user has joined the chat
            //Clients[chatId].userJoinedChat(
            //    new
            //    {
            //        ChatId = chatId,
            //        Timestamp = DateTime.UtcNow,
            //        User = _userViewFactory.Make(user)
            //    });
        }

        public void ExitChat(string chatId)
        {
            var user = GetUserByConnectionId(Context.ConnectionId);
            var chat = _documentSession.Load<Chat>(chatId);
            
            chat.RemoveUser(user.Id);
            _documentSession.Store(chat);

            _documentSession.SaveChanges();
        }

        public void Typing(string chatId, bool typing)
        {
            var user = GetUserByConnectionId(Context.ConnectionId);

            Clients[chatId].userIsTyping(
                new
                {
                    ChatId = chatId,
                    Timestamp = DateTime.UtcNow,
                    Typing = typing,
                    User = _userViewFactory.Make(user)
                });
        }

        public void SendChatMessage(string chatId, string message, string[] userIds = null)
        {
            Caller.debugToLog(string.Format("ChatHub.sendChatMessage - chatId:{0} message:{1}", chatId, message));

            var chat = _documentSession.Load<Chat>(chatId);
            var user = GetUserByConnectionId(Context.ConnectionId);

            if (chat == null)
            {
                var users = _documentSession.Load<User>(userIds).ToList();
                users.Add(user);
                chat = new Chat(chatId, user, users, DateTime.UtcNow, message);
            }

            chat.AddMessage(user, DateTime.UtcNow, message);

            _documentSession.Store(chat);
            //dynamic chatMessage = null;

            //// Create chat if it doesn't exist yet
            //if (chat == null)
            //{
            //    // Get all users
            //    var allUserIds = userIds.ToList();
            //    allUserIds.Add(user.Id);
            //    var allUsers = _documentSession.Load<User>(allUserIds).ToList();

            //    // Get all clientIds for all users that will receive message and add the to signalr group
            //    foreach (var connectionId in allUsers.SelectMany(x => x.Sessions.Select(y => y.ConnectionId)))
            //    {
            //        Groups.Add(connectionId, chatId);
            //    }

            //    chatMessage = new
            //    {
            //        ChatId = chatId,
            //        Timestamp = DateTime.UtcNow,
            //        Id = Guid.NewGuid().ToString(),
            //        FromUser = _userViewFactory.Make(user),
            //        Message = message,
            //        Users = allUsers.Select(_userViewFactory.Make)
            //    };
            //} 
            //else
            //{
            //    chatMessage = new
            //    {
            //        ChatId = chatId,
            //        Timestamp = DateTime.UtcNow,
            //        Id = Guid.NewGuid().ToString(),
            //        FromUser = _userViewFactory.Make(user),
            //        Message = message
            //    };
            //}

            //Clients[chatId].chatMessageReceived(chatMessage);
        }

        public Task Disconnect()
        {
            var user = GetUserByConnectionId(Context.ConnectionId);

            var chats = _documentSession
                            .Query<Chat>()
                            .Where(x => x.Users.Any(y => y.Id == user.Id))
                            .ToList();

            foreach (var chat in chats)
            {
                chat.RemoveUser(user.Id);
                _documentSession.Store(chat);
            }

            _documentSession.SaveChanges();

            return Task.Factory.StartNew(() => { });
        }

        private IEnumerable<User> GetUsersByConnectionIds(params string[] connectionIds)
        {
            return _documentSession
                .Query<All_Users.Result, All_Users>()
                .AsProjection<All_Users.Result>()
                .Where(x => x.ConnectionIds.Any(y => y.In(connectionIds)))
                .ToList()
                .Select(x => x.User);
        }

        private User GetUserByConnectionId(string connectionId)
        {
            return GetUsersByConnectionIds(connectionId).First();
        }

        #endregion
    }
}