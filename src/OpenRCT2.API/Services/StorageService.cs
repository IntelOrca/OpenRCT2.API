using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenRCT2.API.Configuration;

namespace OpenRCT2.API.Services
{
    public class StorageService
    {
        private readonly AmazonS3Client _client;
        private readonly string _bucketName;
        private readonly string _publicUrl;
        private readonly ILogger _logger;

        public StorageService(IOptions<StorageConfig> storageOptions, ILogger<StorageService> logger)
        {
            var storageConfig = storageOptions.Value;
            _client = CreateS3Client(storageConfig);
            _bucketName = storageConfig.Bucket;
            _publicUrl = storageConfig.PublicUrl;
            _logger = logger;
        }

        private static AmazonS3Client CreateS3Client(StorageConfig config)
        {
            var clientConfig = new AmazonS3Config()
            {
                ServiceURL = config.Endpoint
            };
            return new AmazonS3Client(config.Key, config.Secret, clientConfig);
        }

        public string GetPublicUrl(string key)
        {
            return _publicUrl + key;
        }

        public async Task<Transaction> UploadPublicFileTransactionAsync(Stream stream, string key, string contentType, IEnumerable<KeyValuePair<string, string>> tags = null)
        {
            await UploadPublicFileAsync(stream, key, contentType, tags);
            return new Transaction(this, key);
        }

        public async Task<string> UploadPublicFileAsync(Stream stream, string key, string contentType, IEnumerable<KeyValuePair<string, string>> tags = null)
        {
            _logger.LogInformation("Uploading '{0}' to S3", key);
            var response = await _client.PutObjectAsync(new PutObjectRequest()
            {
                AutoCloseStream = false,
                AutoResetStreamPosition = false,
                CannedACL = S3CannedACL.PublicRead,
                BucketName = _bucketName,
                ContentType = contentType,
                InputStream = stream,
                Key = key,
                TagSet = tags?.Select(x => new Tag { Key = x.Key, Value = x.Value }).ToList()
            });
            ;
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to upload file.");
            }
            return GetPublicUrl(key);
        }

        public async Task DeleteAsync(string key)
        {
            _logger.LogInformation("Deleting '{0}' from S3", key);
            var response = await _client.DeleteObjectAsync(new DeleteObjectRequest()
            {
                BucketName = _bucketName,
                Key = key
            });
            if (response.HttpStatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failed to delete file.");
            }
        }

        public class Transaction : IAsyncDisposable
        {
            private readonly StorageService _storageService;
            private bool _disposed;
            private bool _committed;

            public string Key { get; }
            public string PublicUrl => _storageService.GetPublicUrl(Key);

            public Transaction(StorageService storageService, string key)
            {
                Key = key;
                _storageService = storageService;
            }

            public void Commit()
            {
                _committed = true;
            }

            public async ValueTask DisposeAsync()
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(Transaction));

                _disposed = true;
                if (!_committed)
                {
                    await RollbackAsync();
                }
            }

            private Task RollbackAsync() => _storageService.DeleteAsync(Key);
        }
    }
}
