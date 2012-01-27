/* Bowerbird V1  - Licensed under MIT 1.1 Public License

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
using Bowerbird.Core.Commands;
using Bowerbird.Core.DomainModels.Posts;
using Bowerbird.Web.Config;
using Bowerbird.Web.Controllers.Members;
using Bowerbird.Web.ViewModels;
using Bowerbird.Web.ViewModels.Members;
using Bowerbird.Web.ViewModels.Shared;
using NUnit.Framework;
using Moq;
using Bowerbird.Test.Utils;
using Raven.Client;

namespace Bowerbird.Test.Controllers.Members
{
    [TestFixture]
    public class ProjectPostControllerTest
    {
        #region Test Infrastructure

        private Mock<ICommandProcessor> _mockCommandProcessor;
        private Mock<IUserContext> _mockUserContext;
        private IDocumentStore _documentStore;
        private ProjectPostController _controller;

        [SetUp]
        public void TestInitialize()
        {
            _mockCommandProcessor = new Mock<ICommandProcessor>();
            _mockUserContext = new Mock<IUserContext>();
            _documentStore = DocumentStoreHelper.TestDocumentStore();

            _controller = new ProjectPostController(
                _mockCommandProcessor.Object,
                _mockUserContext.Object,
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

        #region Method tests

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_List_Returns_ProjectPostList_In_Json_Format()
        {
            var user = FakeObjects.TestUserWithId();
            var project = FakeObjects.TestProjectWithId();
            var projectPosts = new List<ProjectPost>();

            const int page = 1;
            const int pageSize = 10;

            using (var session = _documentStore.OpenSession())
            {
                session.Store(user);
                session.Store(project);
                session.SaveChanges();

                for (var i = 0; i < 15; i++)
                {
                    var projectPost = FakeObjects.TestProjectPostWithId(i.ToString());
                    projectPosts.Add(projectPost);
                    session.Store(projectPost);
                }

                session.SaveChanges();
            }

            var result = _controller.List(new ProjectPostListInput() { Page = page, PageSize = pageSize, ProjectId = project.Id, UserId = user.Id });

            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = result as JsonResult;
            Assert.IsNotNull(jsonResult);

            Assert.IsInstanceOf<ProjectPostList>(jsonResult.Data);
            var jsonData = jsonResult.Data as ProjectPostList;

            Assert.IsNotNull(jsonData);
            Assert.AreEqual(page, jsonData.Page);
            Assert.AreEqual(pageSize, jsonData.PageSize);
            Assert.AreEqual(pageSize, jsonData.Posts.PagedListItems.Count());
            Assert.AreEqual(projectPosts.Count, jsonData.Posts.TotalResultCount);
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Create_Passing_Invalid_Input_Returns_Json_Error()
        {
            _mockUserContext.Setup(x => x.HasProjectPermission(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            _controller.ModelState.AddModelError("Error", "Error");

            var result = _controller.Create(new ProjectPostCreateInput());

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Failure".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Create_Passing_Valid_Input_Returns_Json_Success()
        {
            _mockUserContext.Setup(x => x.HasProjectPermission(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

            var result = _controller.Create(new ProjectPostCreateInput());

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Success".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Create_Having_Invalid_Permission_Returns_HttpUnauthorised()
        {
            _mockUserContext.Setup(x => x.HasProjectPermission(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

            var result = _controller.Create(new ProjectPostCreateInput());

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Update_Passing_Invalid_Input_Returns_Json_Error()
        {
            _mockUserContext.Setup(x => x.HasPermissionToUpdate<ProjectPost>(It.IsAny<string>())).Returns(true);

            _controller.ModelState.AddModelError("Error", "Error");

            var result = _controller.Update(new ProjectPostUpdateInput());

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Failure".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Update_Passing_Valid_Input_Returns_Json_Success()
        {
            _mockUserContext.Setup(x => x.HasPermissionToUpdate<ProjectPost>(It.IsAny<string>())).Returns(true);

            var result = _controller.Update(new ProjectPostUpdateInput());

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Success".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Update_Having_Invalid_Permission_Returns_HttpUnauthorised()
        {
            _mockUserContext.Setup(x => x.HasPermissionToUpdate<ProjectPost>(It.IsAny<string>())).Returns(false);

            var result = _controller.Update(new ProjectPostUpdateInput());

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Delete_Passing_Invalid_Input_Returns_Json_Error()
        {
            _mockUserContext.Setup(x => x.HasPermissionToDelete<ProjectPost>(It.IsAny<string>())).Returns(true);

            _controller.ModelState.AddModelError("Error", "Error");

            var result = _controller.Delete(new IdInput() { Id = FakeValues.KeyString });

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Failure".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Delete_Passing_Valid_Input_Returns_Json_Success()
        {
            _mockUserContext.Setup(x => x.HasPermissionToDelete<ProjectPost>(It.IsAny<string>())).Returns(true);

            var result = _controller.Delete(new IdInput() { Id = FakeValues.KeyString });

            Assert.IsInstanceOf<JsonResult>(result);
            var jsonResult = result as JsonResult;

            Assert.IsNotNull(jsonResult);
            Assert.AreEqual(jsonResult.Data.ToString().ToLower(), "Success".ToLower());
        }

        [Test]
        [Category(TestCategory.Unit)]
        public void ProjectPost_Delete_Having_Invalid_Permission_Returns_HttpUnauthorised()
        {
            _mockUserContext.Setup(x => x.HasPermissionToDelete<ProjectPost>(It.IsAny<string>())).Returns(false);

            var result = _controller.Delete(new IdInput());

            Assert.IsInstanceOf<HttpUnauthorizedResult>(result);
        }

        #endregion
    }
}