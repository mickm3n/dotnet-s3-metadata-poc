using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace AwsS3MetadataTest
{
    class Program
    {
        private const string BucketName = "mick-poc-s3-metadata";
        private static readonly RegionEndpoint BucketRegion = RegionEndpoint.APSoutheast2;
        private static IAmazonS3 s3Client = null!;

        static async Task Main(string[] args)    
        {
            s3Client = new AmazonS3Client(BucketRegion);

            string key = "test-object.txt";
            string copyKey = "test-object.txt";

            var metadata = new Dictionary<string, string>
            {
                { "description", "含有中文的 Metadata 測試" },
                { "notes", "ASCII Only" }
            };

            await UploadObjectWithMetadata(key, metadata);

            await CheckMetadata(key);

            var new_metadata = new Dictionary<string, string>
            {
                { "description", "更新後的中文 Metadata" },
            };

            await UpdateMetadataUsingCopy(key, copyKey, new_metadata);

            await GetObjectAndMetadata(copyKey);

            Console.WriteLine("All Completed");
        }

        private static async Task UploadObjectWithMetadata(string key, Dictionary<string, string> metadata)
        {
            var request = new PutObjectRequest
            {
                BucketName = BucketName,
                Key = key,
                ContentBody = "123"
            };

            foreach (var item in metadata)
            {
                request.Metadata.Add(item.Key, Rfc2047Helper.Rfc2047Encode(item.Value));
            }

            PutObjectResponse response = await s3Client.PutObjectAsync(request);
            Console.WriteLine("Complete Upload Objects");
        }

        private static async Task CheckMetadata(string key)
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = BucketName,
                Key = key
            };

            GetObjectMetadataResponse response = await s3Client.GetObjectMetadataAsync(request);
            Console.WriteLine("HeadObject Metadata: ");
            foreach (var metaKey in response.Metadata.Keys)
            {
                string value = response.Metadata[metaKey];
                Console.WriteLine("{0} : {1}", metaKey, Rfc2047Helper.Rfc2047Decode(value));
            }
        }

        private static async Task UpdateMetadataUsingCopy(string sourceKey, string destinationKey, Dictionary<string, string> metadata)
        {
            var request = new CopyObjectRequest
            {
                SourceBucket = BucketName,
                SourceKey = sourceKey,
                DestinationBucket = BucketName,
                DestinationKey = destinationKey,
                MetadataDirective = S3MetadataDirective.REPLACE                
            };

            foreach (var item in metadata)
            {
                request.Metadata.Add(item.Key, Rfc2047Helper.Rfc2047Encode(item.Value));
            }

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
                Console.WriteLine("GetObject Metadata: ");
                foreach (var metaKey in response.Metadata.Keys)
                {
                    string value = response.Metadata[metaKey];
                    Console.WriteLine("{0} : {1}", metaKey, Rfc2047Helper.Rfc2047Decode(value));
                }
            }
        }
    }
}
