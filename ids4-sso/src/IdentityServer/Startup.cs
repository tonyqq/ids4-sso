using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.Configuration;
using IdentityServer.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using CustomGrantValidator = IdentityServer.Extensions.CustomGrantValidator;

namespace IdentityServer
{
    public class Startup
    {

        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment environment)
        {
            _environment = environment;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            var builder = services.AddIdentityServer(options => options.SigningCertificate = cert);

            builder.AddInMemoryClients(Clients.Get());
            builder.AddInMemoryScopes(Scopes.Get());
            builder.AddInMemoryUsers(Users.Get());

            builder.AddCustomGrantValidator<CustomGrantValidator>();

            //  Uncomment this if you want to use Custom User service that load data from Db via EF
            //builder.Services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();
            //builder.Services.AddTransient<IProfileService, ProfileService>();

            // for the UI
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander());
                });
            services.AddTransient<UI.Login.LoginService>();

            // Did not work, need to define it in IdSDb's overrided method OnConfiguring
            //services.AddDbContext<IdSDbContext>(
            //    options =>
            //        options.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=IdentityServer.IdSDbContext;Trusted_Connection=True;"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Trace);
            loggerFactory.AddDebug(LogLevel.Trace);

            app.UseDeveloperExceptionPage();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Temp",
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();

            Seed();
        }

        private static void Seed()
        {
            using (var db = new IdSDbContext(new DbContextOptions<IdSDbContext>()))
            {
                if (db.Database.EnsureDeleted())
                {
                    db.Database.Migrate();
                    var users = Enumerable.Range(1, 5).Select(i => new User
                    {
                        Id = i,
                        Login = "login" + i,
                        Password = "pass" + i,
                        Name = "name" + i,
                        Surname = "surname" + i
                    });

                    db.Users.AddRange(users);
                    db.SaveChanges();

                }

            }
        }
    }
}
