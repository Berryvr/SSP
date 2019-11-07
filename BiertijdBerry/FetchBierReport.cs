using System;
using System.IO;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using BiertijdBerry;
using BiertijdBerry.Models;

namespace BiertijdBerry
{
    public static class FetchBierReport
    {
        [FunctionName("FetchBierReport")]
        public static async System.Threading.Tasks.Task RunAsync([QueueTrigger("mapqueue", Connection = "AzureWebJobsStorage")]string myQueueItem, ILogger log)
        {
            // Create the HttpClient
            var client = new HttpClient();

            // Convert Json message back to its original object (lon, lat, blobname, blobcontainerreference)
            QueueMessage queueMessage = JsonConvert.DeserializeObject<QueueMessage>(myQueueItem);

            // Retrieve storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

            // Retrieve container if it exists, create one if not
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference("biertijdblob");
            await container.CreateIfNotExistsAsync();

            // Create the blob
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(queueMessage.blobName);

            // Retrieve the picture for the given city using its lat and lon
            var url = String.Format("https://atlas.microsoft.com/map/static/png?subscription-key={0}&api-version=1.0&center={1},{2}", Environment.GetEnvironmentVariable("MapsAPIKey"), queueMessage.lon, queueMessage.lat);
            client.BaseAddress = new Uri(url);
            HttpResponseMessage responseMessage = await client.GetAsync(url);

            if (responseMessage.IsSuccessStatusCode)
            {
                Stream stream = await responseMessage.Content.ReadAsStreamAsync();
                   
                string bier = checkForBier(queueMessage);
                Stream renderedImage = AddTextToImage.AddText(stream, bier, 10, 50);

                // Upload the image to the blob
                await blockBlob.UploadFromStreamAsync(renderedImage);
                log.LogInformation("City has been found and image was uploaded succesfully");
            }
            else
                log.LogError("Could not retrieve the map for the given coordinates");
        }
        public static string checkForBier(QueueMessage queueStorageMessage)
        {
            const double BIERTEMPERATUUR = 0.00;
            string bier = "";
            if (queueStorageMessage.temp >= BIERTEMPERATUUR)
            {
                bier = "It's always time for a beer";
            }
            else
                bier = "Maybe something warmer";

            return bier;
        }
    }

}
