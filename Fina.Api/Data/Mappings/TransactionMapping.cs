using Fina.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Fina.Api.Data.Mappings;

public class TransactionMapping : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transaction");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Title)
               .IsRequired(true)
               .HasColumnType("NVARCHAR")
               .HasMaxLength(80);

        builder.Property(t => t.Type)
               .IsRequired(true)
               .HasColumnType("SMALLINT");
        
        builder.Property(t => t.Amount)
               .IsRequired(true)
               .HasColumnType("MONEY");
        
        builder.Property(t => t.CreatedAt)
               .IsRequired(true);

        builder.Property(t => t.PaidOrReceivedAt)
               .IsRequired(false);

        builder.Property(x => x.UserId)
               .IsRequired(true)
               .HasColumnType("VARCHAR")
               .HasMaxLength(160);
    }
}
