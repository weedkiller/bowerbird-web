﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System.ComponentModel.DataAnnotations;
using Bowerbird.Core.Validators;
using DataAnnotationsExtensions;

namespace Bowerbird.Core.ViewModels
{
    public class AccountUpdateInput
    {
        #region Members

        #endregion

        #region Constructors

        #endregion

        #region Properties

        [Required(ErrorMessage = "Please enter your name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter your email address")]
        [Email(ErrorMessage = "Please enter a valid email address")]
        [UniqueEmail(ErrorMessage = "The email address already exists, please enter another email address", IgnoreAuthenticatedUserEmail = true)]
        public string Email { get; set; }

        public string Description { get; set; }

        public string AvatarId { get; set; }

        public string BackgroundId { get; set; }

        [Required(ErrorMessage = "Please enter your preferred timezone")]
        public string Timezone { get; set; }

        [Required(ErrorMessage = "Please enter your preferred licencing")]
        public string DefaultLicence { get; set; }

        #endregion

        #region Methods

        #endregion      
    }
}