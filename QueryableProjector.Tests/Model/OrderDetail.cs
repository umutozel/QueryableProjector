namespace QueryableProjector.Tests.Model {

    public class OrderDetail {
        public int Id { get; set; }
        public string ProductNo { get; set; }
        public int OrderId { get; set; }

        public int? SupplierId { get; set; }

        public Order Order { get; set; }
        public Supplier Supplier { get; set; }
    }
}
