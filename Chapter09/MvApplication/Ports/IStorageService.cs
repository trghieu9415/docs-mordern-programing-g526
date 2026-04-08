namespace MvApplication.Ports;

public interface IStorageService {
  Task<string> UploadAsync(Stream stream, string fileName, string contentType, string folder, CancellationToken ct = default);
}
