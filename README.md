# QueryableProjector
IQueryable automatic prjection device, easily convert IQueryable&lt;Entity> to IQueryable&lt;Dto>.
Detects **Include**s and extends only those navigation properties.

Usage;

```cs
using (var context = new TestEntities()) {
    // Entity Query
    var query = context.Orders.Include(o => o.Customer).Include(o => o.OrderDetails.Select(od => od.Supplier));
    // Easily convert to Dto Query
    var dtoQuery = query.ProjectTo<OrderDto>();
    // profit?
}
```
