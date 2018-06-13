using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyList.Models.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        //DbSets
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Bug> BugReports { get; set; }
        public DbSet<List> Lists { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ListProduct> ListProducts { get; set; }
        public DbSet<UserList> UserLists { get; set; }
        public DbSet<InviteUserList> Invites { get; set; }
    }
}
