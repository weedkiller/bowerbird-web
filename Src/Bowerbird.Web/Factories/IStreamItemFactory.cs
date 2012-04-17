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
using Bowerbird.Web.ViewModels;
using System.Collections.Generic;
using Bowerbird.Core.DomainModels;

namespace Bowerbird.Web.Factories
{
    public interface IStreamItemFactory : IFactory
    {
        StreamItem Make(object item, IEnumerable<string> groups, string contributionType, User groupUser, DateTime groupCreatedDateTime, string description);
    }
}