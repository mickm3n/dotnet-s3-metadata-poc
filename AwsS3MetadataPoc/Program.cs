using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;

namespace AwsS3MetadataTest
{
    class Program
    {
        // 替换为你的 S3 桶名称
        private const string BucketName = "mick-poc-s3-metadata";
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.APSoutheast2;
        private static IAmazonS3 s3Client = null!;

        static async Task Main(string[] args)    
        {
            s3Client = new AmazonS3Client(BucketRegion);

            string key = "test-object.txt";
            string copyKey = "test-object.txt";

            // 1. 上传对象，并附带含有中文的 metadata
            await UploadObjectWithMetadata(key);

            // 2. 使用 head_object 检查 metadata
            await CheckMetadata(key);

            // 3. 使用 copy_object 更新 metadata
            await UpdateMetadataUsingCopy(key, copyKey);

            // 4. 使用 get_object 同时取得文件和 metadata
            await GetObjectAndMetadata(copyKey);

            Console.WriteLine("All Completed");
        }

        private static bool IsAscii(string s) => s.All(c => c < 128);

        private static string PrepareMetadataValue(string value)
        {
            return IsAscii(value) ? value : Rfc2047Helper.Rfc2047Encode(value);
        }

        private static bool IsRfc2047Encoded(string value) => value.StartsWith("=?") && value.EndsWith("?=");

        private static string GetDecodedMetadata(string headerValue)
        {
            return IsRfc2047Encoded(headerValue)
                ? Rfc2047Helper.Rfc2047Decode(headerValue)
                : headerValue;
        }

        private static async Task UploadObjectWithMetadata(string key)
        {
            var request = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = key,
                ContentBody = "123"
            };

            request.Metadata.Add("description", PrepareMetadataValue("含有中文的 Metadata 測試"));
            request.Metadata.Add("notes", PrepareMetadataValue("ASCII Only"));

            PutObjectResponse response = await s3Client.PutObjectAsync(request);
            Console.WriteLine("Complete Upload Objects。");
        }

        private static async Task CheckMetadata(string key)
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key = key
            };

            GetObjectMetadataResponse response = await s3Client.GetObjectMetadataAsync(request);
            Console.WriteLine("HeadObject Metadata：");
            foreach (var metaKey in response.Metadata.Keys)
            {
                string value = response.Metadata[metaKey];
                Console.WriteLine("{0} : {1}", metaKey, GetDecodedMetadata(value));
            }
        }

        private static async Task UpdateMetadataUsingCopy(string sourceKey, string destinationKey)
        {
            var request = new CopyObjectRequest
            {
                SourceBucket = BucketName,
                SourceKey = sourceKey,
                DestinationBucket = BucketName,
                DestinationKey = destinationKey,
                MetadataDirective = S3MetadataDirective.REPLACE                
            };

            request.Metadata.Add("description", Rfc2047Helper.Rfc2047Encode("更新後的中文 Metadata"));

            CopyObjectResponse response = await s3Client.CopyObjectAsync(request);
            Console.WriteLine("Complete Update Metadata");
        }

        private static async Task GetObjectAndMetadata(string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = BucketName,
                Key = key
            };

            using (GetObjectResponse response = await s3Client.GetObjectAsync(request))
            {
                Console.WriteLine("Get Object Content Length: {0}", response.ContentLength);
                Console.WriteLine("GetObject Metadata：");
                foreach (var metaKey in response.Metadata.Keys)
                {
                    string value = response.Metadata[metaKey];
                    Console.WriteLine("{0} : {1}", metaKey, GetDecodedMetadata(value));
                }
            }
        }
    }
}
