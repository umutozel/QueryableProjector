using System.Collections.Generic;

namespace QueryableProjector.Tests.Dto {

    public class OrderDto {

        public OrderDto() {
            OrderDetails = new List<OrderDetailDto>();
        }

        public int Id;
        public string OrderNo { get; set; }
        public int? CustomerId;

        public ICollection<OrderDetailDto> OrderDetails { get; set; }
        public CustomerDto Customer;
    }
}
