using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wddc.Core.Domain.GreatPlains;
using Wddc.Core.Domain.Sales;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;

namespace Wddc.Data.GreatPlains.Mapper
{
    public class SalesTypeMap : IMapperConfiguration
    {
        public int Order
        {
            get
            {
                return 0;
            }
        }

        public Action<IMapperConfigurationExpression> GetConfiguration()
        {
            Action<IMapperConfigurationExpression> action = cfg =>
            {
                cfg.CreateMap<SOP40200, SalesType>()
                    .ForMember(dest => dest.Description, opt => opt.MapFrom(src => String.IsNullOrEmpty(src.COMMNTID) ? src.DOCID.Trim() : src.COMMNTID.Trim()))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.DOCID.Trim()))
                    .ForMember(dest => dest.SOPTYPE, opt => opt.MapFrom(src => src.SOPTYPE))
                    .ForMember(dest => dest.PricingLevel, opt => opt.MapFrom(src => src.SOP40200_Options.PricingLevel))
                    .ForMember(dest => dest.SalesBatches, opt => opt.MapFrom(src => src.SOP40200_Options.SOP40200_SalesBatchOptions.Select(_ => _.SalesBatch)))
                    .ForMember(dest => dest.SalesBatch, opt => opt.MapFrom(src => src.SOP40200_Options.SalesBatch))
                    .ForMember(dest => dest.SopTypeDescription, opt => opt.MapFrom(src => src.SOPTYPE == 2 ? "2 - Order" : src.SOPTYPE == 3 ? "3 - Invoice" : src.SOPTYPE == 4 ? "4 - Credit" : src.SOPTYPE == 5 ? "5 - BackOrder" : "Undefined"));
            };

            return action;
        }

    }
}
