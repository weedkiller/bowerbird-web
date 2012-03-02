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

using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Extensions;

namespace Bowerbird.Core.Events
{
    public class UserLoggedInEvent : IDomainEvent
    {

        #region Members

        #endregion

        #region Constructors

        public UserLoggedInEvent(
            User user)
        {
            Check.RequireNotNull(user, "user");

            User = user;
            EventMessage = user.GetName().AppendWith(" has logged in");
        }

        #endregion

        #region Properties

        public User User { get; private set; }

        public string EventMessage { get; private set; }

        #endregion

        #region Methods

        #endregion

    }
}
