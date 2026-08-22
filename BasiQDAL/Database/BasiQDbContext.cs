using BasiQDAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BasiQDAL.Database
{
    public class BasiQDbContext : IdentityDbContext<User>
    {
        public BasiQDbContext(DbContextOptions<BasiQDbContext> options) : base(options)
        { }
        public BasiQDbContext() { }
    }
}
