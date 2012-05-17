﻿
window.Bowerbird.Views.MediaResourceItemView = Backbone.View.extend({
    className: 'media-resource-uploaded',

    events: {
        'click .view-media-resource-button': 'viewMediaResource',
        'click .add-caption-button': 'viewMediaResource',
        'click .remove-media-resource-button': 'removeMediaResource'
    },

    initialize: function (options) {
        _.extend(this, Backbone.Events);
        _.bindAll(this, 'showTempMedia', 'showUploadedMedia');
        this.mediaResource = options.mediaResource;
        this.mediaResource.on('change:MediumImageUri', this.showUploadedMedia);
    },

    render: function () {
        this.$el.append(ich.ObservationMediaResourceUploaded(this.mediaResource.toJSON()));
        window.scrollTo(0, 0);
        return this;
    },

    viewMediaResource: function () {
        alert('Coming soon');
    },

    removeMediaResource: function () {
        var addToRemoveList = false;
        if (app.get('newObservation').mediaResources.find(function (mr) { return mr.Id == this.mediaResource.Id; }) != null) {
            addToRemoveList = true;
        }
        app.get('newObservation').addMediaResources.remove(this.mediaResource.Id);
        app.get('newObservation').mediaResources.remove(this.mediaResource.Id);
        if (addToRemoveList) {
            app.get('newObservation').removeMediaResources.add(this.mediaResource);
        }
        this.remove();
    },

    showTempMedia: function (img) {
        this.$el.find('div:first-child img').replaceWith($(img));
    },

    showUploadedMedia: function (mediaResource) {
        this.$el.find('div:first-child img').replaceWith($('<img src="' + mediaResource.get('MediumImageUri') + '" alt="" />'));
    }
});