﻿using Microsoft.Extensions.Options;
using Identity.STS.Identity.Configuration.Intefaces;

namespace Identity.STS.Identity.Configuration
{
    public class RootConfiguration : IRootConfiguration
    {      
        public IAdminConfiguration AdminConfiguration { get; set; }
        public IRegisterConfiguration RegisterConfiguration { get; }

        public RootConfiguration(IOptions<AdminConfiguration> adminConfiguration, IOptions<RegisterConfiguration> registerConfiguration)
        {
            RegisterConfiguration = registerConfiguration.Value;
            AdminConfiguration = adminConfiguration.Value;
        }
    }
}