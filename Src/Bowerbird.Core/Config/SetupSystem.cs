﻿/* Bowerbird V1 

 Licensed under MIT 1.1 Public License

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
using Bowerbird.Core.Commands;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels;
using Raven.Client;
using Bowerbird.Core.Events;
using Bowerbird.Core.Config;
using System.Threading;
using Bowerbird.Core.Services;
using System.IO;

namespace Bowerbird.Core.Config
{
    public class SetupSystem
    {

        #region Members

        private readonly IDocumentSession _documentSession;
        private readonly ISystemStateManager _systemStateManager;
        private readonly IConfigService _configService;

        private readonly string[] _speciesFileHeaderColumns = {
                                                                  "Kingdom", 
                                                                  "Group Name", 
                                                                  "Species Common Names", 
                                                                  "Taxonomy", 
                                                                  "Order", 
                                                                  "Family", 
                                                                  "Genus", 
                                                                  "Species",
                                                                  "Synonym"
                                                              };

        #endregion

        #region Constructors

        public SetupSystem(
            IDocumentSession documentSession,
            ISystemStateManager systemStateManager,
            IConfigService configService)
        {
            Check.RequireNotNull(documentSession, "documentSession");
            Check.RequireNotNull(systemStateManager, "systemStateManager");
            Check.RequireNotNull(configService, "configService");

            _documentSession = documentSession;
            _systemStateManager = systemStateManager;
            _configService = configService;
        }

        #endregion

        #region Properties

        private AppRoot TheAppRoot { get; set; }

        private List<Permission> Permissions2 { get; set; }
        
        private List<Role> Roles { get; set; }

        private List<User> Users { get; set; }

        #endregion

        #region Methods

        public void Execute()
        {
            try
            {
                Permissions2 = new List<Permission>();
                Roles = new List<Role>();
                Users = new List<User>();

                // Create the temporary AppRoot to be used before the actual app root is created
                AddAppRoot();

                // Save the approot to be available for all subsequent setup
                _documentSession.SaveChanges();

                // Add permissions first
                AddPermissions();

                // Add roles, using permissions
                AddRoles();

                // Add species data
                AddSpecies();

                // Enable event & command processors to log user activity
                _systemStateManager.EnableCommandProcessor();
                _systemStateManager.EnableEventProcessor();

                // Add system admins
                AddAdminUsers();

                // Set the user now that we have one
                SetAppRootUser(Users[0].Id);

                // Save all system data now
                _documentSession.SaveChanges();

                // Wait for all stale indexes to complete.
                while (_documentSession.Advanced.DatabaseCommands.GetStatistics().StaleIndexes.Length > 0)
                {
                    Thread.Sleep(10);
                }

                // Enable all services
                _systemStateManager.EnableAllServices();
            }
            catch (Exception exception)
            {
                throw new Exception("Could not setup system", exception);
            }
        }

        private void AddAppRoot()
        {
            var categories = new[] 
            {
                "Birds", 
                "Fishes", 
                "Mammals", 
                "Amphibians", 
                "Reptiles", 
                "Invertebrates", 
                "Plants", 
                "Fungi", 
                "Others"
            };

            // Create the TempAppRoot to be used before the actual app root is created
            // Once the real temp app root is created, this one is no longer used
            TheAppRoot = new AppRoot(DateTime.Now, categories);
            _documentSession.Store(TheAppRoot);
        }

        private void SetAppRootUser(string userId)
        {
            TheAppRoot.SetCreatedByUser(Users.Single(x => x.Id == userId));
            _documentSession.Store(TheAppRoot);
        }

        private void AddPermissions()
        {
            AddPermission(PermissionNames.CreateOrganisation, "Create Organisations", "Ability to create organisations");
            AddPermission(PermissionNames.UpdateOrganisation, "Update Organisations", "Ability to update organisations");
            AddPermission(PermissionNames.DeleteOrganisation, "Delete Organisations", "Ability to delete organisations");
            AddPermission(PermissionNames.CreateTeam, "Create Teams", "Ability to create teams");
            AddPermission(PermissionNames.UpdateTeam, "Update Teams", "Ability to update teams");
            AddPermission(PermissionNames.DeleteTeam, "Delete Teams", "Ability to delete teams");
            AddPermission(PermissionNames.CreateProject, "Create Projects", "Ability to create projects");
            AddPermission(PermissionNames.UpdateProject, "Update Projects", "Ability to update projects");
            AddPermission(PermissionNames.DeleteProject, "Delete Projects", "Ability to delete projects");
            AddPermission(PermissionNames.CreateWatchlist, "Create Watchlists", "Ability to create watchlists");
            AddPermission(PermissionNames.UpdateWatchlist, "Update Watchlists", "Ability to update watchlists");
            AddPermission(PermissionNames.DeleteWatchlist, "Delete Watchlists", "Ability to delete watchlists");
            AddPermission(PermissionNames.CreateObservation, "Create Observations", "Ability to create observations");
            AddPermission(PermissionNames.UpdateObservation, "Update Observations", "Ability to update observations");
            AddPermission(PermissionNames.DeleteObservation, "Delete Observations", "Ability to delete observations");
            AddPermission(PermissionNames.CreatePost, "Create Posts", "Ability to create posts");
            AddPermission(PermissionNames.UpdatePost, "Update Posts", "Ability to update posts");
            AddPermission(PermissionNames.DeletePost, "Delete Posts", "Ability to delete posts");
            AddPermission(PermissionNames.CreateSpecies, "Create Species", "Ability to create species");
            AddPermission(PermissionNames.UpdateSpecies, "Update Species", "Ability to update species");
            AddPermission(PermissionNames.DeleteSpecies, "Delete Species", "Ability to delete species");
            AddPermission(PermissionNames.CreateReferenceSpecies, "Create Reference Species", "Ability to create reference species");
            AddPermission(PermissionNames.UpdateReferenceSpecies, "Update Reference Species", "Ability to update reference species");
            AddPermission(PermissionNames.DeleteReferenceSpecies, "Delete Reference Species", "Ability to delete reference species");
        }

        private void AddPermission(string id, string name, string description)
        {
            var permission = new Permission(id, name, description);

            _documentSession.Store(permission);

            Permissions2.Add(permission);
        }

        private void AddRoles()
        {
            AddRole("globaladministrator", "Global Administrator", "Administrator of Bowerbird",
                PermissionNames.CreateOrganisation,
                PermissionNames.UpdateOrganisation,
                PermissionNames.DeleteOrganisation,
                PermissionNames.CreateTeam,
                PermissionNames.UpdateTeam,
                PermissionNames.DeleteTeam,
                PermissionNames.CreateProject,
                PermissionNames.UpdateProject,
                PermissionNames.DeleteProject,
                PermissionNames.CreateSpecies,
                PermissionNames.UpdateSpecies,
                PermissionNames.DeleteSpecies);
            AddRole("globalmember", "Global Member", "Member of Bowerbird",
                PermissionNames.CreateObservation,
                PermissionNames.UpdateObservation,
                PermissionNames.DeleteObservation,
                PermissionNames.CreateProject,
                PermissionNames.UpdateProject,
                PermissionNames.DeleteProject);
            AddRole("organisationadministrator", "Organisation Administrator", "Administrator of an organisation",
                PermissionNames.UpdateOrganisation,
                PermissionNames.CreateTeam,
                PermissionNames.UpdateTeam,
                PermissionNames.DeleteTeam,
                PermissionNames.CreatePost,
                PermissionNames.UpdatePost,
                PermissionNames.DeletePost,
                PermissionNames.CreateReferenceSpecies,
                PermissionNames.UpdateReferenceSpecies,
                PermissionNames.DeleteReferenceSpecies);
            AddRole("teamadministrator", "Team Administrator", "Administrator of a team",
                PermissionNames.UpdateTeam,
                PermissionNames.CreateProject,
                PermissionNames.UpdateProject,
                PermissionNames.DeleteProject,
                PermissionNames.CreateReferenceSpecies,
                PermissionNames.UpdateReferenceSpecies,
                PermissionNames.DeleteReferenceSpecies);
            AddRole("teammember", "Team Member", "Member of a team",
                PermissionNames.CreatePost,
                PermissionNames.UpdatePost,
                PermissionNames.DeletePost);
            AddRole("projectadministrator", "Project Administrator", "Administrator of a project",
                PermissionNames.UpdateProject,
                PermissionNames.CreateReferenceSpecies,
                PermissionNames.UpdateReferenceSpecies,
                PermissionNames.DeleteReferenceSpecies);
            AddRole("projectmember", "Project Member", "Member of a project",
                PermissionNames.CreateObservation,
                PermissionNames.UpdateObservation,
                PermissionNames.DeleteObservation,
                PermissionNames.CreatePost,
                PermissionNames.UpdatePost,
                PermissionNames.DeletePost);
        }

        private void AddRole(string id, string name, string description, params string[] permissionIds)
        {
            var permissions = Permissions2.Where(x => permissionIds.Any(y => x.Id == "permissions/" + y));

            var role = new Role(id, name, description, permissions);

            _documentSession.Store(role);

            Roles.Add(role);
        }

        private void AddAdminUsers()
        {
            AddUser("password", "frank@radocaj.com", "Frank", "Radocaj", "globaladministrator", "globalmember");
            AddUser("password", "hcrittenden@museum.vic.gov.au", "Hamish", "Crittenden", "globaladministrator", "globalmember");
            AddUser("password", "kwalker@museum.vic.gov.au", "Ken", "Walker", "globaladministrator", "globalmember");
        }

        private void AddUser(string password, string email, string firstname, string lastname, params string[] roleIds)
        {
            var user = new User(password, email, firstname, lastname);
            _documentSession.Store(user);

            var member = new Member(
                user,
                user,
                TheAppRoot,
                Roles.Where(x => roleIds.Any(y => x.Id == "roles/" + y)));
            _documentSession.Store(member);

            user.AddMembership(member);
            _documentSession.Store(user);

            var userProject = new UserProject(user, DateTime.Now);
            userProject.SetAncestry(TheAppRoot);
            _documentSession.Store(userProject);

            var userProjectAssociation = new GroupAssociation(TheAppRoot, userProject, user, DateTime.Now);
            _documentSession.Store(userProjectAssociation);

            var userProjectmember = new Member(
                user, 
                user, 
                userProject, 
                Roles.Where(x => x.Id == "roles/projectadministrator" || x.Id == "roles/projectmember"));
            _documentSession.Store(userProjectmember);

            user.AddMembership(userProjectmember);
            _documentSession.Store(user);

            Users.Add(user);
        }

        private void AddSpecies()
        {
            var createdOn = DateTime.UtcNow;

            var speciesFromFiles = LoadSpeciesFilesFromFolder(Path.Combine(_configService.GetEnvironmentRootPath(), _configService.GetSpeciesRelativePath()));

            foreach (var species in speciesFromFiles)
            {
                _documentSession.Store(
                    new Species(
                        species[0],
                        species[1],
                        species[2].Split(',').ToArray(),
                        species[3],
                        species[4],
                        species[5],
                        species[6],
                        species[7],
                        species[8],
                        false,
                        createdOn
                        )
                    );
            }
        }

        private IEnumerable<List<string>> LoadSpeciesFilesFromFolder(string folderPath)
        {
            var species = new List<List<string>>();

            var fileList = Directory.GetFiles(folderPath);

            foreach (var file in fileList)
            {
                using (var reader = new StreamReader(File.OpenRead(file)))
                {
                    var fileHeaderColumns = reader.ReadLine().Split(new[] { '\t' }, StringSplitOptions.None).Take(_speciesFileHeaderColumns.Length);
                    var counter = 0;

                    foreach (var col in fileHeaderColumns)
                    {
                        if (!_speciesFileHeaderColumns[counter].ToLower().Equals(col.ToLower()))
                        {
                            throw new ApplicationException(
                                String.Format(
                                    "The header for column number {0} is {1} but should be {2} in species upload file {3}",
                                    counter + 1,
                                    col,
                                    _speciesFileHeaderColumns[counter],
                                    file
                                    ));
                        }
                        counter++;
                    }

                    while (reader.Peek() > 0)
                    {
                        var fieldValues = reader
                            .ReadLine()
                            .Split(new[] { '\t' }, StringSplitOptions.None)
                            .Take(_speciesFileHeaderColumns.Length);

                        species.Add(fieldValues.Select(x => x.Trim()).ToList());
                    }
                }
            }

            return species;
        }

        #endregion      
      
    }
}