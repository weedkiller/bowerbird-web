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
using Bowerbird.Core.Extensions;
using Bowerbird.Core.Paging;
using Bowerbird.Web.Factories;
using Bowerbird.Web.ViewModels;
using Raven.Client;
using Raven.Client.Linq;

namespace Bowerbird.Web.Builders
{
    public class PostsViewModelBuilder : IPostsViewModelBuilder
    {
        #region Fields

        private readonly IDocumentSession _documentSession;
        private readonly IPostViewFactory _postViewFactory;

        #endregion

        #region Constructors

        public PostsViewModelBuilder(
            IDocumentSession documentSession,
            IPostViewFactory postViewFactory
        )
        {
            Check.RequireNotNull(documentSession, "documentSession");
            Check.RequireNotNull(postViewFactory, "postViewFactory");

            _documentSession = documentSession;
            _postViewFactory = postViewFactory;
        }

        #endregion

        #region Methods

        public object BuildItem(IdInput idInput)
        {
            Check.RequireNotNull(idInput, "idInput");

            return _postViewFactory.Make(_documentSession.Load<Post>(idInput.Id));
        }

        public object BuildList(PostListInput listInput)
        {
            RavenQueryStatistics stats;

            var posts = _documentSession
                .Query<Post>()
                .Where(x => x.GroupId == listInput.GroupId)
                .Include(x => x.GroupId)
                .OrderByDescending(x => x.CreatedOn)
                .Statistics(out stats)
                .Skip(listInput.Page.Or(Default.PageStart))
                .Take(listInput.PageSize.Or(Default.PageSize))
                .ToList()
                .Select(x => _postViewFactory.Make(x));

            return new
            {
                listInput.GroupId,
                Page = listInput.Page.Or(Default.PageStart),
                PageSize = listInput.PageSize.Or(Default.PageSize),
                List = posts.ToPagedList(
                    listInput.Page.Or(Default.PageStart),
                    listInput.PageSize.Or(Default.PageSize),
                    stats.TotalResults,
                    null)
            };
        }

        #endregion
    }
}