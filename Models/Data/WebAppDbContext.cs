using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Models
{
    public class WebAppDbContext : DbContext
    {
        public WebAppDbContext(DbContextOptions<WebAppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<PARTNER>().HasOne<PARTNER>(x => x.PARENT_PARTNER);
            modelBuilder.Entity<FINANCIAL_ITEM>().HasKey(x => x.financialitem_id);
                
        }

        public DbSet<PARTNER> PARTNER { get; set; }
        public DbSet<FINANCIAL_ITEM> FINANCIAL_ITEM { get; set; }
    }
}
