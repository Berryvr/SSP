using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using BiertijdBerry.Models;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BiertijdBerry
{
    public static class CreateBierReport
    {
        [FunctionName("CreateBierReport")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            // Create the HttpClient
            var client = new HttpClient();

            string city = req.Query["city"];
            string country = req.Query["country"];

            if (city != null && country != null)
            {


                var weatherApiUrl = String.Format("http://api.openweathermap.org/data/2.5/weather?q={0},{1}&units=metric&appid={2}", city, country, "52abf6f17ad3b20caa8f466327132436");
                Weather weather = await GetWeatherData(weatherApiUrl);
                HttpResponseMessage responseMessageWeatherApi = await client.GetAsync(weatherApiUrl);

                if (responseMessageWeatherApi.IsSuccessStatusCode)
                {
                    // Converting the commas in the lon and lat to dots
                    string lon = Formatcoords(weather.coord.lon);
                    string lat = Formatcoords(weather.coord.lat);

                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));

                    // Create the CloudBlobClient that represents the Blob storage endpoint for the storage account.
                    CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                    // Create the blobcontainer with permissions if it doesnt exist
                    var cloudBlobContainer = cloudBlobClient.GetContainerReference("biertijdblob");
                    await cloudBlobContainer.CreateIfNotExistsAsync();
                    BlobContainerPermissions permissions = new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    };
                    await cloudBlobContainer.SetPermissionsAsync(permissions);

                    // Create a Globally  Unique Identifier (GUID)
                    var guid = Guid.NewGuid().ToString();

                    // Define the needed parameters for the image object
                    string blobName = String.Format("map-{0}-{1}-{2}.png", city, country, guid);
                    string blobReference = "bierimage";
                    string bloburl = String.Format("https://storageaccountssp95ec.blob.core.windows.net/bierimage/{1}", blobName);

                    // Make the image object 
                    var imageObj = new Image(lon, lat, weather.main.temp, blobName, blobReference);

                    // Convert the image object into Json
                    string json = JsonConvert.SerializeObject(imageObj);

                    // Create the queue if it doesnt exist
                    CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
                    CloudQueue queue = queueClient.GetQueueReference("mapqueue");
                    await queue.CreateIfNotExistsAsync();
                    CloudBlockBlob blockBlob = cloudBlobContainer.GetBlockBlobReference(blobName);

                    // Add the Json message to the queue
                    var queueMessage = new CloudQueueMessage(json);
                    await queue.AddMessageAsync(queueMessage);

                    return new OkObjectResult("You will find your image at the following link: " + bloburl);
                } 
                else
                {
                    return new NotFoundObjectResult("The city & country combination provided has not been found");
                }


            } 
            else
            {
                return new BadRequestObjectResult("Please enter a city and its matching country");
            }

            async Task<Weather> GetWeatherData(string url)
            {
                Weather weather = null;
                HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    weather = await response.Content.ReadAsAsync<Weather>();
                }
                return weather;
            }
            string Formatcoords(double coords)
                {
                    string newcoords = coords.ToString();
                    string output = newcoords.Replace(",", ".");
                    return output;
                }
        }
    }
}
