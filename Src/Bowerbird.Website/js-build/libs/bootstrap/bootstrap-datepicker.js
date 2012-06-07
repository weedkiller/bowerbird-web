/* ===========================================================
 * bootstrap-datepicker.js v1.3.0
 * http://twitter.github.com/bootstrap/javascript.html#datepicker
 * ===========================================================
 * Copyright 2011 Twitter, Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 * Contributed by Scott Torborg - github.com/storborg
 * Loosely based on jquery.date_input.js by Jon Leighton, heavily updated and
 * rewritten to match bootstrap javascript approach and add UI features.
 * =========================================================== */

define(["jquery","date"],function(a){!function(a){function d(a){var b;for(b=0;b<c.length;b++)c[b]!=a&&c[b].hide()}function e(b,d){this.$el=a(b),this.proxy("show").proxy("ahead").proxy("hide").proxy("keyHandler").proxy("selectDate");var d=a.extend({},a.fn.datepicker.defaults,d);if(!!d.parse||!!d.format||!this.detectNative())a.extend(this,d),this.$el.data("datepicker",this),c.push(this),this.init()}var b="[data-datepicker]",c=[];e.prototype={detectNative:function(b){if(navigator.userAgent.match(/(iPad|iPhone); CPU(\ iPhone)? OS 5_\d/i)){var c=a("<span>").insertBefore(this.$el);return this.$el.detach().attr("type","date").insertAfter(c),c.remove(),!0}return!1},init:function(){var b=this.nav("months",1),c=this.nav("years",12),d=a("<div>").addClass("nav").append(b,c);this.$month=a(".name",b),this.$year=a(".name",c),$calendar=a("<div>").addClass("calendar");for(var e=0;e<this.shortDayNames.length;e++)$calendar.append('<div class="dow">'+this.shortDayNames[(e+this.startOfWeek)%7]+"</div>");this
.$days=a("<div>").addClass("days"),$calendar.append(this.$days),this.$picker=a("<div>").click(function(a){a.stopPropagation()}).mousedown(function(a){a.preventDefault()}).addClass("datepicker").append(d,$calendar).insertAfter(this.$el),this.$el.focus(this.show).click(this.show).change(a.proxy(function(){this.selectDate()},this)),this.selectDate(),this.hide()},nav:function(b,c){var d=a('<div><button type="button" class="prev button"><i></i> &larr;</button><span class="name"></span><button type="button" class="next button"><i></i> &rarr;</button></div>').addClass(b);return a(".prev",d).click(a.proxy(function(){this.ahead(-c,0)},this)),a(".next",d).click(a.proxy(function(){this.ahead(c,0)},this)),d},updateName:function(b,c){var d=b.find(".fg").text(),e=a("<div>").addClass("fg").append(c);b.empty();if(d!=c){var f=a("<div>").addClass("bg");b.append(f,e),f.fadeOut("slow",function(){a(this).remove()})}else b.append(e)},selectMonth:function(b){var c=new Date(b.getFullYear(),b.getMonth(),1);if(!
this.curMonth||this.curMonth.getFullYear()!=c.getFullYear()||this.curMonth.getMonth()!=c.getMonth()){this.curMonth=c;var d=this.rangeStart(b),e=this.rangeEnd(b),f=this.daysBetween(d,e);this.$days.empty();for(var g=0;g<=f;g++){var h=new Date(d.getFullYear(),d.getMonth(),d.getDate()+g,12,0),i=a("<div>").attr("date",this.format(h));i.text(h.getDate()),h.getMonth()!=b.getMonth()&&i.addClass("overlap"),this.$days.append(i)}this.updateName(this.$month,this.monthNames[b.getMonth()]),this.updateName(this.$year,this.curMonth.getFullYear()),a("div",this.$days).click(a.proxy(function(b){var c=a(b.target);this.update(c.attr("date")),c.hasClass("overlap")||this.hide()},this)),a("[date='"+this.format(new Date)+"']",this.$days).addClass("today")}a(".selected",this.$days).removeClass("selected"),a('[date="'+this.selectedDateStr+'"]',this.$days).addClass("selected")},selectDate:function(a){typeof a=="undefined"&&(a=this.parse(this.$el.val())),a||(a=new Date),this.selectedDate=a,this.selectedDateStr=this
.format(this.selectedDate),this.selectMonth(this.selectedDate)},update:function(a){this.$el.val(a).change()},show:function(b){b&&b.stopPropagation(),d(this);var c=this.$el.offset();this.$picker.show(),a("html").on("keydown",this.keyHandler)},hide:function(){this.$picker.hide(),a("html").off("keydown",this.keyHandler)},keyHandler:function(a){switch(a.keyCode){case 9:case 27:this.hide();return;case 13:this.update(this.selectedDateStr),this.hide();break;case 38:this.ahead(0,-7);break;case 40:this.ahead(0,7);break;case 37:this.ahead(0,-1);break;case 39:this.ahead(0,1);break;default:return}a.preventDefault()},parse:function(a){return Date.isValid(a,"d MMM yyyy")?Date.parseString(a,"d MMM yyyy"):null},format:function(a){return a.format("d MMM yyyy")},ahead:function(a,b){this.selectDate(new Date(this.selectedDate.getFullYear(),this.selectedDate.getMonth()+a,this.selectedDate.getDate()+b))},proxy:function(b){return this[b]=a.proxy(this[b],this),this},daysBetween:function(a,b){var a=Date.UTC(a.getFullYear
(),a.getMonth(),a.getDate()),b=Date.UTC(b.getFullYear(),b.getMonth(),b.getDate());return(b-a)/864e5},findClosest:function(a,b,c){var d=c*(Math.abs(b.getDay()-a-c*7)%7);return new Date(b.getFullYear(),b.getMonth(),b.getDate()+d)},rangeStart:function(a){return this.findClosest(this.startOfWeek,new Date(a.getFullYear(),a.getMonth()),-1)},rangeEnd:function(a){return this.findClosest((this.startOfWeek-1)%7,new Date(a.getFullYear(),a.getMonth()+1,0),1)}},a.fn.datepicker=function(a){return this.each(function(){new e(this,a)})},a(function(){a(b).datepicker(),a("html").click(d)}),a.fn.datepicker.DatePicker=e,a.fn.datepicker.defaults={monthNames:["January","February","March","April","May","June","July","August","September","October","November","December"],shortDayNames:["Sun","Mon","Tue","Wed","Thu","Fri","Sat"],startOfWeek:1}}(window.jQuery||window.ender)})