﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using Bowerbird.Core.DomainModels;
using Bowerbird.Web.ViewModels.Public;
using Bowerbird.Web.ViewModels.Shared;
using Raven.Client;

namespace Bowerbird.Web.Controllers.Public
{
    #region Namespaces

    using System;
    using System.Web.Mvc;

    using Bowerbird.Core;
    using Bowerbird.Core.Commands;
    using Bowerbird.Core.DesignByContract;
    using Bowerbird.Web.ViewModels;
    using Bowerbird.Web.Config;

    #endregion

    public class ObservationController : Controller
    {
        #region Members

        private readonly ICommandProcessor _commandProcessor;
        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public ObservationController(
            ICommandProcessor commandProcessor,
            IDocumentSession documentSession
            )
        {
            Check.RequireNotNull(commandProcessor, "commandProcessor");
            Check.RequireNotNull(documentSession, "documentSession");

            _commandProcessor = commandProcessor;
            _documentSession = documentSession;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult Index(IdInput idInput)
        {
            return View(MakeObservationIndex(idInput));
        }

        private ObservationIndex MakeObservationIndex(IdInput idInput)
        {
            return new ObservationIndex()
                       {
                           Observation = _documentSession.Load<Observation>(idInput.Id)
                       };
        }

        #endregion      
    }
}