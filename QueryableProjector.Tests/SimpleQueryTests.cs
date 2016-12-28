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
        public void OrderCustomer() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.Customer);
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(0, order.OrderDetails.Count);
                Assert.IsNotNull(order.Customer);
            }
        }

        [TestMethod]
        public void OrderCustomerStringInclude() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include("Customer");
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(0, order.OrderDetails.Count);
                Assert.IsNotNull(order.Customer);
            }
        }

        [TestMethod]
        public void OrderDetails() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.OrderDetails);
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(3, order.OrderDetails.Count);
                Assert.IsNull(order.Customer);

                var detail = order.OrderDetails.First();
                Assert.IsNull(detail.Supplier);
            }
        }

        [TestMethod]
        public void OrderDetailsStringInclude() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include("OrderDetails");
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(3, order.OrderDetails.Count);
                Assert.IsNull(order.Customer);

                var detail = order.OrderDetails.First();
                Assert.IsNull(detail.Supplier);
            }
        }

        [TestMethod]
        public void OrderCustomerDetailSupplier() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(od => od.Supplier));
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(3, order.OrderDetails.Count);
                Assert.IsNotNull(order.Customer);

                var detail = order.OrderDetails.First();
                Assert.IsNotNull(detail.Supplier);
            }
        }

        [TestMethod]
        public void OrderCustomerDetailSupplierStringInclude() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include("Customer").Include("OrderDetails.Supplier");
                var dtoQuery = query.ProjectTo<OrderDto>();

                var orders = dtoQuery.ToList();
                Assert.AreEqual(2, orders.Count);

                var order = orders.First();
                Assert.AreEqual(3, order.OrderDetails.Count);
                Assert.IsNotNull(order.Customer);

                var detail = order.OrderDetails.First();
                Assert.IsNotNull(detail.Supplier);
            }
        }
    }
}
