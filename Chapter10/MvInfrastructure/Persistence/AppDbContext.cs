using System.Data;
using L2.Application.Exceptions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MvApplication.Abstractions;

namespace MvInfrastructure.Persistence;

public class AppDbContext(
  DbContextOptions<AppDbContext> options
) : DbContext(options), IUnitOfWork {
  private IDbContextTransaction? _currentTransaction;

  public async Task BeginTransactionAsync(CancellationToken ct = default) {
    if (_currentTransaction != null) {
      return;
    }

    _currentTransaction = await Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
  }

  public async Task CommitTransactionAsync(CancellationToken ct = default) {
    try {
      if (_currentTransaction != null) {
        await _currentTransaction.CommitAsync(ct);
      }
    } catch (DbUpdateConcurrencyException) {
      await RollbackTransactionAsync(ct);
      throw new WorkflowException("Dữ liệu đã bị thay đổi. Vui lòng thử lại.", 409);
    } catch {
      await RollbackTransactionAsync(ct);
      throw;
    } finally {
      if (_currentTransaction != null) {
        _currentTransaction.Dispose();
        _currentTransaction = null;
      }
    }
  }

  public async Task RollbackTransactionAsync(CancellationToken ct = default) {
    try {
      if (_currentTransaction != null) {
        await _currentTransaction.RollbackAsync(ct);
      }
    } finally {
      if (_currentTransaction != null) {
        _currentTransaction.Dispose();
        _currentTransaction = null;
      }
    }
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder) {
    base.OnModelCreating(modelBuilder);

    // ========== [MassTransit Outbox Entities] ==========
    modelBuilder.AddInboxStateEntity();
    modelBuilder.AddOutboxMessageEntity();
    modelBuilder.AddOutboxStateEntity();

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
  }
}
