using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MvDomain.Base;

namespace MvInfrastructure.Persistence;

public abstract class BaseConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity {
  public virtual void Configure(EntityTypeBuilder<T> builder) {
    builder.HasKey(e => e.Id);
    builder.Property(x => x.CreatedAt).IsRequired();
  }
}
