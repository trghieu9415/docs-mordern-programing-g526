using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using MvApplication.Ports;
using MvInfrastructure.Configuration;
using MvInfrastructure.Exceptions;

namespace MvInfrastructure.Adapters;

public class S3StorageService(
  IAmazonS3 s3Client,
  IOptions<ObjectStorageOptions> options
) : IStorageService {
  private readonly ObjectStorageOptions _options = options.Value;

  public async Task<string> UploadAsync(
    Stream stream,
    string fileName,
    string contentType,
    string folder,
    CancellationToken ct = default
  ) {
    await EnsureBucketExistsAsync(ct);

    var sanitizedFileName = fileName.Replace(" ", "-");
    var key = $"{folder.Trim('/')}/{Guid.NewGuid():N}-{sanitizedFileName}";

    var request = new PutObjectRequest {
      BucketName = _options.BucketName,
      Key = key,
      InputStream = stream,
      ContentType = contentType,
      AutoCloseStream = false
    };

    var response = await s3Client.PutObjectAsync(request, ct);
    if (response.HttpStatusCode is not System.Net.HttpStatusCode.OK) {
      throw new InfrastructureException("Upload poster len object storage that bai.");
    }

    return BuildPublicUrl(key);
  }

  private async Task EnsureBucketExistsAsync(CancellationToken ct) {
    var exists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(s3Client, _options.BucketName);
    if (!exists) {
      await s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _options.BucketName }, ct);
    }
  }

  private string BuildPublicUrl(string key) {
    var baseUrl = _options.PublicBaseUrl;
    if (string.IsNullOrWhiteSpace(baseUrl)) {
      baseUrl = _options.ServiceUrl ?? throw new InfrastructureException("ObjectStorage:PublicBaseUrl hoac ServiceUrl chua duoc cau hinh.");
    }

    return $"{baseUrl.TrimEnd('/')}/{_options.BucketName}/{key}";
  }
}
