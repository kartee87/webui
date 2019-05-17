using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace testwebapp.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			// Set Ids of your Azure account  
			AzureOperations.applicationId = "2801cb4a-8cf8-4a4d-8886-d51b0dbe375a";
			AzureOperations.clientSecret = "Wv6kKHJup4d3vR7WvtVrlE]GTKfIN=@=";
			AzureOperations.tenantId = "188285f7-8f1e-4c0d-a0bc-797e3e38c5b3";

			//Demo Upload File  
			string srcPathToUpload = string.Format(@"C:\blobtest.txt");
			UploadFile(srcPathToUpload);


			//Demo Download File  
			string azurePathInBlob = "blobtest1.txt";
			string destinationPath = string.Format(@"C:\newfolder\myfile.txt");
			DownloadFile(destinationPath, azurePathInBlob);

			ViewBag.Title = "Home Page";
			return View();
		}
		//static void Main(string[] args)
		//{

		//}

		public static void UploadFile(string srcPath)
		{
			AzureOperationHelper azureOperationHelper = new AzureOperationHelper();
			// your Storage Account Name  
			azureOperationHelper.storageAccountName = "wpptestblob";
			azureOperationHelper.storageEndPoint = "core.windows.net";
			// File path to upload  
			azureOperationHelper.srcPath = srcPath;
			// Your Container Name   
			azureOperationHelper.containerName = "sample-container";
			// Destination Path you can set it file name or if you want to put it in folders do it like below  
			azureOperationHelper.blobName = string.Format("dev/files/" + Path.GetFileName(srcPath));
			AzureOperations.UploadFile(azureOperationHelper);

		}

		public static void DownloadFile(string destinationPath, string srcPath)
		{
			AzureOperationHelper azureOperationHelper = new AzureOperationHelper();
			// your Storage Account Name  
			azureOperationHelper.storageAccountName = "wpptestblob";
			azureOperationHelper.storageEndPoint = "core.windows.net";
			// Destination Path where you want to download file  
			azureOperationHelper.destinationPath = destinationPath;
			// Your Container Name   
			azureOperationHelper.containerName = "sample-container";
			// Blob Path in container where to download File  
			azureOperationHelper.blobName = srcPath;

			AzureOperations.DownloadFile(azureOperationHelper);

		}
		public static class AzureOperations
		{
			#region ConfigParams  
			public static string tenantId;
			public static string applicationId;
			public static string clientSecret;
			#endregion
			public static void UploadFile(AzureOperationHelper azureOperationHelper)
			{
				CloudBlobContainer blobContainer = CreateCloudBlobContainer(tenantId, applicationId, clientSecret, azureOperationHelper.storageAccountName, azureOperationHelper.containerName, azureOperationHelper.storageEndPoint);
				blobContainer.CreateIfNotExists();
				CloudBlockBlob blob = blobContainer.GetBlockBlobReference(azureOperationHelper.blobName);
				blob.UploadFromFile(azureOperationHelper.srcPath);
			}
			public static void DownloadFile(AzureOperationHelper azureOperationHelper)
			{
				CloudBlobContainer blobContainer = CreateCloudBlobContainer(tenantId, applicationId, clientSecret, azureOperationHelper.storageAccountName, azureOperationHelper.containerName, azureOperationHelper.storageEndPoint);
				CloudBlockBlob blob = blobContainer.GetBlockBlobReference(azureOperationHelper.blobName);
				blob.DownloadToFile(azureOperationHelper.destinationPath, FileMode.OpenOrCreate);
			}
			private static CloudBlobContainer CreateCloudBlobContainer(string tenantId, string applicationId, string clientSecret, string storageAccountName, string containerName, string storageEndPoint)
			{
				string accessToken = GetUserOAuthToken(tenantId, applicationId, clientSecret);
				TokenCredential tokenCredential = new TokenCredential(accessToken);
				StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
				CloudStorageAccount cloudStorageAccount = new CloudStorageAccount(storageCredentials, storageAccountName, storageEndPoint, useHttps: true);
				CloudBlobClient blobClient = cloudStorageAccount.CreateCloudBlobClient();
				CloudBlobContainer blobContainer = blobClient.GetContainerReference(containerName);
				return blobContainer;
			}
			static string GetUserOAuthToken(string tenantId, string applicationId, string clientSecret)
			{
				const string ResourceId = "https://storage.azure.com/";
				const string AuthInstance = "https://login.microsoftonline.com/{0}/";
				string authority = string.Format(CultureInfo.InvariantCulture, AuthInstance, tenantId);
				AuthenticationContext authContext = new AuthenticationContext(authority);
				var clientCred = new ClientCredential(applicationId, clientSecret);
				AuthenticationResult result = authContext.AcquireTokenAsync(ResourceId, clientCred).Result;
				return result.AccessToken;
			}
		}
	}

	public class AzureOperationHelper
	{
		public string blobName { get; set; }
		public string srcPath { get; set; }
		public string destinationPath { get; set; }
		public string storageAccountName { get; set; }
		public string containerName { get; set; }
		public string storageEndPoint { get; set; }

	}
}