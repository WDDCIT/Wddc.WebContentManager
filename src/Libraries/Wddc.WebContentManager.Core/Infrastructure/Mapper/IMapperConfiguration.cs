﻿using System;
using AutoMapper;

namespace Wddc.WebContentManager.Core.Infrastructure.Mapper
{
    /// <summary>
    /// Mapper configuration registrar interface
    /// </summary>
    public interface IMapperConfiguration
    {
        /// <summary>
        /// Get configuration
        /// </summary>
        /// <returns>Mapper configuration action</returns>
        Action<IMapperConfigurationExpression> GetConfiguration();

        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        int Order { get; }
    }
}
