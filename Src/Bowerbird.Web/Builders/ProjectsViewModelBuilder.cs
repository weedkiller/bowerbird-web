﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Linq;
using Bowerbird.Core.Config;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Indexes;
using Bowerbird.Core.Paging;
using Bowerbird.Web.ViewModels;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Core.Factories;

namespace Bowerbird.Web.Builders
{
    public class ProjectsViewModelBuilder : IProjectsViewModelBuilder
    {
        #region Fields

        private readonly IDocumentSession _documentSession;
        private readonly IAvatarFactory _avatarFactory;

        #endregion

        #region Constructors

        public ProjectsViewModelBuilder(
            IDocumentSession documentSession,
            IAvatarFactory avatarFactory)
        {
            Check.RequireNotNull(documentSession, "documentSession");
            Check.RequireNotNull(avatarFactory, "avatarFactory");

            _documentSession = documentSession;
            _avatarFactory = avatarFactory;
        }

        #endregion

        #region Methods

        public object BuildProject()
        {
            return new
            {
                Name = "New Project",
                Description = "New Project",
                Website = "",
                Avatar = _avatarFactory.MakeDefaultAvatar(AvatarDefaultType.Project),
                MemberCount = 1
            };
        }

        public object BuildProject(string projectId)
        {
            Check.RequireNotNullOrWhitespace(projectId, "projectId");

            var project = _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .FirstOrDefault(x => x.GroupId == projectId);

            return MakeProject(project);
        }

        public object BuildProjectList(PagingInput pagingInput)
        {
            RavenQueryStatistics stats;

            return _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .Include(x => x.GroupId)
                .Where(x => x.GroupType == "project")
                .Statistics(out stats)
                .Skip(((pagingInput.Page -1) * pagingInput.PageSize))
                .Take(pagingInput.PageSize)
                .ToList()
                .Select(MakeProject)
                .ToPagedList(
                    pagingInput.Page,
                    pagingInput.PageSize,
                    stats.TotalResults);
        }

        /// <summary>
        /// PagingInput.Id is User.Id
        /// </summary>
        public object BuildUserProjectList(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            RavenQueryStatistics stats;

            var projects = _documentSession
                .Query<All_Users.Result, All_Users>()
                .AsProjection<All_Users.Result>()
                .Where(x => x.UserId == pagingInput.Id)
                .ToList()
                .SelectMany(x => x.Members.Where(y => y.Group.GroupType == "project").Select(y => y.Group.Id));

            return _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .Where(x => x.GroupId.In(projects))
                .Statistics(out stats)
                .Skip(pagingInput.Page)
                .Take(pagingInput.PageSize)
                .ToList()
                .Select(MakeProject)
                .ToPagedList(
                    pagingInput.Page,
                    pagingInput.PageSize,
                    stats.TotalResults);
        }

        /// <summary>
        /// PagingInput.Id is Team.Id
        /// </summary>
        public object BuildTeamProjectList(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            RavenQueryStatistics stats;

            var teamProjects = _documentSession
                .Query<GroupAssociation>()
                .Include(x => x.ChildGroup.Id)
                .Where(x => x.ParentGroup.Id == pagingInput.Id && x.ChildGroup.GroupType == "project");

            return _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .Where(x => x.GroupId.In(teamProjects.Select(y => y.ChildGroup.Id)))
                .Statistics(out stats)
                .Skip(pagingInput.Page)
                .Take(pagingInput.PageSize)
                .ToList()
                .Select(MakeProject)
                .ToPagedList(
                    pagingInput.Page,
                    pagingInput.PageSize,
                    stats.TotalResults);
        }

        /// <summary>
        /// PagingInput.Id is Project.Id
        /// </summary>
        public object BuildProjectUserList(PagingInput pagingInput)
        {
            RavenQueryStatistics stats;

            return _documentSession
                .Query<Member>()
                .Where(x => x.Group.Id == pagingInput.Id)
                .Customize(x => x.WaitForNonStaleResults())
                .Include(x => x.User.Id)
                .Statistics(out stats)
                .Skip(pagingInput.Page)
                .Take(pagingInput.PageSize)
                .ToList()
                .Select(x => MakeUser(x.User.Id))
                .ToPagedList(
                    pagingInput.Page,
                    pagingInput.PageSize,
                    stats.TotalResults);
        }

        private object MakeUser(string userId)
        {
            return MakeUser(_documentSession.Load<User>(userId));
        }

        private object MakeUser(User user)
        {
            return new
            {
                user.Avatar,
                user.Id,
                user.LastLoggedIn,
                Name = user.GetName()
            };
        }

        private object MakeProject(All_Groups.Result result)
        {
            var projectId = result.Project.Id.Replace("projects/", "");
            return new
            {
                Id = projectId,
                result.Project.Name,
                result.Project.Description,
                result.Project.Website,
                result.Project.Avatar,
                MemberCount = result.MemberIds.Count()
            };
        }

        #endregion
    }
}