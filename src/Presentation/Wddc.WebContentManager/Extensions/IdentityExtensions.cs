using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace Wddc.Web
{
    public static class IdentityExtensions
    {
        public static string GetPosition(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Position");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetFullName(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("FullName");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
        public static string GetCompany(this IIdentity identity)
        {
            var claim = ((ClaimsIdentity)identity).FindFirst("Company");
            // Test for null to avoid issues during local testing
            return (claim != null) ? claim.Value : string.Empty;
        }
    }
}