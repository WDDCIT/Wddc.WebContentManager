using System;
using AutoMapper;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;
using Wddc.Core.Domain.GreatPlains;
using Wddc.Core.Domain.EdiOrdering.Customers;

namespace Wddc.Data.GreatPlains.Mapper
{
    public class GreatPlainsMapperConfigurations : IMapperConfiguration
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
                cfg.CreateMap<RM00101, ContactInformationDTO>()
                    .ForMember(dest => dest.Address1, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.ADDRESS1))
                    .ForMember(dest => dest.Address2, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.ADDRESS2))
                    .ForMember(dest => dest.City, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.CITY))
                    .ForMember(dest => dest.ContactPerson, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.CNTCPRSN))
                    .ForMember(dest => dest.Country, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.COUNTRY))
                    .ForMember(dest => dest.Fax, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.FAX))
                    .ForMember(dest => dest.CustomerName, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.CUSTNAME))
                    .ForMember(dest => dest.CustomerId, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.CUSTNMBR))
                    .ForMember(dest => dest.State, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.STATE))
                    .ForMember(dest => dest.Phone1, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.PHONE1))
                    .ForMember(dest => dest.Phone2, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.PHONE2))
                    .ForMember(dest => dest.Phone3, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, string> opt) => opt.MapFrom(src => src.PHONE3))
                    //.ForMember(dest => dest.EmailAddresses, (IMemberConfigurationExpression<RM00101, ContactInformationDTO, IEnumerable<EmailAddressDTO>> opt) => opt.MapFrom(src => src.RM00106))
                    .ForMember(dest => dest.ShortName, opt => opt.MapFrom(src => src.SHRTNAME))
                    .ForMember(dest => dest.AddressCode, opt => opt.MapFrom(src => src.RM00106))
                    .ForMember(dest => dest.PrimaryShipToAddress, opt => opt.MapFrom(src => src.PRSTADCD))
                    .ForMember(dest => dest.PrimaryBillToAddress, opt => opt.MapFrom(src => src.PRBTADCD))
                    .ForMember(dest => dest.AddressCode, opt => opt.MapFrom(src => src.ADRSCODE));

                cfg.CreateMap<RM00106, EmailAddressDTO>()
                    .ForMember(dest => dest.DexRowId, opt => opt.MapFrom(src => src.DEX_ROW_ID))
                    .ForMember(dest => dest.EmailRecipient, opt => opt.MapFrom(src => src.Email_Recipient))
                    .ForMember(dest => dest.EmailType, opt => opt.MapFrom(src => (int)src.Email_Type));
            };

            return action;
        }
    }
}
