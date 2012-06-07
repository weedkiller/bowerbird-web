(function(){var a=function(){function a(a){return(""+a).replace(/&(?!\w+;)|[<>"']/g,function(a){return g[a]||a})}var b=Object.prototype.toString;Array.isArray=Array.isArray||function(a){return"[object Array]"==b.call(a)};var c=String.prototype.trim,d;if(c)d=function(a){return null==a?"":c.call(a)};else{var e,f;/\S/.test("\u00a0")?(e=/^[\s\xA0]+/,f=/[\s\xA0]+$/):(e=/^\s+/,f=/\s+$/),d=function(a){return null==a?"":a.toString().replace(e,"").replace(f,"")}}var g={"&":"&amp;","<":"&lt;",">":"&gt;",'"':"&quot;","'":"&#39;"},h={},i=function(){};return i.prototype={otag:"{{",ctag:"}}",pragmas:{},buffer:[],pragmas_implemented:{"IMPLICIT-ITERATOR":!0},context:{},render:function(a,b,c,d){d||(this.context=b,this.buffer=[]);if(this.includes("",a)){var a=this.render_pragmas(a),e=this.render_section(a,b,c);!1===e&&(e=this.render_tags(a,b,c,d));if(d)return e;this.sendLines(e)}else{if(d)return a;this.send(a)}},send:function(a){""!==a&&this.buffer.push(a)},sendLines:function(a){if(a)for(var a=a.split("\n"
),b=0;b<a.length;b++)this.send(a[b])},render_pragmas:function(a){if(!this.includes("%",a))return a;var b=this,c=this.getCachedRegex("render_pragmas",function(a,b){return RegExp(a+"%([\\w-]+) ?([\\w]+=[\\w]+)?"+b,"g")});return a.replace(c,function(a,c,d){if(!b.pragmas_implemented[c])throw{message:"This implementation of mustache doesn't understand the '"+c+"' pragma"};return b.pragmas[c]={},d&&(a=d.split("="),b.pragmas[c][a[0]]=a[1]),""})},render_partial:function(a,b,c){a=d(a);if(!c||void 0===c[a])throw{message:"unknown_partial '"+a+"'"};return!b||"object"!=typeof b[a]?this.render(c[a],b,c,!0):this.render(c[a],b[a],c,!0)},render_section:function(a,b,c){if(!this.includes("#",a)&&!this.includes("^",a))return!1;var d=this,e=this.getCachedRegex("render_section",function(a,b){return RegExp("^([\\s\\S]*?)"+a+"(\\^|\\#)\\s*(.+)\\s*"+b+"\n*([\\s\\S]*?)"+a+"\\/\\s*\\3\\s*"+b+"\\s*([\\s\\S]*)$","g")});return a.replace(e,function(a,e,f,g,h,i){var a=e?d.render_tags(e,b,c,!0):"",i=i?d.render(i,b,c,!0
):"",j,g=d.find(g,b);return"^"===f?j=!g||Array.isArray(g)&&0===g.length?d.render(h,b,c,!0):"":"#"===f&&(j=Array.isArray(g)?d.map(g,function(a){return d.render(h,d.create_context(a),c,!0)}).join(""):d.is_object(g)?d.render(h,d.create_context(g),c,!0):"function"==typeof g?g.call(b,h,function(a){return d.render(a,b,c,!0)}):g?d.render(h,b,c,!0):""),a+j+i})},render_tags:function(b,c,d,e){for(var f=this,g=function(){return f.getCachedRegex("render_tags",function(a,b){return RegExp(a+"(=|!|>|&|\\{|%)?([^#\\^]+?)\\1?"+b+"+","g")})},h=g(),i=function(b,e,i){switch(e){case"!":return"";case"=":return f.set_delimiters(i),h=g(),"";case">":return f.render_partial(i,c,d);case"{":case"&":return f.find(i,c);default:return a(f.find(i,c))}},b=b.split("\n"),j=0;j<b.length;j++)b[j]=b[j].replace(h,i,this),e||this.send(b[j]);if(e)return b.join("\n")},set_delimiters:function(a){a=a.split(" "),this.otag=this.escape_regex(a[0]),this.ctag=this.escape_regex(a[1])},escape_regex:function(a){return arguments.callee.sRE||
(arguments.callee.sRE=RegExp("(\\/|\\.|\\*|\\+|\\?|\\||\\(|\\)|\\[|\\]|\\{|\\}|\\\\)","g")),a.replace(arguments.callee.sRE,"\\$1")},find:function(a,b){function c(a){return!1===a||0===a||a}var a=d(a),e;if(a.match(/([a-z_]+)\./ig)){var f=this.walk_context(a,b);c(f)&&(e=f)}else c(b[a])?e=b[a]:c(this.context[a])&&(e=this.context[a]);return"function"==typeof e?e.apply(b):void 0!==e?e:""},walk_context:function(a,b){for(var c=a.split("."),d=void 0!=b[c[0]]?b:this.context,e=d[c.shift()];void 0!=e&&0<c.length;)d=e,e=e[c.shift()];return"function"==typeof e?e.apply(d):e},includes:function(a,b){return-1!=b.indexOf(this.otag+a)},create_context:function(a){if(this.is_object(a))return a;var b=".";this.pragmas["IMPLICIT-ITERATOR"]&&(b=this.pragmas["IMPLICIT-ITERATOR"].iterator);var c={};return c[b]=a,c},is_object:function(a){return a&&"object"==typeof a},map:function(a,b){if("function"==typeof a.map)return a.map(b);for(var c=[],d=a.length,e=0;e<d;e++)c.push(b(a[e]));return c},getCachedRegex:function(a,
b){var c=h[this.otag];c||(c=h[this.otag]={});var d=c[this.ctag];return d||(d=c[this.ctag]={}),(c=d[a])||(c=d[a]=b(this.otag,this.ctag)),c}},{name:"mustache.js",version:"0.4.0",to_html:function(a,b,c,d){var e=new i;d&&(e.send=d),e.render(a,b||{},c);if(!d)return e.buffer.join("\n")}}}();(function(){var b={VERSION:"0.10",templates:{},$:"undefined"!=typeof window?window.jQuery||window.Zepto||null:null,addTemplate:function(d,e){if("object"==typeof d)for(var f in d)this.addTemplate(f,d[f]);else b[d]?console.error("Invalid name: "+d+"."):b.templates[d]?console.error('Template "'+d+'  " exists'):(b.templates[d]=e,b[d]=function(e,f){var e=e||{},g=a.to_html(b.templates[d],e,b.templates);return b.$&&!f?b.$(g):g})},clearAll:function(){for(var a in b.templates)delete b[a];b.templates={}},refresh:function(){b.clearAll(),b.grabTemplates()},grabTemplates:function(){var a,d=document.getElementsByTagName("script"),e,f=[];for(a=0,l=d.length;a<l;a++)(e=d[a])&&e.innerHTML&&e.id&&("text/html"===e.type||"text/x-icanhaz"===
e.type)&&(b.addTemplate(e.id,"".trim?e.innerHTML.trim():e.innerHTML.replace(/^\s+/,"").replace(/\s+$/,"")),f.unshift(e));for(a=0,l=f.length;a<l;a++)f[a].parentNode.removeChild(f[a])}};"undefined"!=typeof require?module.exports=b:window.ich=b,"undefined"!=typeof document&&(b.$?b.$(function(){b.grabTemplates()}):document.addEventListener("DOMContentLoaded",function(){b.grabTemplates()},!0))})()})()