using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Context
{
    public class IdSDbContext: DbContext
    {
        public IdSDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                @"Server=(localdb)\mssqllocaldb;Database=IdentityServer.IdSDbContext;Trusted_Connection=True;");
        }

        public DbSet<User> Users { get; set; }

        public DbSet<Client> Clients { get; set; }
    }

    public class User
    {
        public int Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }
    }

    public class Client
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }
}