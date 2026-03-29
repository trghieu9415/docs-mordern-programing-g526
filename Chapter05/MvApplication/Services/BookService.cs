using Microsoft.EntityFrameworkCore;
using MvApplication.DTOs;
using MvApplication.Exceptions;
using MvApplication.Ports;
using MvApplication.Repositories;
using MvDomain.Entities;

namespace MvApplication.Services;

public class BookService(
  IBookReadRepository bookReadRepository,
  IBookRepository bookRepository,
  ICategoryRepository categoryRepository,
  IUnitOfWork unitOfWork,
  IAppLogger<BookService> logger
) : IBookService {
  public async Task<IReadOnlyCollection<BookDto>> GetListAsync(CancellationToken ct = default) {
    var books = await bookReadRepository
      .GetAll()
      .Include(book => book.Categories)
      .OrderBy(book => book.Id)
      .ToListAsync(ct);

    return books
      .Select(MapBookWithoutDetail)
      .ToList();
  }

  public async Task<BookDto> GetDetailAsync(int id, CancellationToken ct = default) {
    var book = await bookReadRepository
      .GetById(id)
      .Select(book => new BookDto(
        book.Id,
        book.Title,
        book.RowVersion,
        book.Categories
          .OrderBy(category => category.Name)
          .Select(category => new CategoryDto(category.Id, category.Name))
          .ToList(),
        book.BookDetail == null
          ? null
          : new BookDetailDto(
            book.BookDetail.Id,
            book.BookDetail.Summary,
            book.BookDetail.IsEbook
          )
      ))
      .FirstOrDefaultAsync(ct);

    return book ?? throw new AppException($"Kh\u00f4ng t\u00ecm th\u1ea5y s\u00e1ch ID: {id}", 404);
  }

  public async Task<BookDto> UpdateAsync(int id, UpdateBookRequest request, CancellationToken ct = default) {
    await using var transaction = await unitOfWork.BeginTransactionAsync(ct);

    try {
      var book = await bookRepository
        .GetByIdTracking(id)
        .Include(item => item.BookDetail)
        .Include(item => item.Categories)
        .FirstOrDefaultAsync(ct);

      if (book is null) {
        throw new AppException($"Kh\u00f4ng t\u00ecm th\u1ea5y s\u00e1ch ID: {id}", 404);
      }

      var categoryIds = request.CategoryIds
        .Distinct()
        .ToArray();

      var categories = await categoryRepository.GetByIdsAsync(categoryIds, ct);

      if (categories.Count != categoryIds.Length) {
        throw new AppException("M\u1ed9t ho\u1eb7c nhi\u1ec1u th\u1ec3 lo\u1ea1i kh\u00f4ng t\u1ed3n t\u1ea1i.", 400);
      }

      book.Update(request.Title, request.Summary, request.IsEbook);
      book.ReplaceCategories(categories);
      bookRepository.Update(book);
      bookRepository.SetOriginalRowVersion(book, request.RowVersion);

      await unitOfWork.SaveChangesAsync(ct);
      await transaction.CommitAsync(ct);

      return await GetDetailAsync(id, ct);
    } catch (DbUpdateConcurrencyException ex) {
      await transaction.RollbackAsync(ct);
      logger.LogBusinessError(
        ex,
        "D\u1eef li\u1ec7u \u0111\u00e3 b\u1ecb thay \u0111\u1ed5i b\u1edfi ng\u01b0\u1eddi kh\u00e1c, vui l\u00f2ng t\u1ea3i l\u1ea1i trang."
      );
      throw new AppException(
        "D\u1eef li\u1ec7u \u0111\u00e3 b\u1ecb thay \u0111\u1ed5i b\u1edfi ng\u01b0\u1eddi kh\u00e1c, vui l\u00f2ng t\u1ea3i l\u1ea1i trang.",
        409
      );
    } catch {
      await transaction.RollbackAsync(ct);
      throw;
    }
  }

  private static BookDto MapBookWithoutDetail(Book book) {
    return new BookDto(
      book.Id,
      book.Title,
      book.RowVersion,
      book.Categories
        .OrderBy(category => category.Name)
        .Select(category => new CategoryDto(category.Id, category.Name))
        .ToList(),
      null
    );
  }
}
