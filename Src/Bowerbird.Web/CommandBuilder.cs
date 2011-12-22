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

namespace Bowerbird.Web
{
    #region Namespaces

    using System;
    using Microsoft.Practices.ServiceLocation;
    using Bowerbird.Core.DesignByContract;
    using Bowerbird.Web.CommandFactories;

    #endregion

    public class CommandBuilder : ICommandBuilder
    {
        #region Members

        private readonly IServiceLocator _serviceLocator;

        #endregion

        #region Constructors

        public CommandBuilder(IServiceLocator serviceLocator)
        {
            Check.RequireNotNull(serviceLocator, "serviceLocator");

            _serviceLocator = serviceLocator;
        }

        #endregion

        #region Properties

        #endregion

        #region Methods

        public TCommand Build<TInput, TCommand>(TInput input, Action<TCommand> setup = null)
        {
            Check.RequireNotNull(input, "input");

            var commandFactory = _serviceLocator.GetInstance<ICommandFactory<TInput, TCommand>>();

            if (commandFactory == null)
            {
                throw new Exception("A command factory for the specified command type does not exist.");
            }

            var command = commandFactory.Make(input);

            if (setup != null)
            {
                setup(command);
            } 

            return command;
        }

        #endregion      
    }
}