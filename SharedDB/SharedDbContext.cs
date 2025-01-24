using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace SharedDB
{
    public class SharedDbContext : DbContext
    {
        public DbSet<ServerDb> Servers { get; set; }

        
        #region GameServer 방식
        #endregion

        #region ASP.NET 방식
        public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerDb>()
                .HasIndex(s => s.Name)
                .IsUnique();
        }
        #endregion
    }
}
