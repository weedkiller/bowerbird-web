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
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.DomainModels.Members;
using Bowerbird.Core.Paging;
using Bowerbird.Web.Config;
using Bowerbird.Web.Indexes;
using Bowerbird.Web.ViewModels.Members;
using Bowerbird.Web.ViewModels.Shared;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Core.Extensions;

namespace Bowerbird.Web.Controllers.Members
{
    public class HomeController : ControllerBase
    {
        #region Members

        private readonly ICommandProcessor _commandProcessor;
        private readonly IUserContext _userContext;
        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public HomeController(
            ICommandProcessor commandProcessor,
            IUserContext userContext,
            IDocumentSession documentSession)
        {
            Check.RequireNotNull(commandProcessor, "commandProcessor");
            Check.RequireNotNull(userContext, "userContext");
            Check.RequireNotNull(documentSession, "documentSession");

            _commandProcessor = commandProcessor;
            _userContext = userContext;
            _documentSession = documentSession;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult Index(HomeIndexInput homeIndexInput)
        {
            if (Request.IsAjaxRequest())
            {
                return Json(MakeHomeIndex(homeIndexInput));
            }
            return View(MakeHomeIndex(homeIndexInput));
        }

        private HomeIndex MakeHomeIndex(HomeIndexInput indexInput)
        {
            // add menu id
            //List<string> userProjects;
            //List<string> userTeams;

            return new HomeIndex()
                       {
                           ProjectMenu = ProjectMenuForUser(_userContext.GetAuthenticatedUserId()/*, out userProjects*/),
                           TeamMenu = TeamMenuForUser(_userContext.GetAuthenticatedUserId()/*, out userTeams*/),
                           UserProfile = UserProfileForUser(_userContext.GetAuthenticatedUserId()),
                           StreamItems = StreamItemsForUser(new List<string>()
                                //.AddRangeFromList(userProjects)
                                //.AddRangeFromList(userTeams), 
                                ,indexInput.Page
                                ,indexInput.PageSize
                                )
                       };
        }

        private IEnumerable<MenuItem> ProjectMenuForUser(string userId/*, out List<string> projectIds*/)
        {
            return _documentSession
                .Advanced
                .LuceneQuery<ProjectMember>("ProjectMember/WithProjectIdAndUserId")
                .WhereEquals("UserId", userId)
                .WaitForNonStaleResults()
                .Select(
                    x =>
                    new MenuItem()
                        {
                            Id = x.Project.Id,
                            Name = x.Project.Name
                        });
            //.ExtractFromResults(x => x.Select(y => y.Id).ToList(), out projectIds);
        }

        private IEnumerable<MenuItem> TeamMenuForUser(string userId/*, out List<string> teamIds*/)
        {
            return _documentSession
                .Advanced
                .LuceneQuery<TeamMember>("TeamMember/WithTeamIdAndUserId")
                .WhereEquals("UserId", userId)
                .WaitForNonStaleResults()
                .Select(
                    x =>
                    new MenuItem()
                        {
                            Id = x.Team.Id,
                            Name = x.Team.Name
                        });
            //.ExtractFromResults(x => x.Select(y => y.Id).ToList(), out teamIds);
        }

        private PagedList<StreamItemViewModel> StreamItemsForUser(IEnumerable<string> userStreamParentIds, int page, int pageSize)
        {
            RavenQueryStatistics stats;

            var streamItems = _documentSession
                .Advanced
                .LuceneQuery<StreamItem>("StreamItem/WithParentIdAndUserIdAndCreatedDateTimeAndType")
                //.Query<StreamItem, StreamItem_WithStuff>()
                .Statistics(out stats)
                //.Where(x => x.ParentId.In(userStreamParentIds))
                .WhereContains("ParentId", userStreamParentIds)
                //.WaitForNonStaleResults()
                .Skip(page)
                .Take(pageSize)
                .Select(
                    x =>
                    new StreamItemViewModel()
                        {
                            Item = x.Item,
                            ItemId = x.ItemId,
                            ParentId = x.ParentId,
                            SubmittedOn = x.CreatedDateTime,
                            Type = x.Type,
                            UserId = x.User.Id
                        });

            return streamItems.ToPagedList(
                    page,
                    pageSize,
                    stats.TotalResults,
                    null);
        }

        private UserProfile UserProfileForUser(string userId)
        {
            var userProfile = _documentSession
                .Advanced
                .LuceneQuery<User>("User/WithUserIdAndEmail")
                .WhereContains("UserId", userId)
                .WaitForNonStaleResults()
                .Select(
                    x =>
                    new UserProfile()
                    {
                        Id = x.Id,
                        LastLoggedIn = x.LastLoggedIn,
                        Name = x.FirstName + " " + x.LastName
                    })
                .FirstOrDefault();

            return userProfile;
        }

        #endregion      
    }
}