using System.Data.Entity;

namespace QueryableProjector.Tests.Model {

    public class TestEntities: DbContext {

        static TestEntities() {
            Database.SetInitializer(new TestDatabaseInitializer());
        }

        public DbSet<Order> Orders { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
        }
    }
}
