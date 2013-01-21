﻿/// <reference path="../../libs/log.js" />
/// <reference path="../../libs/require/require.js" />
/// <reference path="../../libs/jquery/jquery-1.7.2.js" />
/// <reference path="../../libs/underscore/underscore.js" />
/// <reference path="../../libs/backbone/backbone.js" />
/// <reference path="../../libs/backbone.marionette/backbone.marionette.js" />
/// <reference path="../models/observation.js" />

// ObservationController & ObservationRouter
// -----------------------------------------

define(['jquery', 'underscore', 'backbone', 'app', 'models/observation', 'models/sighting', 'models/identification', 'models/sightingnote', 'collections/sightingnotecollection', 'collections/identificationcollection',
        'views/observationdetailsview', 'views/observationformview', 'views/sightingidentificationformview', 'views/sightingnoteformview'],
function ($, _, Backbone, app, Observation, Sighting, Identification, SightingNote, SightingNoteCollection, IdentificationCollection, ObservationDetailsView, ObservationFormView, SightingIdentificationFormView, SightingNoteFormView) {
    var ObservationRouter = Backbone.Marionette.AppRouter.extend({
        appRoutes: {
            'observations/:id/createidentification': 'showSightingIdentificationCreateForm',
            'observations/:id/createnote': 'showSightingNoteCreateForm',
            'observations/:sightingId/updateidentification/:sightingNoteId': 'showSightingIdentificationUpdateForm',
            'observations/:sightingId/updatenote/:sightingNoteId': 'showSightingNoteUpdateForm',
            'observations/create*': 'showObservationCreateForm',
            'observations/:id/update': 'showObservationUpdateForm',
            'observations/:id': 'showObservationDetails'
        }
    });

    var ObservationController = {};

    var showObservationForm = function (uri) {
        $.when(getModel(uri, 'observations'))
            .done(function (model) {
                var observation = new Observation(model.Observation);

                var options = { model: observation, categorySelectList: model.CategorySelectList, categories: model.Categories, projectsSelectList: model.ProjectsSelectList };

                if (app.isPrerenderingView('observations')) {
                    options['el'] = '.observation-form';
                }

                var observationFormView = new ObservationFormView(options);
                app.showContentView('Edit Sighting', observationFormView, 'observations');
            });
    };

    var showSightingIdentificationForm = function (uri) {
        $.when(getModel(uri, 'identification'))
        .done(function (model) {
            var identification = new Identification(model.Identification);

            var options = { model: identification, sighting: new Sighting(model.Sighting), categories: model.Categories, categorySelectList: model.CategorySelectList };

            if (app.isPrerenderingView('identification')) {
                options['el'] = '.sighting-identification-form';
            }

            var sightingIdentificationFormView = new SightingIdentificationFormView(options);
            app.showContentView('Edit Identification', sightingIdentificationFormView, 'identification');
        });
    };

    var showSightingNoteForm = function (uri) {
        $.when(getModel(uri, 'sightingnotes'))
            .done(function (model) {
                var sightingNote = new SightingNote(model.SightingNote);

                var options = { model: sightingNote, sighting: new Sighting(model.Sighting), descriptionTypesSelectList: model.DescriptionTypesSelectList, categories: model.Categories, categorySelectList: model.CategorySelectList };

                if (app.isPrerenderingView('sightingnotes')) {
                    options['el'] = '.sighting-note-form';
                }

                var sightingNoteFormView = new SightingNoteFormView(options);
                app.showContentView('Edit Note', sightingNoteFormView, 'sightingnotes');
            });
    };

    var getModel = function (uri, viewName) {
        var deferred = new $.Deferred();
        if (app.isPrerenderingView(viewName)) {
            deferred.resolve(app.prerenderedView.data);
        } else {
            $.ajax({
                url: uri
            }).done(function (data) {
                deferred.resolve(data.Model);
            });
        }
        return deferred.promise();
    };

    // Public API
    // ----------

    ObservationController.showObservationDetails = function (id) {
        // Beacause IE is using hash fragments, we have to fix the id manually for IE
        var url = id;
        if (url.indexOf('observations') == -1) {
            url = '/observations/' + url;
        }

        $.when(getModel(url, 'observations'))
            .done(function (model) {
                var observation = new Sighting(model.Observation);

                var identifications = new IdentificationCollection(model.Observation.Identifications);
                var sightingNotes = new SightingNoteCollection(model.Observation.Notes);

                var options = {
                     model: observation,
                     identifications: identifications,
                     sightingNotes: sightingNotes
                };

                if (app.isPrerenderingView('observations')) {
                    options['el'] = '.observation';
                }

                var observationDetailsView = new ObservationDetailsView(options);
                app.showContentView(observation.get('Title'), observationDetailsView, 'observations');
            });
    };

    ObservationController.showObservationCreateForm = function (id) {
        var uri = '/observations/create';

        if (id) {
            var uriParts = [];
            if (id.projectid) {
                uriParts.push('projectid=' + id.projectid);
            }
            if (id.category) {
                uriParts.push('category=' + id.category);
            }
            uri += '?' + uriParts.join('&');
        }

        showObservationForm(uri);
    };

    ObservationController.showObservationUpdateForm = function (id) {
        showObservationForm('/observations/' + id + '/update');
    };

    ObservationController.mediaResourceUploaded = function (e, mediaResource) {
        app.vent.trigger('mediaResourceUploaded:', mediaResource);
    };

    ObservationController.showSightingIdentificationCreateForm = function (id) {
        showSightingIdentificationForm('/observations/' + id + '/createidentification');
    };

    ObservationController.showSightingIdentificationUpdateForm = function (sightingId, identificationId) {
        showSightingIdentificationForm('/observations/' + sightingId + '/updateidentification/' + identificationId);
    };

    ObservationController.showSightingNoteCreateForm = function (id) {
        showSightingNoteForm('/observations/' + id + '/createnote');
    };

    ObservationController.showSightingNoteUpdateForm = function (sightingId, sightingNoteId) {
        showSightingNoteForm('/observations/' + sightingId + '/updatenote/' + sightingNoteId);
    };

    // Event Handlers
    // --------------

    app.addInitializer(function () {
        this.observationRouter = new ObservationRouter({
            controller: ObservationController
        });
    });

    return ObservationController;
});