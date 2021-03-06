﻿/* Bowerbird V1 

 Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Configuration;

namespace Bowerbird.Web.Config
{
    public class BowerbirdEmailConfigurationSection : ConfigurationSection
    {

        #region Members

        #endregion

        #region Constructors

        #endregion

        #region Properties

        [ConfigurationProperty("adminAccount", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string AdminAccount
        {
            get
            {
                return (string)this["adminAccount"];
            }
            set
            {
                this["adminAccount"] = value;
            }
        }

        [ConfigurationProperty("resetPasswordRelativeUri", DefaultValue = "", IsRequired = true, IsKey = false)]
        public string ResetPasswordRelativeUri
        {
            get
            {
                return (string)this["resetPasswordRelativeUri"];
            }
            set
            {
                this["resetPasswordRelativeUri"] = value;
            }
        }

        #endregion

        #region Methods

        #endregion

    }
}
