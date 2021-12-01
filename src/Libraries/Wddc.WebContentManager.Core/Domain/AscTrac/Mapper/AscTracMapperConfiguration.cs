using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Wddc.Core.Domain.AscTrac.Carriers;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;

namespace Wddc.WebContentManager.Core.Domains.AscTrac.Mapper
{
    /// <summary>
    /// AutoMapper configuration for AscTrac tables
    /// </summary>
    public class AscTracMapperConfiguration : IMapperConfiguration
    {
        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Get configuration
        /// </summary>
        /// <returns>Mapper configuration action</returns>
        public Action<IMapperConfigurationExpression> GetConfiguration()
        {
            Action<IMapperConfigurationExpression> action = cfg =>
            {
                cfg.CreateMap<CARRIERACCT, CarrierAccountDTO>()
                    .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.ENTITY_ID.Replace("-MAIN", "")));
                cfg.CreateMap<CarrierAccountDTO, CARRIERACCT>();
                //cfg.CreateMap<CUSTNOTE, CustomerNoteDTO>();
                //cfg.CreateMap<CustomerNoteDTO, CUSTNOTE>();
                //cfg.CreateMap<CARRIER, Carrier>();
                //cfg.CreateMap<RTCUST, RouteCustomer>();
                //cfg.CreateMap<ROUTE, Route>();
            };

            return action;
        }
    }
}
