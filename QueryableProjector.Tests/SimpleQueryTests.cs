using System;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryableProjector.Tests.Dto;
using QueryableProjector.Tests.Model;

namespace QueryableProjector.Tests {

    [TestClass]
    public class SimpleQueryTests {

        [ClassInitialize]
        public static void ClassInit(TestContext context) {
            AppDomain.CurrentDomain.SetData("DataDirectory", context.TestDeploymentDir);

            // warm-up, to get more realistic test run times.
            using (var ctx = new TestEntities()) {
                ctx.Orders.ToList();
            }
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderCustomer() {
            using (var context = new TestEntities()) {
                OrderCustomerImpl(context.Orders.Include(o => o.Customer));
            }
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderCustomerStringInclude() {
            using (var context = new TestEntities()) {
                OrderCustomerImpl(context.Orders.Include("Customer"));
            }
        }

        private static void OrderCustomerImpl(IQueryable<Order> query) {
            var dtoQuery = query.ProjectTo<OrderDto>();
            var orders = dtoQuery.ToList();

            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.All(o => !o.OrderDetails.Any()));
            Assert.IsTrue(orders.All(o => o.Customer != null && o.CustomerId == o.Customer.Id));
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderDetails() {
            using (var context = new TestEntities()) {
                OrderDetailsImpl(context.Orders.Include(o => o.OrderDetails));
            }
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderDetailsStringInclude() {
            using (var context = new TestEntities()) {
                OrderDetailsImpl(context.Orders.Include("OrderDetails"));
            }
        }

        private static void OrderDetailsImpl(IQueryable<Order> query) {
            var dtoQuery = query.ProjectTo<OrderDto>();
            var orders = dtoQuery.ToList();

            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.All(o => o.OrderDetails.Count == 3));
            Assert.IsTrue(orders.All(o => o.Customer == null));
            Assert.IsTrue(orders.All(o => o.OrderDetails.All(od => od.Supplier == null)));
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderCustomerDetailSupplier() {
            using (var context = new TestEntities()) {
                OrderCustomerDetailSupplierImpl(context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(od => od.Supplier)));
            }
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderCustomerDetailSupplierStringInclude() {
            using (var context = new TestEntities()) {
                OrderCustomerDetailSupplierImpl(context.Orders.Include("Customer").Include("OrderDetails.Supplier"));
            }
        }

        private static void OrderCustomerDetailSupplierImpl(IQueryable<Order> query) {
            var dtoQuery = query.ProjectTo<OrderDto>();
            var orders = dtoQuery.ToList();

            Assert.AreEqual(2, orders.Count);
            Assert.IsTrue(orders.All(o => o.OrderDetails.Count == 3));
            Assert.IsTrue(orders.All(o => o.Customer != null));
            Assert.IsTrue(orders.All(o => o.OrderDetails.All(od => od.Supplier != null)));
        }

        [TestMethod]
        [TestCategory("Simple Query Tests")]
        public void OrderCustomerDetailSupplierWithProjector() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(od => od.Supplier));

                var projector = Helper.CreateProjector<Order, OrderDto>(Helper.GetIncludes(query));
                var dtoQuery = query.Select(projector);
                var orders = dtoQuery.ToList();

                Assert.AreEqual(2, orders.Count);
                Assert.IsTrue(orders.All(o => o.OrderDetails.Count == 3));
                Assert.IsTrue(orders.All(o => o.Customer != null));
                Assert.IsTrue(orders.All(o => o.OrderDetails.All(od => od.Supplier != null)));
            }
        }

        [ClassCleanup]
        public static void ClassCleanup() {
            using (var ctx = new TestEntities()) {
                ctx.Database.Delete();
            }
        }
    }
}
