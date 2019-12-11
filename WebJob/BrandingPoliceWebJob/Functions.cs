using System;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure; // Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage; // Namespace for CloudStorageAccount
using Microsoft.Azure.Storage.Queue; // Namespace for Queue storage types
using System.Linq;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Presentation;
using A = DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace BrandingPoliceWebJob
{
    public class Functions
    {
        public static string connectionString = "DefaultEndpointsProtocol=https;AccountName=blobwebapprandingpolice;AccountKey=KSHFoKrUVCNytbWDpeDQ9a1iboXBbUcDc2DxpgixpvngcYPmDaUD/LpuiV/HtJ2z/sjG1kFzJtcNQjSpjlj0hg==;EndpointSuffix=core.windows.net";
        //Create a unique name for the container
        public static string containerName = "containerbrandingpolice";
        public static string queueName = "queuebrandingpolice";
        public static string localFilePath_txt ;
        public static string filename_txt ;
        public static async Task ProcessQueueMessage([QueueTrigger("queuebrandingpolice")] string message, ILogger logger)
        {
            logger.LogInformation(message);

            try
            {
                 Microsoft.WindowsAzure.Storage.CloudStorageAccount storageacc = Microsoft.WindowsAzure.Storage.CloudStorageAccount.Parse(connectionString);
                // Create a BlobServiceClient object which will be used to create a container client
                CloudBlobClient blobCliente = storageacc.CreateCloudBlobClient();
                // Create the container and return a container client object
                CloudBlobContainer container = blobCliente.GetContainerReference(containerName);
                await container.CreateIfNotExistsAsync();

                // Retrieve storage account from connection string.
                Microsoft.Azure.Storage.CloudStorageAccount queueStorageAccount = Microsoft.Azure.Storage.CloudStorageAccount.Parse(connectionString);
                CloudQueueClient queueClient = queueStorageAccount.CreateCloudQueueClient();
                // Retrieve a reference to a queue.
                CloudQueue queue = queueClient.GetQueueReference(queueName);

                // Create a BlobServiceClient object which will be used to create a container client
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                // Create the container and return a container client object
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // Get the message from the queue and update the message contents.
                CloudQueueMessage messagequeue = queue.GetMessage();
                // Get the next message
                //CloudQueueMessage retrievedMessage = queue.GetMessage();


                string localPath = "";
                string filename_pptx = null;
                string GetExtension = null;

                Uri uri = new Uri(message);

                GetExtension = System.IO.Path.GetExtension(uri.LocalPath);
                
                if (GetExtension == ".txt")
                { 
                    filename_txt = System.IO.Path.GetFileName(uri.LocalPath);
                    localFilePath_txt = Path.Combine(localPath, filename_txt);
                }
                if (GetExtension != ".txt")
                { 
                    filename_pptx = System.IO.Path.GetFileName(uri.LocalPath);
                    CloudBlockBlob blockBlob_txt = container.GetBlockBlobReference(filename_txt);
                    blockBlob_txt.Properties.ContentType = "text/plain";

                    // Get a reference to a blob
                    BlobClient blobClient_pptx = containerClient.GetBlobClient(filename_pptx);

                    BlobDownloadInfo download = blobClient_pptx.Download();
                    using (FileStream file1 = File.OpenWrite(filename_pptx))
                    {
                        download.Content.CopyTo(file1);
                        file1.Close();
                        int numberOfSlides = CountSlides(file1.Name);
                        string result = "", search = "Windows Azure";
                        System.Console.WriteLine("Number of slides = {0}", numberOfSlides);
                        string slideText;
                        for (int i = 0; i < numberOfSlides; i++)
                        {
                            GetSlideIdAndText(out slideText, file1.Name, i);
                            System.Console.WriteLine("Slide #{0} contains: {1}", i + 1, slideText);
                            // append text to the file
                            //await System.IO.File.AppendAllTextAsync(localFilePath_txt, slideText);
                            if (slideText.Contains(search))
                                System.Console.WriteLine("\nRETURN 0\n", i + 1, slideText);
                                result += result + String.Format("In der {0}.Slide benutzen Sie den Begriff Windows Azure statt Microsoft Azure\n", i + 1);
                        }
                        await System.IO.File.AppendAllTextAsync(localFilePath_txt, result);
                        using (var filestream = System.IO.File.OpenRead(localFilePath_txt))
                        {
                            await blockBlob_txt.UploadFromStreamAsync(filestream);
                        }
                    }

                    //Delete .pptx blob
                    await blobClient_pptx.DeleteIfExistsAsync();
                    

                    filename_txt = null;
                    localFilePath_txt = null;
                }
            }
            catch (System.Exception)
            {
                throw;
            }

        }
        public static int CountSlides(string presentationFile)
        {
            // Open the presentation as read-only.
            using (PresentationDocument presentationDocument = PresentationDocument.Open(presentationFile, false))
            {
                // Pass the presentation to the next CountSlides method
                // and return the slide count.
                return CountSlides(presentationDocument);
            }
        }

        // Count the slides in the presentation.
        public static int CountSlides(PresentationDocument presentationDocument)
        {
            // Check for a null document object.
            if (presentationDocument == null)
            {
                throw new ArgumentNullException("presentationDocument");
            }

            int slidesCount = 0;

            // Get the presentation part of document.
            PresentationPart presentationPart = presentationDocument.PresentationPart;
            // Get the slide count from the SlideParts.
            if (presentationPart != null)
            {
                slidesCount = presentationPart.SlideParts.Count();
            }
            // Return the slide count to the previous method.
            return slidesCount;
        }

        public static void GetSlideIdAndText(out string sldText, string docName, int index)
        {
            using (PresentationDocument ppt = PresentationDocument.Open(docName, false))
            {
                // Get the relationship ID of the first slide.
                PresentationPart part = ppt.PresentationPart;
                OpenXmlElementList slideIds = part.Presentation.SlideIdList.ChildElements;

                string relId = (slideIds[index] as SlideId).RelationshipId;

                // Get the slide part from the relationship ID.
                SlidePart slide = (SlidePart)part.GetPartById(relId);

                // Build a StringBuilder object.
                StringBuilder paragraphText = new StringBuilder();

                // Get the inner text of the slide:
                IEnumerable<A.Text> texts = slide.Slide.Descendants<A.Text>();
                foreach (A.Text text in texts)
                {
                    paragraphText.Append(text.Text);
                }
                sldText = paragraphText.ToString();
            }
        }

    }
}