﻿using System;
using System.Collections.Generic;
using Bowerbird.Core.DesignByContract;
using Bowerbird.Core.DomainModels.DenormalisedReferences;
using Bowerbird.Core.Events;

namespace Bowerbird.Core.DomainModels.Members
{
    public class TeamMember : Member
    {

        #region Members

        #endregion

        #region Constructors

        protected TeamMember()
            : base()
        {
            InitMembers();
        }

        public TeamMember(
            User createdByUser,
            Team team,
            User user,
            IEnumerable<Role> roles)
            : base(
            user,
            roles)
        {
            Check.RequireNotNull(createdByUser, "createdByUser");
            Check.RequireNotNull(team, "team");

            SetDetails(team, user);

            #if DEBUG
                Id = (new Random(System.DateTime.Now.Millisecond)).Next().ToString();
            #endif
            
            EventProcessor.Raise(new DomainModelCreatedEvent<TeamMember>(this, createdByUser));
        }

        #endregion

        #region Properties

        public DenormalisedNamedDomainModelReference<Team> Team { get; private set; }

        #endregion

        #region Methods

        private void InitMembers()
        {
        }

        private void SetDetails(Team team, User user)
        {
            Team = team;

            base.SetDetails(user);
        }

        #endregion

    }
}