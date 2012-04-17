﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Indexes;
using Bowerbird.Core.Paging;
using Bowerbird.Core.Queries;
using Bowerbird.Web.Config;
using Bowerbird.Web.Factories;
using Bowerbird.Web.ViewModels;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Core.Config;

namespace Bowerbird.Web.Controllers
{
    public class OrganisationController : ControllerBase
    {
        #region Fields

        private readonly ICommandProcessor _commandProcessor;
        private readonly IUserContext _userContext;
        private readonly IDocumentSession _documentSession;
        private readonly IAvatarFactory _avatarFactory;
        private readonly IUsersGroupsHavingPermissionQuery _usersGroupsHavingPermissionQuery;

        #endregion

        #region Constructors

        public OrganisationController(
            ICommandProcessor commandProcessor,
            IUserContext userContext,
            IDocumentSession documentSession,
            IAvatarFactory avatarFactory,
            IUsersGroupsHavingPermissionQuery usersGroupsHavingPermissionQuery
            )
        {
            Check.RequireNotNull(commandProcessor, "commandProcessor");
            Check.RequireNotNull(userContext, "userContext");
            Check.RequireNotNull(documentSession, "documentSession");
            Check.RequireNotNull(avatarFactory, "avatarFactory");
            Check.RequireNotNull(usersGroupsHavingPermissionQuery, "usersGroupsHavingPermissionQuery");

            _commandProcessor = commandProcessor;
            _userContext = userContext;
            _documentSession = documentSession;
            _avatarFactory = avatarFactory;
            _usersGroupsHavingPermissionQuery = usersGroupsHavingPermissionQuery;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult Index(IdInput idInput)
        {
            if (_userContext.IsUserAuthenticated())
            {
                if (Request.IsAjaxRequest())
                {
                    return Json(MakeOrganisationIndex(idInput));
                }

                return View(MakeOrganisationIndex(idInput));
            }

            return RedirectToAction("List");
        }

        [HttpGet]
        public ActionResult List(OrganisationListInput listInput)
        {
            if (_userContext.IsUserAuthenticated())
            {
                if (listInput.HasAddTeamPermission)
                {
                    return new JsonNetResult(GetGroupsHavingAddTeamPermission());
                }

                if(Request.IsAjaxRequest())
                {
                    return new JsonNetResult(MakeOrganisationList(listInput));
                }
            }

            return View();
        }

        [Transaction]
        [Authorize]
        [HttpPost]
        public ActionResult Create(OrganisationCreateInput createInput)
        {
            if (!_userContext.HasAppRootPermission(PermissionNames.CreateOrganisation))
            {
                return HttpUnauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeOrganisationCreateCommand(createInput));

            return Json("Success");
        }

        [Transaction]
        [Authorize]
        [HttpPut]
        public ActionResult Update(OrganisationUpdateInput updateInput)
        {
            if (!_userContext.HasGroupPermission<Organisation>(PermissionNames.UpdateOrganisation, updateInput.Id))
            {
                return HttpUnauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeOrganisationUpdateCommand(updateInput));

            return Json("Success");
        }

        [Transaction]
        [Authorize]
        [HttpDelete]
        public ActionResult Delete(IdInput deleteInput)
        {
            if (!_userContext.HasGroupPermission<Organisation>(PermissionNames.DeleteOrganisation, deleteInput.Id))
            {
                return HttpUnauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeOrganisationDeleteCommand(deleteInput));

            return Json("Success");
        }

        private OrganisationIndex MakeOrganisationIndex(IdInput idInput)
        {
            Check.RequireNotNull(idInput, "idInput");

            var organisation = _documentSession.Load<Organisation>(idInput.Id);

            return new OrganisationIndex()
            {
                Name = organisation.Name,
                Description = organisation.Description,
                Website = organisation.Website,
                Avatar = _avatarFactory.GetAvatar(organisation)
            };
        }

        private OrganisationList MakeOrganisationList(OrganisationListInput listInput)
        {
            RavenQueryStatistics stats;

            var results = _documentSession
                .Query<Organisation>()
                .Statistics(out stats)
                .Skip(listInput.Page)
                .Take(listInput.PageSize)
                .ToList()
                .Select(organisation => new OrganisationView()
                    {
                        Id = organisation.Id,
                        Description = organisation.Description,
                        Name = organisation.Name,
                        Website = organisation.Website,
                        Avatar = _avatarFactory.GetAvatar(organisation)
                    });

            return new OrganisationList()
            {
                Page = listInput.Page,
                PageSize = listInput.PageSize,
                Organisations = results.ToPagedList(
                    listInput.Page,
                    listInput.PageSize,
                    stats.TotalResults,
                    null)
            };
        }

        private List<OrganisationView> GetGroupsHavingAddTeamPermission()
        {
            var loggedInUserId = _userContext.GetAuthenticatedUserId();

            return _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .Where(x =>
                    x.Id.In(_usersGroupsHavingPermissionQuery.GetUsersGroupsHavingPermission(loggedInUserId, "createteam")) &&
                    x.GroupType == "organisation"
                )
                .ToList()
                .Select(x => new OrganisationView()
                {
                    Id = x.Id,
                    Description = x.Organisation.Description,
                    Name = x.Organisation.Name,
                    Website = x.Organisation.Website,
                    Avatar = _avatarFactory.GetAvatar(x.Organisation)
                })
                .ToList();
        }

        private OrganisationCreateCommand MakeOrganisationCreateCommand(OrganisationCreateInput createInput)
        {
            return new OrganisationCreateCommand()
            {
                Description = createInput.Description,
                Name = createInput.Name,
                Website = createInput.Website,
                UserId = _userContext.GetAuthenticatedUserId(),
                AvatarId = createInput.AvatarId
            };
        }

        private OrganisationDeleteCommand MakeOrganisationDeleteCommand(IdInput deleteInput)
        {
            return new OrganisationDeleteCommand()
            {
                Id = deleteInput.Id,
                UserId = _userContext.GetAuthenticatedUserId()
            };
        }

        private OrganisationUpdateCommand MakeOrganisationUpdateCommand(OrganisationUpdateInput updateInput)
        {
            return new OrganisationUpdateCommand()
            {
                Description = updateInput.Description,
                Name = updateInput.Name,
                Website = updateInput.Website,
                UserId = _userContext.GetAuthenticatedUserId(),
                AvatarId = updateInput.AvatarId
            };
        }

        #endregion
    }
}