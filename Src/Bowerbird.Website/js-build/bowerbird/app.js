define(["jquery","underscore","backbone","signalr","bootstrap-data","models/user","collections/usercollection","collections/projectcollection","collections/teamcollection","collections/organisationcollection","marionette"],function(a,b,c,d,e,f,g,h,i,j){var k=new c.Marionette.Application;window.Bowerbird=window.Bowerbird||{},window.Bowerbird.version="1.0.0",window.Bowerbird.app=k;var l=function(a){this.user=new f(a.User),this.memberships=a.Memberships,this.projects=new h(a.Projects),this.teams=new i(a.Teams),this.organisations=new j(a.Organisations),this.appRoot=a.Application,this.hasGroupPermission=function(a,c){var d=b.find(this.memberships,function(b){return b.GroupId===a});return d?b.any(d.PermissionIds,function(a){return a===c}):!1}};return k.addRegions({header:"header",footer:"footer",sidebar:"#sidebar",content:"#content",notifications:"#notifications",usersonline:"#onlineusers"}),k.bind("initialize:before",function(){k.onlineUsers=new g,e.AuthenticatedUser&&(k.authenticatedUser=new 
l(e.AuthenticatedUser)),e.OnlineUsers&&k.onlineUsers.add(e.OnlineUsers),k.prerenderedView={name:e.PrerenderedView,isBound:!1,data:e.Model}}),k.bind("initialize:after",function(){c.history&&c.history.start({pushState:!0}),a.connection.hub.start({transport:"longPolling"},function(){a.connection.activityHub.registerUserClient(k.authenticatedUser.user.id).done(function(){k.clientId=a.signalR.hub.id,log("connected as "+k.authenticatedUser.user.id+" with "+this.clientId)}).fail(function(a){log(a)})})}),k.isPrerendering=function(a){return a===k.prerenderedView.name&&!k.prerenderedView.isBound},k.setPrerenderComplete=function(){k.prerenderedView.isBound=!0},k.getShowViewMethodName=function(a){return k.isPrerendering(a)?"attachView":"show"},a(function(){k.start(e),a("body").click(function(){a(".sub-menu-button").removeClass("active")})}),k})