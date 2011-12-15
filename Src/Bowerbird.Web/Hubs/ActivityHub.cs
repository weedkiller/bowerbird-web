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

namespace Bowerbird.Web.Hubs
{
    #region Namespaces

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using SignalR.Hubs;
    using Raven.Client;

    using Bowerbird.Core.Entities;
    using Bowerbird.Core.EventHandlers;
    using Bowerbird.Core.Events;
    using Bowerbird.Core.DesignByContract;

    #endregion

    public class ActivityHub : Hub
    {

        #region Members

        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public ActivityHub(
            IDocumentSession documentSession)
        {
            Check.RequireNotNull(documentSession, "documentSession");

            _documentSession = documentSession;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void StartActivityStream()
        {
            throw new NotImplementedException();
        }

        #endregion      

    }
}