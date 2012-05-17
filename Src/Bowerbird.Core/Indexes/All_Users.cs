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
using Bowerbird.Core.DomainModels;
using Raven.Client.Indexes;
using Raven.Abstractions.Indexing;
using System.Collections.Generic;
using Bowerbird.Core.DomainModels.DenormalisedReferences;

namespace Bowerbird.Core.Indexes
{
    public class All_Users : AbstractMultiMapIndexCreationTask<All_Users.Result>
    {
        public class Result
        {
            public string UserId { get; set; }
            public string[] MemberIds { get; set; }
            public DenormalisedGroupReference[] Groups { get; set; }
            public string[] RoleIds { get; set; }
            public string[] PermissionIds { get; set; }
            public User User { get; set; }
            public IEnumerable<Member> Memberships { get; set; }
        }

        public All_Users()
        {
            AddMap<Member>(members => from member in members 
                                      let roles = member.Roles
                                      let permissions = roles.SelectMany(x => x.Permissions)
                                        select new
                                        {
                                            UserId = member.User.Id,
                                            MemberIds = new [] { member.Id },
                                            Groups = new [] { member.Group },
                                            RoleIds = roles.Select(x => x.Id),
                                            PermissionIds = permissions.Select(x => x.Id)
                                        });

            Reduce = results => from result in results
                                group result by result.UserId
                                    into g
                                    select new
                                    {
                                        UserId = g.Key,
                                        MemberIds = g.SelectMany(x => x.MemberIds),
                                        Groups = g.SelectMany(x => x.Groups),
                                        RoleIds = g.SelectMany(x => x.RoleIds),
                                        PermissionIds = g.SelectMany(x => x.PermissionIds)
                                    };

            TransformResults = (database, results) =>
                                from result in results
                                let user = database.Load<User>(result.UserId)
                                let memberships = database.Load<Member>(result.MemberIds)
                                select new
                                {
                                    result.UserId,
                                    result.MemberIds,
                                    result.Groups,
                                    result.RoleIds,
                                    result.PermissionIds,
                                    User = user,
                                    Memberships = memberships
                                };

            Store(x => x.UserId, FieldStorage.Yes);
            Store(x => x.MemberIds, FieldStorage.Yes);
            Store(x => x.Groups, FieldStorage.Yes);
            Store(x => x.RoleIds, FieldStorage.Yes);
            Store(x => x.PermissionIds, FieldStorage.Yes);
        }
    }
}