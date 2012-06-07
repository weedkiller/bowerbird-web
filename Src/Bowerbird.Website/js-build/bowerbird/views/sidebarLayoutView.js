define(["jquery","underscore","backbone","app","views/sidebarmenugroupcompositeview","views/sidebarprojectitemview","views/sidebarteamitemview","views/sidebarorganisationitemview"],function(a,b,c,d,e,f,g,h){var i=c.Marionette.Layout.extend({tagName:"section",id:"sidebar",className:"triple-1",template:"Sidebar",regions:{projectsMenu:".menu-projects",watchlistsMenu:"#watch-menu-group #watch-list",teamsMenu:".menu-teams",organisationsMenu:".menu-organisations"},events:{"click .menu-group-options .sub-menu-button":"showMenu","click .menu-group-options .sub-menu-button li":"selectMenuItem"},onRender:function(){a("article").prepend(this.el);var b=new e({id:"project-menu-group",collection:this.model.projects,type:"project",label:"Projects"});b.itemView=f,this.projectsMenu.show(b);if(this.model.teams.length>0){var c=new e({id:"team-menu-group",collection:this.model.teams,type:"team",label:"Teams"});c.itemView=g,this.teamsMenu.show(c)}if(this.model.organisations.length>0){var i=new e({id:"organisation-menu-group"
,collection:this.model.organisations,type:"organisation",label:"Organisations"});i.itemView=h,this.organisationsMenu.show(i)}var j=this;this.$el.find("a.user-stream").on("click",function(b){return b.preventDefault(),d.groupUserRouter.navigate(a(this).attr("href"),{trigger:!0}),!1}),this.$el.find("#project-menu-group-list a").on("click",function(b){return b.preventDefault(),d.projectRouter.navigate(a(this).attr("href"),{trigger:!0}),!1}),this.$el.find("#team-menu-group-list a").on("click",function(b){return b.preventDefault(),d.teamRouter.navigate(a(this).attr("href"),{trigger:!0}),!1}),this.$el.find("#organisation-menu-group-list a").on("click",function(b){return b.preventDefault(),d.organisationRouter.navigate(a(this).attr("href"),{trigger:!0}),!1})},serializeData:function(){return{Model:{User:this.model.user.toJSON(),Teams:this.model.teams.length>0,Organisations:this.model.organisations.length>0,AppRoot:this.model.appRoot!=null}}},showMenu:function(b){a(".sub-menu-button").removeClass
("active"),a(b.currentTarget).addClass("active"),b.stopPropagation()},selectMenuItem:function(b){a(".sub-menu-button").removeClass("active"),b.stopPropagation()}});return d.addInitializer(function(b){a(function(){if(d.authenticatedUser){var a={user:d.authenticatedUser.user,projects:d.authenticatedUser.projects,teams:d.authenticatedUser.teams,organisations:d.authenticatedUser.organisations,appRoot:d.authenticatedUser.appRoot},b=new i({model:a});b.on("show",function(){d.vent.trigger("sidebar:rendered")}),d.sidebar.show(b)}})}),i})