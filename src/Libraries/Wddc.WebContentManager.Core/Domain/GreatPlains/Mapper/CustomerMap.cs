using AutoMapper;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Wddc.WebContentManager.Core.Infrastructure.Mapper;
using Wddc.Core.Domain.GreatPlains;
using Wddc.Core.Domain.EdiOrdering.Customers;

namespace Wddc.WebContentManager.Core.Domains.GreatPlains.Mapper
{
    /// <summary>
    /// Mapping configuration is required for each Wddc.Web.Core.BaseEntity/Wddc.Data table.
    /// 
    /// Nuget Package Automapper: http://automapper.org/
    /// </summary>
    public class CustomerMap : IMapperConfiguration
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
                cfg.CreateMap<RM00101, CustomerDTO>()
                    .ForMember(dest => dest.BalanceType, opt => opt.MapFrom(src => src.BALNCTYP))
                    .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.BANKNAME))
                    .ForMember(dest => dest.BankBranch, opt => opt.MapFrom(src => src.BNKBRNCH))
                    .ForMember(dest => dest.CashBasedVAT, opt => opt.MapFrom(src => src.CBVAT))
                    .ForMember(dest => dest.CountryCode, opt => opt.MapFrom(src => src.CCode))
                    .ForMember(dest => dest.CreditCardExpDate, opt => opt.MapFrom(src => src.CCRDXPDT))
                    .ForMember(dest => dest.CheckbookId, opt => opt.MapFrom(src => src.CHEKBKID))
                    .ForMember(dest => dest.Comment1, opt => opt.MapFrom(src => src.COMMENT1))
                    .ForMember(dest => dest.Comment2, opt => opt.MapFrom(src => src.COMMENT2))
                    .ForMember(dest => dest.CorporateCustomerNumber, opt => opt.MapFrom(src => src.CPRCSTNM))
                    .ForMember(dest => dest.CreditCardID, opt => opt.MapFrom(src => src.CRCARDID))
                    .ForMember(dest => dest.CreditCardNumber, opt => opt.MapFrom(src => src.CRCRDNUM))
                    .ForMember(dest => dest.RecordCreated, opt => opt.MapFrom(src => src.CREATDDT))
                    .ForMember(dest => dest.CreditLimitAmount, opt => opt.MapFrom(src => src.CRLMTAMT))
                    .ForMember(dest => dest.CreditLimitPeriodAmount, opt => opt.MapFrom(src => src.CRLMTPAM))
                    .ForMember(dest => dest.CreditLimitPeriod, opt => opt.MapFrom(src => (int)src.CRLMTPER))
                    .ForMember(dest => dest.CreditLimitType, opt => opt.MapFrom(src => src.CRLMTTYP))
                    .ForMember(dest => dest.CurrencyId, opt => opt.MapFrom(src => src.CURNCYID))
                    .ForMember(dest => dest.CustomerClass, opt => opt.MapFrom(src => src.CUSTCLAS))
                    .ForMember(dest => dest.CustomerDiscount, opt => opt.MapFrom(src => (int)src.CUSTDISC))
                    .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CUSTNAME))
                    .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CUSTNMBR))
                    .ForMember(dest => dest.CustomerPriority, opt => opt.MapFrom(src => src.CUSTPRIORITY))
                    .ForMember(dest => dest.DeclarantID, opt => opt.MapFrom(src => src.DECLID))
                    .ForMember(dest => dest.DefaultCashAccountType, opt => opt.MapFrom(src => src.DEFCACTY))
                    .ForMember(dest => dest.UniqueAutonumberIdentity, opt => opt.MapFrom(src => src.DEX_ROW_ID))
                    .ForMember(dest => dest.TimeStamp, opt => opt.MapFrom(src => src.DEX_ROW_TS))
                    .ForMember(dest => dest.DiscountGracePeriod, opt => opt.MapFrom(src => src.DISGRPER))
                    .ForMember(dest => dest.DocumentFormatID, opt => opt.MapFrom(src => src.DOCFMTID))
                    .ForMember(dest => dest.DueDateGracePeriod, opt => opt.MapFrom(src => src.DUEGRPER))
                    .ForMember(dest => dest.FinanceChargeDollars, opt => opt.MapFrom(src => src.FINCHDLR))
                    .ForMember(dest => dest.FinanceChargeID, opt => opt.MapFrom(src => src.FINCHID))
                    .ForMember(dest => dest.FinanceChargeAmtType, opt => opt.MapFrom(src => (int)src.FNCHATYP))
                    .ForMember(dest => dest.FinanceChargePercent, opt => opt.MapFrom(src => (int)src.FNCHPCNT))
                    .ForMember(dest => dest.FirstInvoice, opt => opt.MapFrom(src => src.FRSTINDT))
                    .ForMember(dest => dest.GovernmentalCorporateID, opt => opt.MapFrom(src => src.GOVCRPID))
                    .ForMember(dest => dest.GovernmentalIndividualID, opt => opt.MapFrom(src => src.GOVINDID))
                    .ForMember(dest => dest.GPSFOIntegrationID, opt => opt.MapFrom(src => src.GPSFOINTEGRATIONID))
                    .ForMember(dest => dest.Hold, opt => opt.MapFrom(src => src.HOLD))
                    .ForMember(dest => dest.Inactive, opt => opt.MapFrom(src => src.INACTIVE))
                    .ForMember(dest => dest.IncludeInDemandPlanning, opt => opt.MapFrom(src => src.INCLUDEINDP))
                    .ForMember(dest => dest.IntegrationID, opt => opt.MapFrom(src => src.INTEGRATIONID))
                    .ForMember(dest => dest.IntegrationSource, opt => opt.MapFrom(src => src.INTEGRATIONSOURCE))
                    .ForMember(dest => dest.KeepCalendarHistory, opt => opt.MapFrom(src => src.KPCALHST))
                    .ForMember(dest => dest.KeepDistributionHistory, opt => opt.MapFrom(src => src.KPDSTHST))
                    .ForMember(dest => dest.KeepPeriodHistory, opt => opt.MapFrom(src => src.KPERHIST))
                    .ForMember(dest => dest.KeepTrxHistory, opt => opt.MapFrom(src => src.KPTRXHST))
                    .ForMember(dest => dest.MinimumPaymentDollar, opt => opt.MapFrom(src => src.MINPYDLR))
                    .ForMember(dest => dest.MinimumPaymentPercent, opt => opt.MapFrom(src => (int)src.MINPYPCT))
                    .ForMember(dest => dest.MinimumPaymentType, opt => opt.MapFrom(src => (int)src.MINPYTYP))
                    .ForMember(dest => dest.RecordModified, opt => opt.MapFrom(src => src.MODIFDT))
                    .ForMember(dest => dest.MaximumWriteoffType, opt => opt.MapFrom(src => src.MXWOFTYP))
                    .ForMember(dest => dest.MXWROFAM, opt => opt.MapFrom(src => src.MXWROFAM))
                    .ForMember(dest => dest.NoteIndex, opt => opt.MapFrom(src => src.NOTEINDX))
                    .ForMember(dest => dest.OrderFulfillmentShortageDefault, opt => opt.MapFrom(src => src.ORDERFULFILLDEFAULT))
                    .ForMember(dest => dest.PostResultsTo, opt => opt.MapFrom(src => src.Post_Results_To))
                    .ForMember(dest => dest.PrimaryBillToAddress, opt => opt.MapFrom(src => src.PRBTADCD))
                    .ForMember(dest => dest.PriceLevel, opt => opt.MapFrom(src => src.PRCLEVEL))
                    .ForMember(dest => dest.PrimaryShipToAddress, opt => opt.MapFrom(src => src.PRSTADCD))
                    .ForMember(dest => dest.PaymentTermsId, opt => opt.MapFrom(src => src.PYMTRMID))
                    .ForMember(dest => dest.RateTypeId, opt => opt.MapFrom(src => src.RATETPID))
                    .ForMember(dest => dest.RevalueCustomer, opt => opt.MapFrom(src => src.Revalue_Customer))
                    .ForMember(dest => dest.ARAccountIndex, opt => opt.MapFrom(src => src.RMARACC))
                    .ForMember(dest => dest.DiscountsAvailAccountIndex, opt => opt.MapFrom(src => src.RMAVACC))
                    .ForMember(dest => dest.CostofSalesAccountIndex, opt => opt.MapFrom(src => src.RMCOSACC))
                    .ForMember(dest => dest.CashAccountIndex, opt => opt.MapFrom(src => src.RMCSHACC))
                    .ForMember(dest => dest.FinanceChargeAccountIndex, opt => opt.MapFrom(src => src.RMFCGACC))
                    .ForMember(dest => dest.IVAccountIndex, opt => opt.MapFrom(src => src.RMIVACC))
                    .ForMember(dest => dest.OverpaymentWriteoffAccountIndex, opt => opt.MapFrom(src => src.RMOvrpymtWrtoffAcctIdx))
                    .ForMember(dest => dest.SalesAccountIndex, opt => opt.MapFrom(src => src.RMSLSACC))
                    .ForMember(dest => dest.SalesOrderReturnsAccountIndex, opt => opt.MapFrom(src => src.RMSORACC))
                    .ForMember(dest => dest.DiscountsTakenAccountIndex, opt => opt.MapFrom(src => src.RMTAKACC))
                    .ForMember(dest => dest.WriteoffAccountIndex, opt => opt.MapFrom(src => src.RMWRACC))
                    .ForMember(dest => dest.SalesTerritory, opt => opt.MapFrom(src => src.SALSTERR))
                    .ForMember(dest => dest.SendEmailStatements, opt => opt.MapFrom(src => src.Send_Email_Statements))
                    .ForMember(dest => dest.ShipCompleteDocument, opt => opt.MapFrom(src => src.SHIPCOMPLETE))
                    .ForMember(dest => dest.ShippingMethod, opt => opt.MapFrom(src => src.SHIPMTHD))
                    .ForMember(dest => dest.ShortName, opt => opt.MapFrom(src => src.SHRTNAME))
                    .ForMember(dest => dest.SalesPresonId, opt => opt.MapFrom(src => src.SLPRSNID))
                    .ForMember(dest => dest.StatementAddressCode, opt => opt.MapFrom(src => src.STADDRCD))
                    .ForMember(dest => dest.StatementCycle, opt => opt.MapFrom(src => src.STMTCYCL))
                    .ForMember(dest => dest.StatementName, opt => opt.MapFrom(src => src.STMTNAME))
                    .ForMember(dest => dest.TaxExempt1, opt => opt.MapFrom(src => src.TAXEXMT1))
                    .ForMember(dest => dest.TaxExempt2, opt => opt.MapFrom(src => src.TAXEXMT2))
                    .ForMember(dest => dest.TaxSchedule, opt => opt.MapFrom(src => src.TAXSCHID))
                    .ForMember(dest => dest.TaxRegistrationNumber, opt => opt.MapFrom(src => src.TXRGNNUM))
                    .ForMember(dest => dest.UpsZone, opt => opt.MapFrom(src => src.UPSZONE))
                    .ForMember(dest => dest.UserDefined1, opt => opt.MapFrom(src => src.USERDEF1))
                    .ForMember(dest => dest.UserDefined2, opt => opt.MapFrom(src => src.USERDEF2))
                    .ForMember(dest => dest.UserLanguageID, opt => opt.MapFrom(src => src.USERLANG))
                    .ForMember(dest => dest.ContactInformation, opt => opt.MapFrom(src => src));

            };

            return action;
        }

        internal static string PhoneNumberConversion(string gpPhoneFormat)
        {
            if (String.IsNullOrEmpty(gpPhoneFormat) || gpPhoneFormat.All(x => x == '0'))
                return String.Empty;
            var regex = new Regex(@"(\d{3})(\d{3})(\d{4})( ?\d{3,4})");
            var match = regex.Match(gpPhoneFormat);
            var areaCode = match.Groups[1].Value;
            var firstThree = match.Groups[2].Value;
            var lastFour = match.Groups[3].Value;
            var extension = match.Groups[4].Value;
            var sb = new StringBuilder();
            sb.Append(string.Format("({0}) {1}-{2}",
                areaCode, firstThree, lastFour));
            if (extension != null && extension != "0000")
                sb.Append(string.Format(" ext: {0}", extension));
            return sb.ToString();
        }
    }
}
