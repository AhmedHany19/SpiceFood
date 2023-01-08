using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Spice.Models;

namespace Spice.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.Entity<MenuItem>().HasOne(a => a.SubCategory).WithMany(a => a.MenuItems).HasForeignKey(a => a.SubCategoryId).OnDelete(DeleteBehavior.ClientNoAction);
            builder.Entity<MenuItem>().HasOne(a => a.Category).WithMany(a => a.MenuItems).HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.ClientNoAction);

        }
        public DbSet<Category>? Category { get; set; }
        public DbSet<SubCategory>? SubCategory { get; set; }
        public DbSet<MenuItem>? MenuItems { get; set; }
        public DbSet<Coupon>? Coupons { get; set; }
        public DbSet<ApplicationUser>? ApplicationUsers { get; set; }
        public DbSet<OrderHeader>? OrderHeaders { get; set; }
        public DbSet<OrderDetail>? OrderDetails { get; set; }
        public DbSet<ShoppingCart>? ShoppingCarts { get; set; }
    }
}