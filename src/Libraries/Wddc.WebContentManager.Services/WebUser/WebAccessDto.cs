using System;

namespace Wddc.WebContentManager.Services.WebUser
{
    public class WebAccessDto
    {
        public string UserName { get; set; }
        public string AccessName { get; set; }
        public string PasswordRecovery { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool ResetEmailSent { get; set; }
        public bool PasswordSet { get; set; }
    }

    public class WebAccessMemberDto
    {
        public string MemberNbr { get; set; }
        public string CUSTNAME { get; set; }
    }

    public class ContactInfoDto
    {
        public string MemberNbr { get; set; }
        public string CUSTNAME { get; set; }
        public string Clinic_Email { get; set; }
        public string Clinic_Phone { get; set; }
        public string Clinic_Fax { get; set; }
        public string CartSortingDescription { get; set; }
        public string WDDCSupport { get; set; }
        public string InvoiceType { get; set; }
        public string Email_Orders { get; set; }
        public string Email_Invoices { get; set; }
        public string Email_Statements { get; set; }
        public string Email_Returns { get; set; }
        public string Email_News { get; set; }
        public string Contact_NameAcctng { get; set; }
        public string Contact_PhoneAcctng { get; set; }
        public string Contact_EmailAcctng { get; set; }
        public string Contact_NameOrdering { get; set; }
        public string Contact_PhoneOrdering { get; set; }
        public string Contact_EmailOrdering { get; set; }
        public string Reception_Name { get; set; }
        public string Reception_Phone { get; set; }
        public string Reception_Email { get; set; }
        public string Manager_Name { get; set; }
        public string Manager_Phone { get; set; }
        public string Manager_Email { get; set; }
        public DateTime? LastUpdated { get; set; }
    }
}
