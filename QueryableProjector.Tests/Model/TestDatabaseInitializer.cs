using System;
using System.Data.Entity;

namespace QueryableProjector.Tests.Model {

    public class TestDatabaseInitializer: DropCreateDatabaseAlways<TestEntities> {
        private readonly Random _random = new Random();

        protected override void Seed(TestEntities context) {
            // Create some data for tests.
            var os = new[] {
                               CreateOrder(42),
                               CreateOrder()
                           };

            // add created data to context
            Array.ForEach(os, o => context.Orders.Add(o));

            // save all data
            context.SaveChanges();
        }

        public override void InitializeDatabase(TestEntities context) {
            context.Database.Create();

            Seed(context);
        }

        public Order CreateOrder(decimal? price = null) {
            var o = new Order();
            o.OrderNo = "OrderNo_" + _random.Next(1, 100);
            o.Price = price ?? (_random.Next(10000 - 42) + 42);
            o.Customer = CreateCustomer();
            o.OrderDetails.Add(CreateOrderDetail());
            o.OrderDetails.Add(CreateOrderDetail());
            o.OrderDetails.Add(CreateOrderDetail());
            return o;
        }

        public Customer CreateCustomer() {
            var c = new Customer();
            c.Name = "Customer_" + _random.Next(1, 100);
            return c;
        }

        public OrderDetail CreateOrderDetail() {
            var od = new OrderDetail();
            od.ProductNo = "ProductNo_" + _random.Next(1, 100);
            od.Supplier = CreateSupplier();
            return od;
        }

        public Supplier CreateSupplier() {
            var s = new Supplier();
            s.Name = "Supplier_" + _random.Next(1, 100);
            return s;
        }
    }
}