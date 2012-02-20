﻿/* Bowerbird V1  - Licensed under MIT 1.1 Public License

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
using Bowerbird.Core.DomainModels;
using Bowerbird.Web.Controllers.Public;
using Bowerbird.Web.ViewModels.Shared;
using NUnit.Framework;
using Moq;
using Bowerbird.Test.Utils;
using Raven.Client;

namespace Bowerbird.Test.Controllers.Public
{
    [TestFixture]
    public class PostControllerTest
    {
        #region Test Infrastructure

        private IDocumentStore _documentStore;
        private PostController _controller;

        [SetUp]
        public void TestInitialize()
        {
            _documentStore = DocumentStoreHelper.InMemoryDocumentStore();

            _controller = new PostController(
                _documentStore.OpenSession());
        }

        [TearDown]
        public void TestCleanup()
        {
        }

        #endregion

        #region Test Helpers

        #endregion

        #region Constructor tests

        #endregion

        #region Property tests

        #endregion

        #region Method tests

        [Test]
        [Category(TestCategory.Unit)]
        public void Post_List_As_Json()
        {
            var user = FakeObjects.TestUserWithId();
            var project = FakeObjects.TestProjectWithId();
            var posts = new List<Post>();

            const int page = 1;
            const int pageSize = 10;

            using (var session = _documentStore.OpenSession())
            {
                session.Store(user);
                session.Store(project);
                session.SaveChanges();

                for (var i = 0; i < 15; i++)
                {
                    var post = FakeObjects.TestPostWithId(i.ToString());
                    post.AddGroupContribution(project, user, FakeValues.CreatedDateTime.AddDays(i*-1));
                    posts.Add(post);
                    session.Store(post);
                }

                session.SaveChanges();
            }

            var result = _controller.List(new PostListInput() { Page = page, PageSize = pageSize, GroupId = project.Id, UserId = user.Id });

            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = result as JsonResult;
            Assert.IsNotNull(jsonResult);

            Assert.IsInstanceOf<PostList>(jsonResult.Data);
            var jsonData = jsonResult.Data as PostList;

            Assert.IsNotNull(jsonData);
            Assert.AreEqual(page, jsonData.Page);
            Assert.AreEqual(pageSize, jsonData.PageSize);
            Assert.AreEqual(pageSize, jsonData.Posts.PagedListItems.Count());
            Assert.AreEqual(posts.Count, jsonData.Posts.TotalResultCount);
        }

        #endregion 
    }
}