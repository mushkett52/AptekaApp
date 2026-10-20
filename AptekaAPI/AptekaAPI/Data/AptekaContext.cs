using AptekaAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AptekaAPI.Data
{
    public class AptekaContext : DbContext
    {
        public AptekaContext(DbContextOptions<AptekaContext> options) : base(options)
        {
        }

        public DbSet<KursRole> KursRoles { get; set; }
        public DbSet<KursUser> KursUsers { get; set; }
        public DbSet<KursClient> KursClients { get; set; }
        public DbSet<KursCategory> KursCategories { get; set; }
        public DbSet<KursSupplier> KursSuppliers { get; set; }
        public DbSet<KursStatus> KursStatuses { get; set; }
        public DbSet<KursMedicine> KursMedicines { get; set; }
        public DbSet<KursOrder> KursOrders { get; set; }
        public DbSet<KursOrderSostav> KursOrderSostavs { get; set; }
    }
}