using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Web.Domain.Entities;

[assembly: InternalsVisibleTo("Web.Infrastructure.Tests")]

namespace Web.Infrastructure.Persistence;

/// <summary>
///     Entity Framework Core database context for managing product entities.
/// </summary>
internal class DatabaseContext : DbContext
{
    /// <summary>
    ///     Creates a new instance of the <see cref="DatabaseContext" /> class with the specified options.
    /// </summary>
    /// <param name="options">The options.</param>
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="DatabaseContext" /> class.
    /// </summary>
    public DatabaseContext()
    {
    }

    /// <summary>
    ///     Gets or sets the stored products.
    /// </summary>
    public DbSet<ProductRecord> Products { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductRecord>(entity =>
        {
            entity.ToTable("Products");
            entity.Property(p => p.Name).HasMaxLength(ProductLimits.MAX_NAME_LENGTH);
            entity.Property(p => p.ImgUri).HasMaxLength(ProductLimits.MAX_IMG_URI_LENGTH);
            entity.Property(p => p.Description).HasMaxLength(ProductLimits.MAX_DESCRIPTION_LENGTH);
            entity.Property(p => p.Price).HasPrecision(18, 2);
            // Changed by the database on every update. The UPDATE statement checks it, so a stale copy cannot overwrite a newer change
            entity.Property(p => p.RowVersion).IsRowVersion();
            // The product list is ordered by creation time and then by ID, so the list is paged with an index on both
            entity.HasIndex(p => new { p.CreatedAt, p.Id });
        });
    }
}
