using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using QueryableProjector.Tests.Dto;
using QueryableProjector.Tests.Model;

namespace QueryableProjector.Tests {

    [TestClass]
    public class MapDefinitionTests {

        [ClassInitialize]
        public static void ClassInit(TestContext context) {
            AppDomain.CurrentDomain.SetData("DataDirectory", context.TestDeploymentDir);

            // warm-up, to get more realistic test run times.
            using (var ctx = new TestEntities()) {
                ctx.Orders.ToList();
            }
        }

        [TestMethod]
        [TestCategory("Map Definition Tests")]
        [ExpectedException(typeof(ArrayTypeMismatchException))]
        public void OneToManyTypeMismatchCheck() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.OrderDetails);

                var mapDefinitions = new Dictionary<Type, IMapDefinition>();
                mapDefinitions[typeof(Order)] = new MapDefinition(
                    new Dictionary<string, string> {
                        { "Supplier", "OrderDetails" } // set collection to single
                    }, true
                );

                var dtoQuery = query.ProjectTo<OrderDetail>(mapDefinitions);
            }
        }

        [TestMethod]
        [TestCategory("Map Definition Tests")]
        [ExpectedException(typeof(ArrayTypeMismatchException))]
        public void ManyToOneTypeMismatchCheck() {
            using (var context = new TestEntities()) {
                var query = context.Set<OrderDetail>().Include(od => od.Supplier);

                var mapDefinitions = new Dictionary<Type, IMapDefinition>();
                mapDefinitions[typeof(OrderDetail)] = new MapDefinition(
                    new Dictionary<string, string> {
                        { "OrderDetails", "Supplier" } // set single to collection
                    }, true
                );

                var dtoQuery = query.ProjectTo<Order>(mapDefinitions);
            }
        }

        [TestMethod]
        [TestCategory("Map Definition Tests")]
        public void OrderCustomerWithDefinition() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.Customer);

                var mapDefinitions = new Dictionary<Type, IMapDefinition>();
                mapDefinitions[typeof(Order)] = new MapDefinition(
                    new Dictionary<string, string> {
                        { "CustomerId", "Id" } // set OrderNo with Id
                    }
                );
                mapDefinitions[typeof(Customer)] = new MapDefinition(
                    new Dictionary<string, string> {
                        { "Id", "Id" } // get only Id field
                    }, true
                );

                var dtoQuery = query.ProjectTo<OrderDto>(mapDefinitions);
                var orders = dtoQuery.ToList();

                Assert.AreEqual(2, orders.Count);
                Assert.IsTrue(orders.All(o => o.CustomerId == o.Id));
                Assert.IsTrue(orders.All(o => o.Customer != null && o.Customer.Name == null));
                Assert.IsTrue(orders.All(o => !o.OrderDetails.Any()));
            }
        }

        [TestMethod]
        [TestCategory("Map Definition Tests")]
        public void OrderDetailsWithDefinition() {
            using (var context = new TestEntities()) {
                var query = context.Orders.Include(o => o.OrderDetails.Select(od => od.Supplier));

                var mapDefinitions = new Dictionary<Type, IMapDefinition>();
                mapDefinitions[typeof(OrderDetail)] = new MapDefinition(
                    new Dictionary<string, string> {
                        { "Id", "OrderId" } // write OrderId into Id field
                    }, true
                );

                var dtoQuery = query.ProjectTo<OrderDto>(mapDefinitions);
                var orders = dtoQuery.ToList();

                Assert.AreEqual(2, orders.Count);
                Assert.IsTrue(orders.All(o => o.OrderDetails.All(od => od.Id == o.Id)));
                // suppliers should be null even if it is included, because we wanted only explicit properties.
                Assert.IsTrue(orders.All(o => o.OrderDetails.All(od => od.Supplier == null)));
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
