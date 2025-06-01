using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace EduSync.Backend.Services;

public interface IBlobService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteFileAsync(string blobName);
    Task<Stream> DownloadFileAsync(string blobName);
    string GetBlobUrl(string blobName);
}

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobService(IConfiguration configuration)
    {
        var connectionString = configuration["AzureStorage:ConnectionString"];
        _containerName = configuration["AzureStorage:ContainerName"] ?? "media";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, new BlobHttpHeaders { ContentType = contentType });

        return blobClient.Uri.ToString();
    }

    public async Task DeleteFileAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<Stream> DownloadFileAsync(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DownloadAsync();
        return response.Value.Content;
    }

    public string GetBlobUrl(string blobName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = containerClient.GetBlobClient(blobName);
        return blobClient.Uri.ToString();
    }
} 