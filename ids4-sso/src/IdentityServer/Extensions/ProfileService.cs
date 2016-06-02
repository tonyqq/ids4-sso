using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Context;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Extensions
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject.Claims.ToList().Find(s => s.Type == "sub").Value;

            var user = GetUserByLogin(subject);
            if (user == null)
            {
                return Task.FromResult(0);
            }

            var claims = new[]
            {
                new Claim(JwtClaimTypes.Name, user.Name),
                new Claim(JwtClaimTypes.FamilyName, user.Surname),
                new Claim(JwtClaimTypes.Subject, subject)
            };

            context.IssuedClaims = claims.Where(x => context.RequestedClaimTypes.Contains(x.Type));
            return Task.FromResult(0);
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.FromResult(0);
        }

        private static User GetUserByLogin(string userName)
        {
            using (var context = new IdSDbContext(new DbContextOptions<IdSDbContext>()))
            {
                return context.Users.SingleOrDefault(x => x.Login == userName);
            }
        }
    }
}