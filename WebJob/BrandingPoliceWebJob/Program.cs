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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BrandingPoliceWebJob
{

    class Program
    {

        static void Main(string[] args)
        {
            /*
            * https://docs.microsoft.com/fr-fr/azure/app-service/webjobs-sdk-get-started
            */
            var builder = new HostBuilder();
            builder.ConfigureWebJobs(b =>
                    {
                        b.AddAzureStorageCoreServices();
                        b.AddAzureStorage();
                    });
            builder.ConfigureLogging((context, b) =>
                    {
                        b.AddConsole();
                    });       
            var host = builder.Build();
            using (host)
            {
                host.Run();
            }
        }
/* 
        static async Task MainAsync()
        {
        } */
    }
}