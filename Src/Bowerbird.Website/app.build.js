({
    baseUrl: "./",
    paths: {
        jquery: 'libs/jquery/jquery-1.7.2', // jQuery is now AMD compliant
        json2: 'libs/json/json2',
        underscore: 'libs/underscore/underscore',
        backbone: 'libs/backbone/backbone',
        marionette: 'libs/backbone.marionette/backbone.marionette',
        text: 'libs/require/text',
        noext: 'libs/require/noext',
        async: 'libs/require/async',
        goog: 'libs/require/goog',
        propertyParser: 'libs/require/propertyparser',
        ich: 'libs/icanhaz/icanhaz',
        jqueryui: 'libs/jqueryui',
        datepicker: 'libs/bootstrap/bootstrap-datepicker',
        date: 'libs/date/date',
        multiselect: 'libs/jquery.multiselect/jquery.multiselect',
        loadimage: 'libs/jquery.fileupload/load-image',
        fileupload: 'libs/jquery.fileupload/jquery.fileupload',
        signalr: 'libs/jquery.signalr/jquery.signalr',
        timeago: 'libs/jquery.timeago/jquery.timeago',
        'bootstrap-data': 'empty:'
    },
    name: "main",
    include: [
        'app', 
        'ich',
        'jquery',
        'json2',
        'underscore',
        'backbone',
        'marionette',
        'signalr',
        'routers/homerouter',
        'routers/groupuserrouter',
        'routers/observationrouter',
        'routers/projectrouter',
        'routers/postrouter',
        'routers/teamrouter',
        'routers/organisationrouter',
        'routers/speciesrouter',
        'routers/referencespeciesrouter',
        'routers/activityrouter',
        'views/headerview',
        'views/footerview',
        'views/sidebarlayoutview',
        'views/homelayoutview',
        'views/projectlayoutview',
        'views/observationlayoutview',
        'views/onlineuserscompositeview'
    ],
    locale: "en-us",
    optimize: "uglify",
    uglify: {
        toplevel: true,
        ascii_only: true,
        beautify: false,
        max_line_length: 1000
    },
    inlineText: true,
    useStrict: false,
    findNestedDependencies: true
})