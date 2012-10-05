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

using System.Linq;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.Indexes;
using Bowerbird.Core.Repositories;
using Raven.Client;
using Raven.Client.Linq;

namespace Bowerbird.Core.CommandHandlers
{
    public class UserRequestPasswordResetCommandHandler : ICommandHandler<UserRequestPasswordResetCommand>
    {
        #region Members

        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public UserRequestPasswordResetCommandHandler(
            IDocumentSession documentSession)
        {
            Check.RequireNotNull(documentSession, "documentSession");

            _documentSession = documentSession;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void Handle(UserRequestPasswordResetCommand userRequestPasswordResetCommand)
        {
            Check.RequireNotNull(userRequestPasswordResetCommand, "userRequestPasswordResetCommand");

            var user = _documentSession
                .Query<All_Users.Result, All_Users>()
                .AsProjection<All_Users.Result>()
                .Where(x => x.Email == userRequestPasswordResetCommand.Email)
                .First()
                .User;
            
            user.RequestPasswordReset();

            _documentSession.Store(user);
        }

        #endregion      
    }
}
