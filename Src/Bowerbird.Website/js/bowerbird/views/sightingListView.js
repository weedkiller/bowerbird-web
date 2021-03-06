﻿// <reference path="../../libs/log.js" />
/// <reference path="../../libs/require/require.js" />
/// <reference path="../../libs/jquery/jquery-1.7.2.js" />
/// <reference path="../../libs/underscore/underscore.js" />
/// <reference path="../../libs/backbone/backbone.js" />
/// <reference path="../../libs/backbone.marionette/backbone.marionette.js" />

// SightingListView
// ----------------

// Shows sighting items for selected user/group
define(['jquery', 'underscore', 'backbone', 'app', 'ich', 'views/sightingdetailsview', 'infinitescroll'],
function ($, _, Backbone, app, ich, SightingDetailsView) {

    var SightingListView = Backbone.Marionette.CompositeView.extend({
        template: 'SightingList',

        itemView: SightingDetailsView,

        activeTab: '',

        events: {
            'click #stream-load-more-button': 'onLoadMoreClicked',
            'click .details-view-button': 'onDetailsTabClicked',
            'click .thumbnails-view-button': 'onThumbnailsTabClicked',
            'click .map-view-button': 'onMapTabClicked',
            'click .sort-button': 'showSortMenu',
            'click .sort-button a': 'changeSort',
            'click .search-button': 'showHideSearch'
        },

        initialize: function (options) {
            _.bindAll(this, 'appendHtml', 'clearListAnPrepareShowLoading', 'onDetailsTabClicked', 'onThumbnailsTabClicked');

            //            if (!options.activeTab) {
            //                this.activeTab = 'thumbnails';
            //            } else {
            //                this.activeTab = options.activeTab;
            //            }

            this.activeTab = this.collection.viewType;

            this.newItemsCount = 0;

            this.collection.on('fetching', this.onLoadingStart, this);
            this.collection.on('fetched', this.onLoadingComplete, this);
            this.collection.on('search-reset', this.clearListAnPrepareShowLoading, this);
        },

        serializeData: function () {
            return {
                Model: {
                    IsFavourites: this.collection.subId === 'favourites',
                    Query: {
                        Id: this.collection.subId === 'favourites' ? null : this.collection.subId,
                        Page: this.collection.page,
                        PageSize: this.collection.pageSize,
                        View: this.collection.viewType,
                        Sort: this.collection.sortByType
                    }
                }
            };
        },

        onShow: function () {
            this._showDetails();
        },

        showBootstrappedDetails: function () {
            var els = this.$el.find('.sighting-item');

            var template = this.collection.viewType == 'details' ? 'SightingListItem' : null;

            _.each(this.collection.models, function (item, index) {
                var childView = new SightingDetailsView({ model: item, tagName: 'li', template: template });
                childView.$el = $(els[index]);
                childView.showBootstrappedDetails();
                childView.delegateEvents();
                this.storeChild(childView);
            }, this);

            this._showDetails();
        },

        _showDetails: function () {
            this.$el.find('.tabs li a, .tabs .tab-list-button, .search-button').not('.details-view-button').tipsy({ gravity: 's' });
            this.$el.find('.details-view-button').tipsy({ gravity: 'se' });

            this.$el.find('h3 a').on('click', function (e) {
                e.preventDefault();
                Backbone.history.navigate($(this).attr('href'), { trigger: true });
                return false;
            });

            this.onLoadingComplete(this.collection);
            this.changeSortLabel(this.collection.sortByType);
            this.switchTabHighlight(this.collection.viewType);

            this.collection.on('criteria-changed', this.clearListAnPrepareShowLoading);

            this.enableInfiniteScroll();

            this.refresh();
        },

        changeSortLabel: function (value) {
            var label = '';
            switch (value) {
                case 'oldest':
                    label = 'Oldest Added';
                    break;
                case 'a-z':
                    label = 'Alphabetical (A-Z)';
                    break;
                case 'z-a':
                    label = 'Alphabetical (Z-A)';
                    break;
                case 'newest':
                default:
                    label = 'Latest Added';
                    break;
            }

            this.$el.find('.sort-button .tab-list-selection').empty().text(label);
            this.$el.find('.tabs li a, .tabs .tab-list-button').tipsy.revalidate();
        },

        refresh: function () {
            _.each(this.children, function (childView) {
                childView.refresh();
            });
        },

        enableInfiniteScroll: function () {
            // Infinite scroll
            var that = this;
            this.$el.find('.sighting-list').infinitescroll({
                navSelector: '.stream-load-more', // selector for the paged navigation (it will be hidden)
                nextSelector: ".next-page", // selector for the NEXT link (to page 2)
                itemSelector: '.sighting-item', // selector for all items you'll retrieve                
                dataType: 'json',
                appendCallback: false,
                binder: $(window), // used to cache the selector for the element that will be scrolling
                maxPage: that.collection.total > 0 ? (Math.floor(that.collection.total / that.collection.pageSize)) + 1 : 0, // Total number of navigable pages
                animate: false,
                pixelsFromNavToBottom: 30,
                path: function (currentPage) {
                    //return that.buildPagingUrl(true);
                    return that.collection.searchUrl(true, currentPage);
                },
                loading: {
                    msg: $(ich.StreamLoading()),
                    speed: 1
                }
            }, function (json, opts) {
                // Get current page
                //var page = opts.state.currPage;

                for (var x = 0; x < json.Model.Sightings.PagedListItems.length; x++) {
                    that.collection.add(json.Model.Sightings.PagedListItems[x]);
                }

                var maxPage = that.collection.total > 0 ? (Math.floor(that.collection.total / that.collection.pageSize)) + 1 : 0;
                if (json.Model.Sightings.Page === maxPage) {
                    that.$el.find('.sighting-list').append('<div class="no-more-items">You\'ve reached the end. Time to add some more sightings!</div>');
                    opts.state.isDone = true;
                }
            });

            this.$el.find('.stream-load-more').hide();
        },

        buildItemView: function (item, ItemView) {
            var template = 'SightingTileDetails';
            var className = ' tile-sighting-details';
            if (this.collection.viewType === 'details') {
                template = 'SightingListItem';
                className = '';
            }

            var view = new ItemView({
                template: template,
                model: item,
                tagName: 'li',
                className: 'sighting-item' + className
            });
            return view;
        },

        appendHtml: function (collectionView, itemView) {
            $.when(this.sortAndAppend(collectionView, itemView))
                .then(function () {
                    itemView._showDetails();
                    itemView.refresh();
                });

            //            var items = this.collection.pluck('Id');
            //            var index = _.indexOf(items, itemView.model.id);

            //            var $li = collectionView.$el.find('.sighting-items > li:eq(' + (index) + ')');

            //            if ($li.length === 0) {
            //                collectionView.$el.find('.sighting-items').append(itemView.el);
            //            } else {
            //                $li.before(itemView.el);
            //            }

            //            itemView._showDetails();

            //            itemView.refresh();
        },

        sortAndAppend: function (collectionView, itemView) {
            var that = this;
            return $.Deferred(function (dfd) {
                var items = that.collection.pluck('Id');
                var index = _.indexOf(items, itemView.model.id);

                var $li = collectionView.$el.find('.sighting-items > li:eq(' + (index) + ')');

                if ($li.length === 0) {
                    collectionView.$el.find('.sighting-items').append(itemView.el);
                } else {
                    $li.before(itemView.el);
                }
                dfd.resolve();
            }).promise();
        },

        onLoadMoreClicked: function () {
            this.$el.find('.stream-message, .stream-load-more').remove();
            this.collection.fetchNextPage();
        },

        onLoadNewClicked: function () {
            this.$el.find('.stream-message, .stream-load-new').remove();
            this.collection.add(this.newStreamItemsCache);
            this.newStreamItemsCache = [];
            this.newItemsCount = 0;
        },

        onLoadingStart: function (collection) {
            this.$el.find('.sighting-list').append(ich.StreamLoading());
        },

        onLoadingComplete: function (collection) {
            this.$el.find('.stream-message, .stream-loading').remove();
            if (collection.length === 0) {
                this.$el.find('.sighting-list').append(ich.StreamMessage());
            }
            if (collection.pageInfo().next) {
                var pagingInfo = {
                    NextUrl: this.collection.searchUrl(true, this.collection.page + 1)
                };

                if (this.collection.page > 1) {
                    pagingInfo['PreviousUrl'] = this.collection.searchUrl(true, this.collection.page - 1);
                }

                this.$el.find('.sighting-list').append(ich.StreamLoadMore(pagingInfo));
            }

            this.enableInfiniteScroll();
        },

        onDetailsTabClicked: function (e) {
            e.preventDefault();
            this.clearListAnPrepareShowLoading();
            this.switchTabHighlight('details');
            this.collection.changeView('details');
            Backbone.history.navigate(this.collection.searchUrl(), { trigger: false });
            return false;
        },

        onThumbnailsTabClicked: function (e) {
            e.preventDefault();
            this.clearListAnPrepareShowLoading();
            this.switchTabHighlight('thumbnails');
            this.collection.changeView('thumbnails');
            Backbone.history.navigate(this.collection.searchUrl(), { trigger: false });
            return false;
        },

        onMapTabClicked: function (e) {
            e.preventDefault();
            this.clearListAnPrepareShowLoading();
            this.switchTabHighlight('map');
            Backbone.history.navigate($(e.currentTarget).attr('href'), { trigger: false });
        },

        switchTabHighlight: function (tab) {
            this.activeTab = tab;
            this.$el.find('.tab-image-button').removeClass('selected');
            this.$el.find('.' + tab + '-view-button').addClass('selected');
            this.$el.find('.tabs li a, .tabs .tab-list-button').tipsy.revalidate();
        },

        showSortMenu: function (e) {
            app.vent.trigger('close-sub-menus');
            $(e.currentTarget).addClass('active');
            e.stopPropagation();
        },

        changeSort: function (e) {
            e.preventDefault();
            this.$el.find('.sort-button .tab-list-selection').empty().text($(e.currentTarget).text());
            app.vent.trigger('close-sub-menus');
            this.clearListAnPrepareShowLoading();
            this.collection.changeSort($(e.currentTarget).data('sort'));
            Backbone.history.navigate($(e.currentTarget).attr('href'), { trigger: false });
            return false;
        },

        clearListAnPrepareShowLoading: function () {
            this.$el.find('.stream-message, .stream-load-more, .stream-load-new, .stream-loading, .no-more-items').remove();
            this.$el.find('.sighting-items').empty();
            this.$el.find('.sighting-list').infinitescroll('destroy');
        },

        showLoading: function () {
            this.$el.find('.stream-message, .stream-load-new, .stream-load-more, .no-more-items').remove();
            this.$el.find('.sighting-items, .tab-bar-right').hide();
            this.onLoadingStart();
        },

        showHideSearch: function () {
            this.trigger('toggle-search');
        },

        beforeClose: function () {
            this.$el.find('.sighting-list').infinitescroll('destroy');
        }
    });

    return SightingListView;

});