using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BrandingPolice.Models;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;


namespace BrandingPolice.Controllers
{
    public static class Globals
    {
        // public static global::System.String ContainerName { get => containerName; set => containerName = value; 
        public static string ContainerName = "";

        public static void sGuID() => ContainerName += "" + Guid.NewGuid().ToString();
    }

    public class HomeController : Controller
    {

        // Retrieve the connection string for use with the application. The storage
        // connection string is stored in an environment variable on the machine
        // running the application called CONNECT_STR. If the
        // environment variable is created after the application is launched in a
        // console or with Visual Studio, the shell or application needs to be closed
        // and reloaded to take the environment variable into account.
        public string connectionString = "DefaultEndpointsProtocol=https;AccountName=blobwebapprandingpolice;AccountKey=KSHFoKrUVCNytbWDpeDQ9a1iboXBbUcDc2DxpgixpvngcYPmDaUD/LpuiV/HtJ2z/sjG1kFzJtcNQjSpjlj0hg==;EndpointSuffix=core.windows.net";
        //Create a unique name for the container
        //public string containerName = "containerbrandingpolice"/* + Guid.NewGuid().ToString()*/;
        public string queueName = "queuebrandingpolice";
        public string containerName = "containerbrandingpolice";


        [HttpGet]
        // public IActionResult Index()
        // {
        //     //hier sende ich mein Model(PowerpointFile) zu der View(Index)
        //     return View(new PowerpointFile());

        // }

        [HttpPost]
        public async Task<IActionResult> Index(PowerpointFile powerpointFile)
        {
            //Generating a Unique Identifier for the container
            Globals.sGuID();

            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "./data";
            //powerpointFile.FileTitle = powerpointFile.MyFile.FileName;
            string results_fileName = "result" + Globals.ContainerName + ".txt";
            string localFilePath_txt = Path.Combine(localPath, results_fileName);

            //Parsing the connection string
            Microsoft.WindowsAzure.Storage.CloudStorageAccount storageacc = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
            Microsoft.Azure.Storage.CloudStorageAccount queueStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(connectionString);

            // Create a BlobServiceClient object which will be used to create a container client
            CloudBlobClient blobClient = storageacc.CreateCloudBlobClient();

            CloudQueueClient queueClient = queueStorageAccount.CreateCloudQueueClient();

            // Retrieve a reference to a queue.
            CloudQueue queue = queueClient.GetQueueReference(queueName);

            // Create the queue if it doesn't already exist.
            queue.CreateIfNotExists();


            // Create the container and return a container client object
            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            await container.CreateIfNotExistsAsync();

            CloudBlockBlob blockBlob = container.GetBlockBlobReference(results_fileName);
            blockBlob.Properties.ContentType = "text/plain";

            // Write text to the file
            await System.IO.File.WriteAllTextAsync(localFilePath_txt, "Working on " + powerpointFile.MyFile.FileName + ".pptx");
            using (var filestream = System.IO.File.OpenRead(localFilePath_txt))
            {
                await blockBlob.UploadFromStreamAsync(filestream);
                //Send message to Queue
                CloudQueueMessage message = new CloudQueueMessage(blockBlob.Uri.ToString());
                queue.AddMessage(message);
            }

            if (Microsoft.Azure.Storage.CloudStorageAccount.TryParse(connectionString, out Microsoft.Azure.Storage.CloudStorageAccount storageAccount))
            {
                await container.CreateIfNotExistsAsync();

                //MS: Don't rely on or trust the FileName property without validation. The FileName property should only be used for display purposes.
                var picBlob = container.GetBlockBlobReference(Path.GetFileNameWithoutExtension(powerpointFile.MyFile.FileName) + Globals.ContainerName + ".pptx");

                await picBlob.UploadFromStreamAsync(powerpointFile.MyFile.OpenReadStream());
                //send message to Queue
                CloudQueueMessage message = new CloudQueueMessage(picBlob.Uri.ToString());
                queue.AddMessage(message);
            }

            return Redirect("/Home/LinkPage");

        }

        [HttpGet]
        public async Task<IActionResult> LinkPage()
        {

            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);

            // Create the container and return a container client object
            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            BlobLists blobLists = new BlobLists();
            blobLists.Links = new List<string>();
            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                blobLists.Links.Add(blobItem.Name);
            }
            ViewData["id"] = "result" + Globals.ContainerName + ".txt";

            return View(blobLists);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
