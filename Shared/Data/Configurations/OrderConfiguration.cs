using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace WebApplication1.Data.Configurations;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> entity)
    {
        entity.ToTable("Orders");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Status).HasMaxLength(20).IsRequired();
        entity.Property(x => x.Address).HasMaxLength(300).IsRequired();
        entity.Property(x => x.Note).HasMaxLength(500);
        entity.Ignore(x => x.Total);
        entity.HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        entity.HasMany(x => x.Lines).WithOne(x => x.Order).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> entity)
    {
        entity.ToTable("OrderLines", table => table.HasCheckConstraint("CK_OrderLines_Quantity", "[Quantity] > 0"));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ProductName).HasMaxLength(120).IsRequired();
        entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
    }
}
