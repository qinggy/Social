﻿using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Framework.Core;
using Social.Domain;

namespace Social.Application
{
    public class ApplicationServicesRegistrar
    {
        private IDependencyResolver _dependencyResolver;
        public ApplicationServicesRegistrar(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public void RegisterServices()
        {
            _dependencyResolver.RegisterAssemblyByConvention(this.GetType().Assembly);
            _dependencyResolver.RegisterAssemblyByConvention(typeof(SiteDataContext).Assembly);
        }
    }
}