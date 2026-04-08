namespace MvInfrastructure.Configuration;

public class ObjectStorageOptions {
  public const string SectionName = "ObjectStorage";

  public string Provider { get; set; } = "MinIO";
  public string BucketName { get; set; } = "event-posters";
  public string Region { get; set; } = "us-east-1";
  public string AccessKey { get; set; } = string.Empty;
  public string SecretKey { get; set; } = string.Empty;
  public string? ServiceUrl { get; set; }
  public string? PublicBaseUrl { get; set; }
  public bool ForcePathStyle { get; set; } = true;
}
