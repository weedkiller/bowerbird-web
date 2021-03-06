﻿/// <reference path="../../libs/log.js" />
/// <reference path="../../libs/require/require.js" />
/// <reference path="../../libs/jquery/jquery-1.7.2.js" />
/// <reference path="../../libs/underscore/underscore.js" />
/// <reference path="../../libs/backbone/backbone.js" />
/// <reference path="../../libs/backbone.marionette/backbone.marionette.js" />

// ChatCollection
// --------------

define(['jquery', 'underscore', 'backbone', 'models/chat'],
function ($, _, Backbone, Chat) 
{
    var ChatCollection = Backbone.Collection.extend({
        
        model: Chat,

        url: '/chats',

        initialize: function () {
            _.extend(this, Backbone.Events);
        }
    });

    return ChatCollection;
});