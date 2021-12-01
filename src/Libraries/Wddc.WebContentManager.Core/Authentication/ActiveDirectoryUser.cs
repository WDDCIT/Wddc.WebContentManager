using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Wddc.WebContentManager.Core.Authentication
{

    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ActiveDirectoryUser : IdentityUser
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string FullName { get; set; }
        public bool Active { get; set; }
        public string Company { get; set; }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ActiveDirectoryUser, string> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here

            userIdentity.AddClaim(new Claim(ClaimTypes.Email, this.Email));
            userIdentity.AddClaim(new Claim(ClaimTypes.Name, this.UserName));
            userIdentity.AddClaim(new Claim("Position", this.Position));
            userIdentity.AddClaim(new Claim("FullName", this.FullName));
            userIdentity.AddClaim(new Claim("Company", this.Company));
            return userIdentity;
        }
    }
}
