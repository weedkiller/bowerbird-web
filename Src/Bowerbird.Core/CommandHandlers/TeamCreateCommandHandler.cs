/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

namespace Bowerbird.Core.CommandHandlers
{
    #region Namespaces

    using Bowerbird.Core.Commands;
    using Bowerbird.Core.DesignByContract;
    using Bowerbird.Core.DomainModels;
    using Bowerbird.Core.Repositories;

    #endregion

    public class TeamCreateCommandHandler : ICommandHandler<TeamCreateCommand>
    {
        #region Fields

        private readonly IRepository<Team> _teamRepository;
        private readonly IRepository<User> _userRepository;

        #endregion

        #region Constructors

        public TeamCreateCommandHandler(
            IRepository<Team> teamRepository
            ,IRepository<User> userRepository
            )
        {
            Check.RequireNotNull(userRepository, "userRepository");
            Check.RequireNotNull(teamRepository, "teamRepository");

            _userRepository = userRepository;
            _teamRepository = teamRepository;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public void Handle(TeamCreateCommand command)
        {
            Check.RequireNotNull(command, "command");

            var team = new Team(
                _userRepository.Load(command.UserId)
                , command.Name
                , command.Description
                , command.Website
                );

            _teamRepository.Add(team);
        }

        #endregion

    }
}