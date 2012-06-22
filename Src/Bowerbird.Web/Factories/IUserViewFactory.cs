﻿using System;
using Bowerbird.Core.DomainModels;
namespace Bowerbird.Web.Factories
{
    public interface IUserViewFactory : IFactory
    {
        object Make(User user);
    }
}