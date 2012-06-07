define(["jquery","underscore","backbone","app"],function(a,b,c,d){var e=c.Collection.extend({initialize:function(){b.extend(this,c.Events),typeof options!="undefined"||(options={}),this.page=1,typeof this.pageSize!="undefined"||(this.pageSize=10)},fetch:function(a){typeof a!="undefined"||(a={}),this.trigger("fetching",this);var b=this,d=a.success;return a.success=function(a){b.trigger("fetched",b),d&&d(b,a)},c.Collection.prototype.fetch.call(this,a)},parse:function(a){return this.page=a.Model.Page,this.pageSize=a.Model.PageSize,this.total=a.Model.TotalResultCount,a.Model.PagedListItems},url:function(){return this.baseUrl+"?"+a.param({page:this.page,pageSize:this.pageSize})},pageInfo:function(){var a={total:this.total,page:this.page,pageSize:this.pageSize,pages:Math.ceil(this.total/this.pageSize),prev:!1,next:!1},b=Math.min(this.total,this.page*this.pageSize);return this.total==this.pages*this.pageSize&&(b=this.total),a.range=[(this.page-1)*this.pageSize+1,b],this.page>1&&(a.prev=this.page-1
),this.page<a.pages&&(a.next=this.page+1),a},firstPage:function(a){return this.page=1,this.fetch(a)},nextPage:function(a){return this.pageInfo().next?(this.page=this.page+1,this.fetch(a)):!1},previousPage:function(){return this.pageInfo().prev?(this.page=this.page-1,this.fetch(options)):!1}});return e})