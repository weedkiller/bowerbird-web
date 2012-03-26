﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/
				
using System;
using Bowerbird.Core.DomainModels;
using Newtonsoft.Json;

namespace Bowerbird.Web.ViewModels.Shared
{
    public class GroupChatMessage
    {
        #region Fields

        #endregion

        #region Constructors

        #endregion

        #region Properties

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [JsonProperty("user")]
        public UserProfile User { get; set; }

        [JsonProperty("target")]
        public UserProfile TargetUser { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        #endregion

        #region Methods

        #endregion
    }
}