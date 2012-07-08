﻿/* Bowerbird V1 - Licensed under MIT 1.1 Public License

 Developers: 
 * Frank Radocaj : frank@radocaj.com
 * Hamish Crittenden : hamish.crittenden@gmail.com
 
 Project Manager: 
 * Ken Walker : kwalker@museum.vic.gov.au
 
 Funded by:
 * Atlas of Living Australia
 
*/

// attribution: https://raw.github.com/SignalR/SignalR.Ninject/master/SignalR.Ninject/NinjectDependencyResolver.cs				

using System;
using System.Linq;
using System.Collections.Generic;
using Ninject;
using SignalR;

namespace Bowerbird.Web.Config
{
    public class SignalrNinjectDependencyResolver : DefaultDependencyResolver
    {
        private readonly IKernel _kernel;

        public SignalrNinjectDependencyResolver(IKernel kernel)
        {
            if (kernel == null)
            {
                throw new ArgumentNullException("kernel");
            }

            _kernel = kernel;
        }

        public override object GetService(Type serviceType)
        {
            if (serviceType == typeof(IConnectionManager)) // HACK: Need to investigate why I get a stack overflow if I don't have this if statement here
            {
                return base.GetService(serviceType);
            }
            else
            {
                return _kernel.TryGet(serviceType) ?? base.GetService(serviceType);
            }
        }

        public override IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType).Concat(base.GetServices(serviceType));
        }
    }
}