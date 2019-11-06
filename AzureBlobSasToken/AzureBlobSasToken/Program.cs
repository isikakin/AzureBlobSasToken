using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureBlobSasToken
{
    class Program
    {
        static string storageConnectionString = "yourStorageConnectionString";
        static string containerName = "yourPrivateContainerName";
        static int sasTokenExpireTime = 1;

        static void Main(string[] args)
        {
            Console.WriteLine("Account SASToken: {0}", GenerateAccountSASToken());
            Console.WriteLine("Specific Blob SASToken: {0}", GenerateSASToken(containerName));
            Console.ReadLine();
        }

        //ALL PRIVATE CONTAINER
        public static string GenerateAccountSASToken()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create a new access policy for the account.
            SharedAccessAccountPolicy policy = new SharedAccessAccountPolicy()
            {
                Permissions = SharedAccessAccountPermissions.Read | SharedAccessAccountPermissions.List,
                Services = SharedAccessAccountServices.Blob | SharedAccessAccountServices.File,
                ResourceTypes = SharedAccessAccountResourceTypes.Container | SharedAccessAccountResourceTypes.Service | SharedAccessAccountResourceTypes.Object,
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(sasTokenExpireTime), //Token ExpireTime is sasTokenExpireTime hour
                Protocols = SharedAccessProtocol.HttpsOnly
            };
            
            return storageAccount.GetSharedAccessSignature(policy);
        }

        //SPECIFIC PRIVATE CONTAINER
        public static string GenerateSASToken(string containerName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);

            bool containerExists = container.Exists();
            if (!containerExists)
            {
                container.CreateIfNotExists();
            }
            var sasToken = GetContainerSasUri(container);
            return sasToken;
        }

        private static string GetContainerSasUri(CloudBlobContainer container)
        {
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy();
            sasConstraints.SharedAccessStartTime = DateTimeOffset.UtcNow;
            sasConstraints.SharedAccessExpiryTime = DateTimeOffset.UtcNow.AddHours(sasTokenExpireTime); //Token ExpireTime is sasTokenExpireTime hour
            sasConstraints.Permissions = SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List;

            //Generate the shared access signature on the blob, setting the constraints directly on the signature.
            string sasContainerToken = container.GetSharedAccessSignature(sasConstraints);

            //Return the URI string for the container, including the SAS token.
            return sasContainerToken;
        }
    }
}
