/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Linq;
using System.Web.Mvc;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels.Posts;
using Bowerbird.Core.Paging;
using Bowerbird.Web.Config;
using Bowerbird.Web.ViewModels.Members;
using Bowerbird.Web.ViewModels.Shared;
using Raven.Client;
using Raven.Client.Linq;

namespace Bowerbird.Web.Controllers.Members
{
    public class ProjectPostController : ControllerBase
    {

        #region Members

        private readonly ICommandProcessor _commandProcessor;
        private readonly IUserContext _userContext;
        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public ProjectPostController(
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
        public ActionResult List(ProjectPostListInput listInput)
        {
            return Json(MakeProjectPostList(listInput), JsonRequestBehavior.AllowGet);
        }

        [Transaction]
        [HttpPost]
        public ActionResult Create(ProjectPostCreateInput createInput)
        {
            if(!_userContext.HasProjectPermission(createInput.ProjectId, Permissions.CreateProjectPost))
            {
                return HttpUnauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeCreateCommand(createInput));

            return Json("success");
        }

        [Transaction]
        [HttpPut]
        public ActionResult Update(ProjectPostUpdateInput updateInput)
        {
            if (!_userContext.HasPermissionToUpdate<ProjectPost>(updateInput.Id))
            {
                return HttpUnauthorized();
            }
            
            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeUpdateCommand(updateInput));

            return Json("success");
        }

        [Transaction]
        [HttpDelete]
        public ActionResult Delete(IdInput deleteInput)
        {
            if(!_userContext.HasPermissionToDelete<ProjectPost>(deleteInput.Id))
            {
                return HttpUnauthorized();
            }

            if (!ModelState.IsValid)
            {
                return Json("Failure");
            }

            _commandProcessor.Process(MakeDeleteCommand(deleteInput));

            return Json("success");
        }

        private ProjectPostList MakeProjectPostList(ProjectPostListInput listInput)
        {
            RavenQueryStatistics stats;

            var results = _documentSession
                .Query<ProjectPost>()
                .Where(x => x.Project.Id == listInput.ProjectId)
                .Statistics(out stats)
                .Skip(listInput.Page)
                .Take(listInput.PageSize)
                .ToArray(); // HACK: Due to deferred execution (or a RavenDB bug) need to execute query so that stats actually returns TotalResults - maybe fixed in newer RavenDB builds

            return new ProjectPostList
            {
                ProjectId = listInput.ProjectId,
                Page = listInput.Page,
                PageSize = listInput.PageSize,
                Posts = results.ToPagedList(
                    listInput.Page,
                    listInput.PageSize,
                    stats.TotalResults,
                    null)
            };
        }

        private ProjectPostCreateCommand MakeCreateCommand(ProjectPostCreateInput createInput)
        {
            return new ProjectPostCreateCommand()
            {
                UserId = _userContext.GetAuthenticatedUserId(),
                ProjectId = createInput.ProjectId,
                MediaResources = createInput.MediaResources,
                Message = createInput.Message,
                Subject = createInput.Subject,
                Timestamp = createInput.Timestamp
            };
        }

        private ProjectPostDeleteCommand MakeDeleteCommand(IdInput deleteInput)
        {
            return new ProjectPostDeleteCommand()
            {
                UserId = _userContext.GetAuthenticatedUserId(),
                Id = deleteInput.Id
            };
        }

        private ProjectPostUpdateCommand MakeUpdateCommand(ProjectPostUpdateInput updateInput)
        {
            return new ProjectPostUpdateCommand()
            {
                UserId = _userContext.GetAuthenticatedUserId(),
                Id = updateInput.Id,
                MediaResources = updateInput.MediaResources,
                Message = updateInput.Message,
                Subject = updateInput.Subject,
                Timestamp = updateInput.Timestamp
            };
        }

        #endregion
    }
}