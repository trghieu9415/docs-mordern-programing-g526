using MediatR;
using MvApplication.Exceptions;
using MvApplication.Ports;

namespace MvApplication.UseCases.Events.UploadPoster;

public class UploadPosterHandler(IEventRepository repository, IStorageService storageService) : IRequestHandler<UploadPosterCommand, string> {
  public async Task<string> Handle(UploadPosterCommand request, CancellationToken cancellationToken) {
    var entity = await repository.GetByIdAsync(request.EventId, cancellationToken);
    if (entity is null) {
      throw new AppException("Khong tim thay su kien.", 404);
    }

    var fileUrl = await storageService.UploadAsync(
      request.FileStream,
      request.FileName,
      request.ContentType,
      $"events/{request.EventId}",
      cancellationToken);

    entity.UpdatePoster(fileUrl);
    await repository.UpdateAsync(entity, cancellationToken);

    return fileUrl;
  }
}
