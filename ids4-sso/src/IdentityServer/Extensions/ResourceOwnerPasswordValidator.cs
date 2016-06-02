using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer.Context;
using IdentityServer4.Core.Validation;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Extensions
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        public Task<CustomGrantValidationResult> ValidateAsync(string userName, string password,
            ValidatedTokenRequest request)
        {
            var user = GetUserByLogin(userName);
            if (user != null && user.Password == password)
            {
                var result = new CustomGrantValidationResult(user.Login, "password", new[]
                {
                    new Claim(JwtClaimTypes.Name, user.Name),
                    new Claim(JwtClaimTypes.FamilyName, user.Surname)
                });
                return Task.FromResult(result);
            }
            else
            {
                var result = new CustomGrantValidationResult("Username Or Password Incorrect");
                return Task.FromResult(result);
            }
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