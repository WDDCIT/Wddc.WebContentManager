using System;
using AutoMapper;
using Wddc.Core.Domain.EdiOrdering.Customers;
using Wddc.Core.Domain.EdiOrdering.Routes;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;

namespace Wddc.WebContentManager.Core.Domains.Customers.Mapper
{
    public class CustomerMapperConfigurations : IMapperConfiguration
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
                cfg.CreateMap<CustomerSetting, CustomerRouteDTO>()
                    .ForMember(x => x.CustomerSettingID, opt => opt.MapFrom(src => src.CustomerSettingID))
                    .ForMember(x => x.DelayedBilling, opt => opt.MapFrom(src => src.DelayedBillingID))
                    .ForMember(x => x.PetFood, opt => opt.MapFrom(src => src.PetFoodID))
                    .ForMember(x => x.PetFoodMoney, opt => opt.MapFrom(src => src.PetFoodMoney))
                    .ForMember(x => x.Id, opt => opt.MapFrom(src => src.RouteID))
                    .ForMember(x => x.RouteNumber, opt => opt.MapFrom(src => src.Route.RouteNumber))
                    .ForMember(x => x.Description, opt => opt.MapFrom(src => src.Route.Description))
                    .ForMember(x => x.HasShippingCharge, opt => opt.MapFrom(src => src.HasShippingCharge))
                    .ForMember(x => x.HasFinancialHold, opt => opt.MapFrom(src => src.HasFinancialHold));
            };
            return action;
        }
    }
}
