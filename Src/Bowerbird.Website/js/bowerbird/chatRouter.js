﻿
window.Bowerbird.ChatRouter = Backbone.Model.extend({

    // INIT-----------------------------------------

    initialize: function (options) {

        log('chatRouter.Initialize');

        _.extend(this, Backbone.Events);
        _.bindAll(this,
        'joinChat',
        'exitChat',
        'startTyping',
        'stopTyping',
        'sendMessage',
        'setupChat',
        'typing',
        'chatMessageReceived',
        'userJoinedChat',
        'userExitedChat');

        this.appManager = options.appManager;
        options.appManager.chats.on('remove', this.exitChat, this);
        this.chatHub = $.connection.chatHub;
        this.chatHub.chatMessageReceived = this.chatMessageReceived;
        this.chatHub.userJoinedChat = this.userJoinedChat;
        this.chatHub.userExitedChat = this.userExitedChat;
        this.chatHub.setupChat = this.setupChat;
        this.chatHub.typing = this.typing;

        log('chatRouter.Initialize - done');
    },

    // TO HUB---------------------------------------

    joinChat: function (chat) {
        this.chatHub.joinChat(chat.get('group').get('id'));
    },

    exitChat: function (chat) {
        log('chatRouter.exitChat');
        //this.trigger('chatEnded', chat);
        this.chatHub.exitChat(chat.id);
        // and the rest.... 
    },

    startTyping: function (chat) {
        log('chatRouter.startTyping');
        this.chatHub.typing(chat.id, true);
    },

    stopTyping: function (chat) {
        log('chatRouter.stopTyping');
        this.chatHub.typing(chat.id, false);
    },

    sendMessage: function (message, chat) {
        log('chatRouter.sendMessage');
        this.chatHub.sendChatMessage(chat.get('id'), message);
    },

    // FROM HUB-------------------------------------

    setupChat: function (data) {
        log('chatRouter.setupChat');
        var self = this;
        var chat = self.appManager.chats.get(data.id);

        $.each(data.users, function (index, xitem) {
            var chatUser = _.find(chat.chatUsers, function (yitem) {
                return xitem.user.id == yitem.user.id;
            });
            if (_.isNull(chatUser) || _.isUndefined(chatUser)) {
                var user = self.appManager.users.get(xitem.id);
                if (_.isNull(user) || _.isUndefined(user)) {
                    user = new Bowerbird.Models.User(xitem);
                }
                chatUser = new Bowerbird.Models.ChatUser({ chat: chat, user: user, status: 0 });
            }
            chat.chatUsers.add(chatUser);
        });
    },

    typing: function (data) {
        // for the given data.chat
        // find the chatUser
        // set status to typing/not typing
    },

    chatMessageReceived: function (data) {
        log('message for chatId: ' + data.chatId + ' with content ' + data.message);
        var chat = this.appManager.chats.get(data.chatId);
        chat.chatMessages.add(new Bowerbird.Models.ChatMessage(data));
    },

    userJoinedChat: function (data) {
        log('chatRouter.userJoinedChat');
        var chat = this.appManager.chats.get(data.id);

        var chatUsers = chat.chatUsers.pluck('user');

        var match = _.any(chatUsers, function (user) {
            return user.id == data.user.id;
        });

        if (match) return;

        var user = this.appManager.users.get(data.user.id);
        if (_.isNull(user) || _.isUndefined(user)) {
            user = new Bowerbird.Models.User(data.user);
        }
        chatUser = new Bowerbird.Models.ChatUser({ chat: chat, user: user, status: data.status });
        chat.chatUsers.add(chatUser);
    },

    userExitedChat: function (data) {
        log('chatRouter.userExitedChat');
        var chat = this.appManager.chats.get(data.id);

        var chatUsers = chat.chatUsers.pluck('user');
        var match = _.any(chatUsers, function (user) {
            return user.id == data.user.id;
        });

        if (match) return;

        var chatUser = _.find(chat.chatUsers, function (item) {
            return item.user.id == data.user.id;
        });

        if (_.isNull(chatUser) || _.isUndefined(chatUser)) {
            return;
        } else {
            chat.chatUsers.remove(chatUser);
        }
    }
});