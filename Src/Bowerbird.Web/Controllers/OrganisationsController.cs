﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 Organisation Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 Funded by:
 * Atlas of Living Australia
 
*/

using System.Collections.Generic;
using System.Dynamic;
using System.Web.Mvc;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.Infrastructure;
using Bowerbird.Core.Queries;
using Bowerbird.Web.Config;
using Bowerbird.Core.ViewModels;
using Bowerbird.Core.Config;
using System;
using System.Linq;
using Bowerbird.Web.Infrastructure;
using Raven.Client;
using Raven.Client.Linq;
using Bowerbird.Core.Indexes;

namespace Bowerbird.Web.Controllers
{
    public class OrganisationsController : ControllerBase
    {
        #region Fields

        private readonly IMessageBus _messageBus;
        private readonly IUserContext _userContext;
        private readonly IOrganisationViewModelQuery _organisationViewModelQuery;
        private readonly IActivityViewModelQuery _activityViewModelQuery;
        private readonly IPostViewModelQuery _postViewModelQuery;
        private readonly IUserViewModelQuery _userViewModelQuery;
        private readonly IPermissionManager _permissionManager;
        private readonly IDocumentSession _documentSession;

        #endregion

        #region Constructors

        public OrganisationsController(
            IMessageBus messageBus,
            IUserContext userContext,
            IOrganisationViewModelQuery organisationViewModelQuery,
            IActivityViewModelQuery activityViewModelQuery,
            IPostViewModelQuery postViewModelQuery,
            IUserViewModelQuery userViewModelQuery,
            IPermissionManager permissionManager,
            IDocumentSession documentSession
            )
        {
            Check.RequireNotNull(messageBus, "messageBus");
            Check.RequireNotNull(userContext, "userContext");
            Check.RequireNotNull(organisationViewModelQuery, "organisationViewModelQuery");
            Check.RequireNotNull(activityViewModelQuery, "activityViewModelQuery");
            Check.RequireNotNull(postViewModelQuery, "postViewModelQuery");
            Check.RequireNotNull(userViewModelQuery, "userViewModelQuery");
            Check.RequireNotNull(permissionManager, "permissionManager");
            Check.RequireNotNull(documentSession, "documentSession");

            _messageBus = messageBus;
            _userContext = userContext;
            _organisationViewModelQuery = organisationViewModelQuery;
            _activityViewModelQuery = activityViewModelQuery;
            _postViewModelQuery = postViewModelQuery;
            _userViewModelQuery = userViewModelQuery;
            _permissionManager = permissionManager;
            _documentSession = documentSession;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        [HttpGet]
        public ActionResult Posts(string id, PostsQueryInput queryInput)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            queryInput.PageSize = 10;

            if (string.IsNullOrWhiteSpace(queryInput.Sort) ||
                (queryInput.Sort.ToLower() != "newest" &&
                queryInput.Sort.ToLower() != "oldest" &&
                queryInput.Sort.ToLower() != "a-z" &&
                queryInput.Sort.ToLower() != "z-a"))
            {
                queryInput.Sort = "newest";
            }

            queryInput.Query = queryInput.Query ?? string.Empty;
            queryInput.Field = queryInput.Field ?? string.Empty;

            var organisationResult = _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .First(x => x.GroupId == organisationId);

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildOrganisation(organisationId);
            viewModel.Posts = _postViewModelQuery.BuildGroupPostList(organisationId, queryInput);
            viewModel.UserCountDescription = "Member" + (organisationResult.UserCount == 1 ? string.Empty : "s");
            viewModel.PostCountDescription = "Post" + (organisationResult.PostCount == 1 ? string.Empty : "s");
            viewModel.Query = new
            {
                Id = organisationId,
                queryInput.Page,
                queryInput.PageSize,
                queryInput.Sort,
                queryInput.Query,
                queryInput.Field
            };
            viewModel.ShowPosts = true;
            viewModel.FieldSelectList = new[]
                {
                    new
                        {
                            Text = "Title",
                            Value = "title",
                            Selected = queryInput.Field.ToLower() == "title"
                        },
                    new
                        {
                            Text = "Body",
                            Value = "body",
                            Selected = queryInput.Field.ToLower() == "body"
                        }
                };

            return RestfulResult(
                viewModel,
                "organisations",
                "posts");
        }

        [HttpGet]
        public ActionResult Members(string id, UsersQueryInput queryInput)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            queryInput.PageSize = 15;

            if (string.IsNullOrWhiteSpace(queryInput.Sort) ||
                (queryInput.Sort.ToLower() != "a-z" &&
                queryInput.Sort.ToLower() != "z-a"))
            {
                queryInput.Sort = "a-z";
            }

            var organisationResult = _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .First(x => x.GroupId == organisationId);

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildOrganisation(organisationId);
            viewModel.Users = _userViewModelQuery.BuildGroupUserList(organisationId, queryInput);
            viewModel.Query = new
            {
                Id = organisationId,
                queryInput.Page,
                queryInput.PageSize,
                queryInput.Sort,
                queryInput.Query,
                queryInput.Field
            };
            viewModel.ShowMembers = true;
            viewModel.IsMember = _userContext.IsUserAuthenticated() ? _userContext.HasGroupPermission<Organisation>(PermissionNames.CreateObservation, organisationId) : false;
            viewModel.UserCountDescription = "Member" + (organisationResult.UserCount == 1 ? string.Empty : "s");
            viewModel.PostCountDescription = "Post" + (organisationResult.PostCount == 1 ? string.Empty : "s");

            return RestfulResult(
                viewModel,
                "organisations",
                "members");
        }

        [HttpGet]
        public ActionResult About(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            var organisationResult = _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .First(x => x.GroupId == organisationId);

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildOrganisation(organisationId);
            viewModel.ShowAbout = true;
            viewModel.IsMember = _userContext.IsUserAuthenticated() ? _userContext.HasGroupPermission<Organisation>(PermissionNames.CreateObservation, organisationId) : false;
            viewModel.UserCountDescription = "Member" + (organisationResult.UserCount == 1 ? string.Empty : "s");
            viewModel.PostCountDescription = "Post" + (organisationResult.PostCount == 1 ? string.Empty : "s");
            viewModel.OrganisationAdministrators = _userViewModelQuery.BuildGroupUserList(organisationId, "roles/" + RoleNames.OrganisationAdministrator);
            viewModel.ActivityTimeseries = CreateActivityTimeseries(organisationId);

            return RestfulResult(
                viewModel,
                "organisations",
                "about");
        }

        [HttpGet]
        public ActionResult Index(string id, ActivitiesQueryInput activityInput, PagingInput pagingInput)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            var organisationResult = _documentSession
                .Query<All_Groups.Result, All_Groups>()
                .AsProjection<All_Groups.Result>()
                .First(x => x.GroupId == organisationId);

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildOrganisation(organisationId);
            viewModel.Activities = _activityViewModelQuery.BuildGroupActivityList(organisationId, activityInput, pagingInput);
            viewModel.IsMember = _userContext.IsUserAuthenticated() ? _userContext.HasGroupPermission<Organisation>(PermissionNames.CreateObservation, organisationId) : false;
            viewModel.UserCountDescription = "Member" + (organisationResult.UserCount == 1 ? string.Empty : "s");
            viewModel.PostCountDescription = "Post" + (organisationResult.PostCount == 1 ? string.Empty : "s");
            viewModel.ShowActivities = true;

            return RestfulResult(
                viewModel,
                "organisations",
                "index");
        }

        [HttpGet]
        public ActionResult List(OrganisationsQueryInput queryInput)
        {
            queryInput.PageSize = 15;

            if (string.IsNullOrWhiteSpace(queryInput.Sort) ||
                (queryInput.Sort.ToLower() != "popular" &&
                queryInput.Sort.ToLower() != "newest" &&
                queryInput.Sort.ToLower() != "oldest" &&
                queryInput.Sort.ToLower() != "a-z" &&
                queryInput.Sort.ToLower() != "z-a"))
            {
                queryInput.Sort = "popular";
            }

            queryInput.Category = queryInput.Category ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(queryInput.Category) && !Categories.IsValidCategory(queryInput.Category))
            {
                queryInput.Category = string.Empty;
            }

            queryInput.Query = queryInput.Query ?? string.Empty;
            queryInput.Field = queryInput.Field ?? string.Empty;

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisations = _organisationViewModelQuery.BuildOrganisationList(queryInput);
            viewModel.CategorySelectList = Categories.GetSelectList(queryInput.Category);
            viewModel.Query = new
            {
                queryInput.Page,
                queryInput.PageSize,
                queryInput.Sort,
                queryInput.Category,
                queryInput.Query,
                queryInput.Field
            };
            viewModel.FieldSelectList = new[]
                {
                    new
                        {
                            Text = "Name",
                            Value = "name",
                            Selected = queryInput.Field.ToLower() == "name"
                        },
                    new
                        {
                            Text = "Description",
                            Value = "description",
                            Selected = queryInput.Field.ToLower() == "description"
                        }
                };

            if (_userContext.IsUserAuthenticated())
            {
                var user = _documentSession
                    .Query<All_Users.Result, All_Users>()
                    .AsProjection<All_Users.Result>()
                    .Where(x => x.UserId == _userContext.GetAuthenticatedUserId())
                    .Single();

                viewModel.ShowOrganisationExploreWelcome = user.User.CallsToAction.Contains("organisation-explore-welcome");
            }

            return RestfulResult(
                viewModel,
                "organisations",
                "list");
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateForm()
        {
            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildCreateOrganisation();
            viewModel.Create = true;
            viewModel.CategoriesSelectList = Categories.GetSelectList();

            return RestfulResult(
                viewModel,
                "organisations",
                "create");
        }

        [HttpGet]
        [Authorize]
        public ActionResult UpdateForm(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            if (!_userContext.HasGroupPermission(PermissionNames.UpdateOrganisation, organisationId))
            {
                return new HttpUnauthorizedResult();
            }

            var organisation = _documentSession.Load<Organisation>(organisationId);

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildUpdateOrganisation(organisationId);
            viewModel.Update = true;
            viewModel.CategoriesSelectList = Categories.GetSelectList(organisation.Categories.ToArray());

            return RestfulResult(
                viewModel,
                "organisations",
                "update");
        }

        [HttpGet]
        [Authorize]
        public ActionResult DeleteForm(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            // BUG: Fix this to check the parent groups' permission
            if (!_userContext.HasGroupPermission(PermissionNames.DeleteOrganisation, organisationId))
            {
                return new HttpUnauthorizedResult();
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = _organisationViewModelQuery.BuildOrganisation(organisationId);
            viewModel.Delete = true;

            return RestfulResult(
                viewModel,
                "organisations",
                "delete");
        }

        [Authorize]
        [HttpPost]
        public ActionResult UpdateMember(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return JsonFailed();
            }

            _messageBus.Send(
                new MemberUpdateCommand()
                {
                    UserId = _userContext.GetAuthenticatedUserId(),
                    GroupId = organisationId,
                    Roles = new[] { "roles/organisationmember" },
                    ModifiedByUserId = _userContext.GetAuthenticatedUserId()
                });

            return JsonSuccess();
        }

        [Authorize]
        [HttpDelete]
        public ActionResult DeleteMember(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            //// TODO: Not sure what this permission check is actually checking???
            //if (!_userContext.HasGroupPermission(PermissionNames.LeaveOrganisation, organisationId))
            //{
            //    return new HttpUnauthorizedResult();
            //}

            if (!ModelState.IsValid)
            {
                return JsonFailed();
            }

            _messageBus.Send(
                new MemberDeleteCommand()
                {
                    UserId = _userContext.GetAuthenticatedUserId(),
                    GroupId = organisationId,
                    ModifiedByUserId = _userContext.GetAuthenticatedUserId()
                });

            return JsonSuccess();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(OrganisationUpdateInput createInput)
        {
            if (ModelState.IsValid)
            {
                _messageBus.Send(
                    new OrganisationCreateCommand()
                        {
                            UserId = _userContext.GetAuthenticatedUserId(),
                            Name = createInput.Name,
                            Description = createInput.Description,
                            Website = createInput.Website,
                            AvatarId = createInput.AvatarId,
                            BackgroundId = createInput.BackgroundId,
                            Categories = createInput.Categories
                        });
            }
            else
            {
                Response.StatusCode = (int) System.Net.HttpStatusCode.BadRequest;
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = createInput;

            return RestfulResult(
                viewModel,
                "organisations",
                "create");
        }

        [HttpPut]
        [Authorize]
        public ActionResult Update(OrganisationUpdateInput updateInput)
        {
            string organisationId = VerbosifyId<Organisation>(updateInput.Id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            if (!_userContext.HasGroupPermission<Organisation>(PermissionNames.UpdateOrganisation, organisationId))
            {
                return new HttpUnauthorizedResult();
            }

            if (ModelState.IsValid)
            {
                _messageBus.Send(
                    new OrganisationUpdateCommand()
                    {
                        UserId = _userContext.GetAuthenticatedUserId(),
                        Id = organisationId,
                        Name = updateInput.Name,
                        Description = updateInput.Description,
                        Website = updateInput.Website,
                        AvatarId = updateInput.AvatarId,
                        BackgroundId = updateInput.BackgroundId,
                        Categories = updateInput.Categories
                    });
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
            }

            dynamic viewModel = new ExpandoObject();
            viewModel.Organisation = updateInput;

            return RestfulResult(
                viewModel,
                "organisations",
                "update");
        }

        [HttpDelete]
        [Authorize]
        public ActionResult Delete(string id)
        {
            string organisationId = VerbosifyId<Organisation>(id);

            if (!_permissionManager.DoesExist<Organisation>(organisationId))
            {
                return HttpNotFound();
            }

            // BUG: Fix this to check the parent groups' permission
            if (!_userContext.HasGroupPermission(PermissionNames.DeleteOrganisation, organisationId))
            {
                return new HttpUnauthorizedResult();
            }

            if (!ModelState.IsValid)
            {
                return JsonFailed();
            }

            _messageBus.Send(
                new OrganisationDeleteCommand
                {
                    Id = organisationId,
                    UserId = _userContext.GetAuthenticatedUserId()
                });

            return JsonSuccess();
        }

        private dynamic CreateActivityTimeseries(string organisationId)
        {
            var fromDate = DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)).Date;
            var toDate = DateTime.UtcNow.Date;

            var result = _documentSession
                .Advanced
                .LuceneQuery<All_Contributions.Result, All_Contributions>()
                .SelectFields<All_Contributions.Result>("ParentContributionId", "SubContributionId", "ParentContributionType", "SubContributionType", "CreatedDateTime")
                .WhereGreaterThan(x => x.CreatedDateTime, fromDate)
                .AndAlso()
                .WhereIn("GroupIds", new[] { organisationId })
                .AndAlso()
                .OpenSubclause()
                .WhereIn("ParentContributionType", new[] { "post" })
                .OrElse()
                .WhereIn("SubContributionType", new [] { "comment" })
                .CloseSubclause()
                .ToList();

            var contributions = result.Select(x => new
            {
                x.ParentContributionId,
                x.SubContributionId,
                x.ParentContributionType,
                x.SubContributionType,
                x.CreatedDateTime
            })
                .GroupBy(x => x.CreatedDateTime.Date);

            var timeseries = new List<dynamic>();

            for (DateTime dateItem = fromDate; dateItem <= toDate; dateItem = dateItem.AddDays(1))
            {
                string createdDateFormat;

                if (dateItem == fromDate ||
                    dateItem.Day == 1)
                {
                    createdDateFormat = "d MMM";
                }
                else
                {
                    createdDateFormat = "%d";
                }

                if (contributions.Any(x => x.Key.Date == dateItem.Date))
                {
                    timeseries.Add(contributions
                        .Where(x => x.Key.Date == dateItem.Date)
                        .Select(x => new
                        {
                            CreatedDate = dateItem.ToString(createdDateFormat),
                            PostCount = x.Count(y => y.ParentContributionType == "post"),
                            CommentCount = x.Count(y => y.SubContributionType == "comment")
                        }
                        ).First());
                }
                else
                {
                    timeseries.Add(new
                    {
                        CreatedDate = dateItem.ToString(createdDateFormat),
                        PostCount = 0,
                        CommentCount = 0
                    });
                }
            }
            return timeseries;
        }

        #endregion
    }
}