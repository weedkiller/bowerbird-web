require.config({baseUrl:"/js/bowerbird",paths:{jquery:"http://ajax.googleapis.com/ajax/libs/jquery/1.7.2/jquery",json2:"/js/libs/json/json2",underscore:"/js/libs/underscore/underscore",backbone:"/js/libs/backbone/backbone",marionette:"/js/libs/backbone.marionette/backbone.marionette",text:"/js/libs/require/text",noext:"/js/libs/require/noext",async:"/js/libs/require/async",goog:"/js/libs/require/goog",propertyParser:"/js/libs/require/propertyparser",ich:"/js/libs/icanhaz/icanhaz",jqueryui:"/js/libs/jqueryui",datepicker:"/js/libs/bootstrap/bootstrap-datepicker",date:"/js/libs/date/date",multiselect:"/js/libs/jquery.multiselect/jquery.multiselect",loadimage:"/js/libs/jquery.fileupload/load-image",fileupload:"/js/libs/jquery.fileupload/jquery.fileupload",signalr:"/js/libs/jquery.signalr/jquery.signalr",timeago:"/js/libs/jquery.timeago/jquery.timeago"},priority:["ich","jquery","json2","underscore","backbone","marionette","signalr","routers/homerouter","routers/groupuserrouter","routers/observationrouter"
,"routers/projectrouter","routers/postrouter","routers/teamrouter","routers/organisationrouter","routers/speciesrouter","routers/referencespeciesrouter","routers/activityrouter","views/headerview","views/footerview","views/sidebarlayoutview","views/homelayoutview","views/projectlayoutview","views/observationlayoutview","views/onlineuserscompositeview"]}),require(["backbone","ich","marionette","/templates","noext!/signalr/hubs"],function(a,b){a.Marionette.Renderer.render=function(a,c){if(a)return b[a](c)}})