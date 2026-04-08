using MediatR;

namespace MvApplication.UseCases.Events.UploadPoster;

public record UploadPosterCommand(
  Guid EventId,
  Stream FileStream,
  string FileName,
  string ContentType
) : IRequest<string>;
