using System.Collections.Generic;

namespace QueryableProjector.Tests.Model {

    public class Order {

        public Order() {
            OrderDetails = new HashSet<OrderDetail>();
        }

        public int Id { get; set; }
        public string OrderNo { get; set; }
        public decimal Price { get; set; }
        public int CustomerId { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
        public Customer Customer { get; set; }
    }
}
