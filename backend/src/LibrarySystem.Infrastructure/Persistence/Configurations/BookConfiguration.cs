using LibrarySystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibrarySystem.Infrastructure.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Author).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Isbn).HasMaxLength(32).IsRequired();
        builder.HasIndex(x => x.Isbn).IsUnique();
        builder.Property(x => x.TotalQuantity).IsRequired();
        builder.Property(x => x.Quantity).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.Ignore(x => x.LoanItems);
    }
}
