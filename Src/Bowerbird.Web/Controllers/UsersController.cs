﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Web.Mvc;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Web.Builders;
using Bowerbird.Web.Config;
using Bowerbird.Web.ViewModels;
using Bowerbird.Core.Config;
using Bowerbird.Core.Extensions;
using Bowerbird.Core.DomainModels;

namespace Bowerbird.Web.Controllers
{
    [Restful]
    public class UsersController : ControllerBase
    {
        #region Members

        private readonly ICommandProcessor _commandProcessor;
        private readonly IUserContext _userContext;
        private readonly IUserViewModelBuilder _userViewModelBuilder;
        private readonly IStreamItemsViewModelBuilder _streamItemsViewModelBuilder;
        private readonly IProjectsViewModelBuilder _projectsViewModelBuilder;
        private readonly IPostsViewModelBuilder _postsViewModelBuilder;
        private readonly ITeamsViewModelBuilder _teamsViewModelBuilder;

        #endregion

        #region Constructors

        public UsersController(
            ICommandProcessor commandProcessor,
            IUserContext userContext,
            IUserViewModelBuilder userViewModelBuilder,
            IStreamItemsViewModelBuilder streamItemsViewModelBuilder,
            IProjectsViewModelBuilder projectsViewModelBuilder,
            IPostsViewModelBuilder postsViewModelBuilder,
            ITeamsViewModelBuilder teamsViewModelBuilder
            )
        {
            Check.RequireNotNull(commandProcessor, "commandProcessor");
            Check.RequireNotNull(userContext, "userContext");
            Check.RequireNotNull(userViewModelBuilder, "userViewModelBuilder");
            Check.RequireNotNull(streamItemsViewModelBuilder, "streamItemsViewModelBuilder");
            Check.RequireNotNull(projectsViewModelBuilder, "projectsViewModelBuilder");
            Check.RequireNotNull(postsViewModelBuilder, "postsViewModelBuilder");
            Check.RequireNotNull(teamsViewModelBuilder, "teamsViewModelBuilder");

            _commandProcessor = commandProcessor;
            _userContext = userContext;
            _userViewModelBuilder = userViewModelBuilder;
            _projectsViewModelBuilder = projectsViewModelBuilder;
            _postsViewModelBuilder = postsViewModelBuilder;
            _streamItemsViewModelBuilder = streamItemsViewModelBuilder;
            _teamsViewModelBuilder = teamsViewModelBuilder;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult Stream(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");
            
            var userId = pagingInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildUser(userId),
                StreamItems = _streamItemsViewModelBuilder.BuildUserStreamItems(pagingInput)
            };

            return View(Form.Stream);
        }

        [HttpGet]
        public ActionResult StreamList(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            return new JsonNetResult(_streamItemsViewModelBuilder.BuildUserStreamItems(pagingInput));
        }

        [HttpGet]
        public ActionResult Observations(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            var userId = pagingInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildUser(userId)
            };

            return View(Form.Observations);
        }

        [HttpGet]
        public ActionResult Projects(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            var userId = pagingInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildUser(userId),
                Projects = _projectsViewModelBuilder.BuildUserProjectList(pagingInput)
            };

            ViewBag.PrerenderedView = "projects"; // HACK: Need to rethink this

            return View(Form.Stream);
        }

        [HttpGet]
        public ActionResult Teams(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            var userId = pagingInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildUser(userId),
                Teams = _teamsViewModelBuilder.BuildUserTeamList(pagingInput)
            };

            ViewBag.PrerenderedView = "projects"; // HACK: Need to rethink this

            return View(Form.Stream);
        }

        [HttpGet]
        public ActionResult Posts(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            var userId = pagingInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildUser(userId),
                Posts = _postsViewModelBuilder.BuildUserPostList(pagingInput)
            };

            return View(Form.Posts);
        }

        //[HttpGet]
        //public ActionResult Following(PagingInput pagingInput)
        //{
        //    ViewBag.Model = new
        //    {
        //        User = _userViewModelBuilder.BuildUser(new IdInput() { Id = pagingInput.Id }),
        //        Following = _userViewModelBuilder.BuildUsersBeingFollowedByList(pagingInput)
        //    };

        //    return View(Form.Following);
        //}

        //[HttpGet]
        //public ActionResult Followers(PagingInput pagingInput)
        //{
        //    ViewBag.Model = new
        //    {
        //        User = _userViewModelBuilder.BuildUser(new IdInput() { Id = pagingInput.Id }),
        //        Followers = _userViewModelBuilder.BuildUsersFollowingList(pagingInput)
        //    };

        //    return View(Form.Followers);
        //}

        [HttpGet]
        public ActionResult About()
        {
            throw new System.NotImplementedException();
        }

        [HttpGet]
        public ActionResult Explore(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            ViewBag.UserList = _userViewModelBuilder.BuildUserList(pagingInput);

            return View(Form.List);
        }

        [HttpGet]
        public ActionResult GetOne(IdInput idInput)
        {
            Check.RequireNotNull(idInput, "idInput");

            var userId = idInput.Id.VerbosifyId<User>();

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildEditableUser(userId)
            };

            return new JsonNetResult(new
            {
                Model = ViewBag.Model
            });
        }

        [HttpGet]
        public ActionResult GetMany(PagingInput pagingInput)
        {
            Check.RequireNotNull(pagingInput, "pagingInput");

            return new JsonNetResult(_userViewModelBuilder.BuildUserList(pagingInput));
        }

        /// <summary>
        /// Placeholder Method: Keeping Restful Convention
        /// </summary>
        [HttpGet]
        public ActionResult CreateForm()
        {
            return RedirectToAction("Register", "Account");
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdateForm(IdInput idInput)
        {
            Check.RequireNotNull(idInput, "idInput");

            var userId = idInput.Id.VerbosifyId<User>();

#if !JS_COMBINE_MINIFY
    DebugToClient("SERVER: Users/UpdateForm userId:" + userId);
#endif

            ViewBag.Model = new
            {
                User = _userViewModelBuilder.BuildEditableUser(userId)
            };

            if (Request.IsAjaxRequest())
            {
                return new JsonNetResult(new
                {
                    Model = ViewBag.Model
                });
            }

            ViewBag.PrerenderedView = "users";

            return View(Form.Update);
        }

        /// <summary>
        /// Placeholder Method: Keeping Restful Convention
        /// </summary>
        [HttpGet]
        [Authorize]
        public ActionResult DeleteForm(IdInput idInput)
        {
            Check.RequireNotNull(idInput, "idInput");

            var userId = idInput.Id.VerbosifyId<User>();

            ViewBag.User = _userViewModelBuilder.BuildUser(userId);

            return View(Form.Delete);
        }

        /// <summary>
        /// Placeholder Method: Keeping Restful Convention
        /// </summary>
        [HttpPost]
        [Authorize]
        public ActionResult Create()
        {
            return HttpNotFound();
        }

        [HttpPut]
        [Authorize]
        [Transaction]
        public ActionResult Update(UserUpdateInput userUpdateInput)
        {
            Check.RequireNotNull(userUpdateInput, "userUpdateInput");

            var userId = userUpdateInput.Id.VerbosifyId<User>();

            if (!_userContext.HasUserPermission(userId))
            {
                return HttpUnauthorized();
            }

            if (ModelState.IsValid)
            {
                _commandProcessor.Process(
                    new UserUpdateCommand()
                    {
                        Id = userUpdateInput.Id,
                        FirstName = userUpdateInput.FirstName,
                        LastName = userUpdateInput.LastName,
                        Email = userUpdateInput.Email,
                        Description = userUpdateInput.Description,
                        AvatarId = userUpdateInput.AvatarId
                    });

                return RedirectToAction("index", "home");
            }

            ViewBag.User = new
            {
                userUpdateInput.Id,
                userUpdateInput.AvatarId,
                userUpdateInput.Description,
                userUpdateInput.Email,
                userUpdateInput.FirstName,
                userUpdateInput.LastName
            };

            return View(Form.Update);
        }

        /// <summary>
        /// Placeholder Method: Keeping Restful Convention
        /// </summary>
        [HttpDelete]
        [Authorize]
        public ActionResult Delete()
        {
            return HttpNotFound();
        }

        [HttpGet]
        [Authorize]
        public ActionResult ChangePassword()
        {
            return View(Form.ChangePassword);
        }

        [HttpPost]
        [Authorize]
        [Transaction]
        public ActionResult ChangePassword(AccountChangePasswordInput accountChangePasswordInput)
        {
            if (ModelState.IsValid)
            {
                _commandProcessor.Process(
                    new UserUpdatePasswordCommand()
                    {
                        UserId = _userContext.GetAuthenticatedUserId(),
                        Password = accountChangePasswordInput.Password
                    });

                return RedirectToAction("index", "home");
            }

            return View(Form.ChangePassword);
        }

        #endregion
    }
}