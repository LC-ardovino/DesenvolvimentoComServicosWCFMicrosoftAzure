using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;
using WebApplicationAzure.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using System.Configuration;
using App.BLL.Models;
using App.DAL.Repositories;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace WebApplicationAzure.Controllers
{
    public class FriendController : Controller
    {
        private IConfiguration Configuration;
        private readonly IFriendRepository _repository;

        public FriendController(IConfiguration _configuration, IFriendRepository repository)
        {
            Configuration = _configuration;
            _repository = repository;
        }

        // GET: FriendController
        public async Task<IActionResult> Index()
        {

            var result = await _repository.GetAll();
            List<Friend> friends = result.ToList();
            return View(friends);
        }

        // GET: FriendController/Details/5
        public async Task<ActionResult<Friend>> Details(int id)
        {

            if (CreateQueue("friend-queue"))
            {
                InsertMessage("friend-queue", id.ToString());
            }

            Friend friend = null;

            friend = await _repository.GetById(id);
            return View(friend);
        }

        // GET: FriendController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: FriendController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(IFormCollection collection)
        {

            string localPath = "./Imagens/";
            string fileName = Request.Form["PictureUrl"];
            

            Friend friend = new Friend();

            try
            {               
                friend.FirstName = Request.Form["FirstName"];
                friend.LastName = Request.Form["LastName"];
                friend.BirthDate = Convert.ToDateTime(Request.Form["BirthDate"]);
                if (fileName != "")
                {
                    string localFilePath = Path.Combine(localPath, fileName);
                    friend.PictureUrl = await CreateBlob(localFilePath);
                }
 
                _repository.Create(friend);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View(friend);
            }

            return View(friend);
        }

        // GET: FriendController/Edit/5
        public async Task<IActionResult> Edit(int id)
        {

            var friend = await _repository.GetById(id);
            return View(friend);
        }

        // POST: FriendController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {
            Friend friend = new Friend();

            string pictureUrl = "";

            // Create a local file in the ./Imagens/ directory for uploading and downloading
            string localPath = "./Imagens/";
            string fileName = Request.Form["PictureUrlNovo"];
            string fileUrlOld = Request.Form["PictureUrl"];
            string fileNameOld = "";
            string localFilePath = "";

            if (fileUrlOld != string.Empty )
            {
                var uri = new Uri(fileUrlOld);
                fileNameOld = Path.GetFileName(uri.LocalPath);
                localFilePath = fileUrlOld;
                friend.PictureUrl = localFilePath;
            }


            try
            {
                if (fileName != string.Empty)
                {
                    if (fileUrlOld != string.Empty)
                    {
                        fileNameOld = Path.Combine(localPath, fileNameOld);
                        DeleteBlob(fileNameOld);
                    }
                    localFilePath = Path.Combine(localPath, fileName);
                    pictureUrl = await CreateBlob(localFilePath);
                    friend.PictureUrl = pictureUrl;// await CreateBlob(localFilePath);

                }

                friend.Id = int.Parse(Request.Form["Id"]);
                friend.FirstName = Request.Form["FirstName"];
                friend.LastName = Request.Form["LastName"];
                friend.BirthDate = Convert.ToDateTime(Request.Form["BirthDate"]);

                _repository.Update(friend);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }

        }

        // GET: FriendController/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var friend = await _repository.GetById(id);
            return View(friend);
        }

        // POST: FriendController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {

            string fileUrl = Request.Form["PictureUrl"];
            string fileName = "";

            try
            {
                Friend friend = new Friend();

                if (fileUrl != string.Empty)
                {
                    var uri = new Uri(fileUrl);
                    fileName = "/Imagens/" + Path.GetFileName(uri.LocalPath);
                    DeleteBlob(fileName);
                }

                friend.Id = id;

                _repository.Delete(friend);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        private async Task<string> CreateBlob(string fileName)

        {
            string StorageConnectionString = this.Configuration.GetSection("AppSettings")["StorageConnectionString"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);

            var blobContainerName = this.Configuration.GetSection("AppSettings")["BlobContainerName"]; //"imagensardovino";
            var blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);

            BlobClient blobClient = blobContainer.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
            await blobClient.UploadAsync(fileName);
            return blobClient.Uri.AbsoluteUri;
        }

        private async void DeleteBlob(string fileName)

        {
            string StorageConnectionString = this.Configuration.GetSection("AppSettings")["StorageConnectionString"];

            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var blobContainerName = this.Configuration.GetSection("AppSettings")["BlobContainerName"]; //"imagensardovino";
            var blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);

            BlobClient blobClient = blobContainer.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }

        //Create the queue service client
        public void CreateQueueClient(string queueName)
        {
            // Get the connection string from app settings
            var connectionString = System.Configuration.ConfigurationManager.AppSettings["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to create and manipulate the queue
            QueueClient queueClient = new QueueClient(connectionString, queueName);
        }

       

        //-------------------------------------------------
        // Create a message queue
        //-------------------------------------------------
        public bool CreateQueue(string queueName)
        {
            try
            {
                string StorageConnectionString = this.Configuration.GetSection("AppSettings")["StorageConnectionString"];
                
                // Instantiate a QueueClient which will be used to create and manipulate the queue
                QueueClient queueClient = new QueueClient(StorageConnectionString, queueName);

                // Create the queue
                queueClient.CreateIfNotExists();

                if (queueClient.Exists())
                {
                    Console.WriteLine($"Queue created: '{queueClient.Name}'");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}\n\n");
                Console.WriteLine($"Make sure the Azurite storage emulator running and try again.");
                return false;
            }
        }

        //-------------------------------------------------
        // Insert a message into a queue
        //-------------------------------------------------
        public void InsertMessage(string queueName, string message)
        {
            // Get the connection string from app settings
            string connectionString = this.Configuration.GetSection("AppSettings")["StorageConnectionString"];

            // Instantiate a QueueClient which will be used to create and manipulate the queue

            QueueClient queueClient = new QueueClient(connectionString, queueName, new QueueClientOptions
            {
                MessageEncoding = QueueMessageEncoding.Base64
            });

            // Create the queue if it doesn't already exist
            queueClient.CreateIfNotExists();

            if (queueClient.Exists())
            {
                // Send a message to the queue
                queueClient.SendMessage(message);
            }

            Console.WriteLine($"Inserted: {message}");
        }


    }
}
