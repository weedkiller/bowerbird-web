define(["jquery","underscore","backbone","app","collections/paginatedcollection","models/streamitem","models/user","models/project","date"],function(a,b,c,d,e,f,g,h){var i=e.extend({model:f,baseUrl:"/stream",groupOrUser:null,initialize:function(){b.bindAll(this,"onSuccess","onSuccessWithAddFix","getFetchOptions"),e.prototype.initialize.apply(this,arguments),typeof options.groupOrUser!="undefined"||(this.groupOrUser=options.groupOrUser)},comparator:function(a){return-parseInt(a.get("CreatedDateTimeOrder"))},fetchFirstPage:function(){this.firstPage(this.getFetchOptions(!0))},fetchNextPage:function(){this.nextPage(this.getFetchOptions(!0))},getFetchOptions:function(a){var b={data:{},add:a,success:null};return a?b.success=this.onSuccess:b.success=this.onSuccessWithAddFix,this.groupOrUser&&(this.groupOrUser instanceof h?b.data.groupId=this.groupOrUser.id:this.groupOrUser instanceof g&&(b.data.userId=this.groupOrUser.id)),b},onSuccess:function(a,b){},onSuccessWithAddFix:function(a,b){this.onSuccess
(a,b);var c=this;b.each(function(a,b){c.trigger("add",a,c,{Index:b})})}});return i})