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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Web.Mvc;
using Bowerbird.Core.Commands;
using Bowerbird.Core.DomainModels;
using Bowerbird.Core.DomainModels.Members;
using Bowerbird.Core.DomainModels.Posts;
using Bowerbird.Test.Utils;
using Bowerbird.Web.Config;
using Bowerbird.Web.Controllers.Members;
using Bowerbird.Web.Indexes;
using Bowerbird.Web.ViewModels.Members;
using Bowerbird.Web.ViewModels.Shared;
using Moq;
using NUnit.Framework;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using Raven.Client.Embedded;
using Raven.Abstractions.Indexing;

namespace Bowerbird.Test.Controllers.Members
{
    [TestFixture]
    public class HomeControllerTest
    {
        #region Test Infrastructure

        private Mock<ICommandProcessor> _mockCommandProcessor;
        private Mock<IUserContext> _mockUserContext;
        private IDocumentStore _documentStore;
        private HomeController _controller;

        [SetUp]
        public void TestInitialize()
        {
            _mockCommandProcessor = new Mock<ICommandProcessor>();
            _documentStore = DocumentStoreHelper.TestDocumentStore();
            _mockUserContext = new Mock<IUserContext>();

            _controller = new HomeController(
                _mockCommandProcessor.Object,
                _mockUserContext.Object,
                _documentStore.OpenSession()
                );

            IndexCreation.CreateIndexes(typeof(StreamItem_WithParentIdAndUserIdAndCreatedDateTimeAndType).Assembly, _documentStore);
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
        [Category(TestCategory.Persistance)]
        public void Home_Index_Returns_HomeIndex_Containing_MenuItems_User_And_StreamItems()
        {
            var user = FakeObjects.TestUserWithId();
            var project = FakeObjects.TestProjectWithId();
            var team = FakeObjects.TestTeamWithId();

            var projectMember = new ProjectMember(user, project, user, FakeObjects.TestRoles());
            ((IAssignableId)projectMember).SetIdTo("members", "1");
            
            var teamMember = new TeamMember(user, team, user, FakeObjects.TestRoles());
            ((IAssignableId)teamMember).SetIdTo("members", "2");

            var streamItems = new List<StreamItem>();

            const int page = 1;
            const int pageSize = 10;

            using (var session = _documentStore.OpenSession())
            {
                session.Store(user);
                session.Store(project);
                session.Store(team);
                session.Store(projectMember);
                session.Store(teamMember);
                
                session.SaveChanges();

                for (var i = 0; i < 10; i++)
                {
                    var observation = new Observation(
                        user,
                        FakeValues.Title,
                        DateTime.Now.AddDays(i * -1),
                        FakeValues.Latitude,
                        FakeValues.Longitude,
                        FakeValues.Address,
                        FakeValues.IsTrue,
                        FakeValues.Category,
                        new List<MediaResource>() { FakeObjects.TestImageMediaResourceWithId((i).ToString()) }
                        );

                    ((IAssignableId)observation).SetIdTo("observations", i.ToString());

                    var observationNote = new ObservationNote(
                        user,
                        observation,
                        FakeValues.CommonName,
                        FakeValues.ScientificName,
                        FakeValues.Taxonomy,
                        FakeValues.Tags,
                        new Dictionary<string, string>() {{FakeValues.Description, FakeValues.Description}},
                        new Dictionary<string, string>() {{FakeValues.Description, FakeValues.Description}},
                        FakeValues.Notes,
                        DateTime.Now.AddDays(i * -1)
                        );

                    ((IAssignableId)observationNote).SetIdTo("observationnotes", i.ToString());

                    var projectPost = new ProjectPost(
                        project, 
                        user,
                        DateTime.Now.AddDays(i * -1), 
                        FakeValues.Subject,
                        FakeValues.Message,
                        new List<MediaResource>() { FakeObjects.TestImageMediaResourceWithId((i + 100).ToString()) });
                    
                    ((IAssignableId)projectPost).SetIdTo("posts", i.ToString());

                    var teamPost = new TeamPost(
                        team,
                        user,
                        DateTime.Now.AddDays(i*-1),
                        FakeValues.Subject,
                        FakeValues.Message,
                        new List<MediaResource>() {FakeObjects.TestImageMediaResourceWithId((i + 1000).ToString())}
                        );

                    ((IAssignableId)teamPost).SetIdTo("posts", (i + 100).ToString());

                    session.Store(observation);
                    session.Store(observationNote);
                    session.Store(projectPost);
                    session.Store(teamPost);
                    
                    var streamItem1 = new StreamItem(user, DateTime.Now.AddDays(i * -1), "Observation", observation.Id, observation, project.Id);
                    var streamItem2 = new StreamItem(user, DateTime.Now.AddDays(i * -1), "ObservationNote", observationNote.Id, observationNote, project.Id);
                    var streamItem3 = new StreamItem(user, DateTime.Now.AddDays(i * -1), "Post", projectPost.Id, projectPost, project.Id);
                    var streamItem4 = new StreamItem(user, DateTime.Now.AddDays(i * -1), "Post", teamPost.Id, teamPost, team.Id);

                    session.Store(streamItem1);
                    session.Store(streamItem2);
                    session.Store(streamItem3);
                    session.Store(streamItem4);

                    streamItems.Add(streamItem1);
                    streamItems.Add(streamItem2);
                    streamItems.Add(streamItem3);
                    streamItems.Add(streamItem4);
                }

                session.SaveChanges();
            }

            _controller.SetupAjaxRequest();

            var result = _controller.Index(new HomeIndexInput() { Page = page, PageSize = pageSize});

            Assert.IsNotNull(result);
            Assert.IsInstanceOf<JsonResult>(result);

            var jsonResult = result as JsonResult;
            Assert.IsNotNull(jsonResult);

            Assert.IsNotNull(jsonResult.Data);
            Assert.IsInstanceOf<HomeIndex>(jsonResult.Data);
            var jsonData = jsonResult.Data as HomeIndex;

            Assert.IsNotNull(jsonData);
            Assert.AreEqual(1, jsonData.ProjectMenu.Count());
            Assert.AreEqual(1, jsonData.TeamMenu.Count());
            Assert.AreEqual(streamItems.Count, jsonData.StreamItems.TotalResultCount);
            Assert.AreEqual(page, jsonData.StreamItems.SelectedPageNumber.Number);
            Assert.AreEqual(pageSize, jsonData.StreamItems.PageSize);
        }

        #endregion 
    }
}