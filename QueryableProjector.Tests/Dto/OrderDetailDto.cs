namespace QueryableProjector.Tests.Dto {

    public class OrderDetailDto {
        public int Id { get; set; }
        public int OrderId;

        public SupplierDto Supplier { get; set; }
    }
}
