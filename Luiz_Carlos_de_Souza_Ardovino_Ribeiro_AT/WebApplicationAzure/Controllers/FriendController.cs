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

namespace WebApplicationAzure.Controllers
{
    public class FriendController : Controller
    {
        // GET: FriendController
        public ActionResult Index()
        {
            var friends = new List<FriendModel>();
            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "ReadFriends";
                var sqlCommand = new SqlCommand(procedureName, connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;

                try
                {
                    connection.Open();
                    using (var reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var friend = new FriendModel
                                {
                                    Id = (int)reader["Id"],
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    BirthDate = (DateTime)reader["BirthDate"]
                                };
                                friends.Add(friend);
                            }
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return View(friends);
        }

        // GET: FriendController/Details/5
        public ActionResult Details(int id)
        {
            FriendModel friend = null;

            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                //
                var procedureName = "UpdateDetailsViews";
                var sqlCommandUpdate = new SqlCommand(procedureName, connection);

                sqlCommandUpdate.CommandType = CommandType.StoredProcedure;
                sqlCommandUpdate.Parameters.AddWithValue("@Id", id);

                connection.Open();
                sqlCommandUpdate.ExecuteNonQuery();
  
                //


                procedureName = "ReadFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);

                try
                {
        
                    using (var reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            friend = new FriendModel
                            {
                                Id = (int)reader["Id"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                BirthDate = (DateTime)reader["BirthDate"],
                                Views = (int)reader["Views"],
                                PictureUrl = reader["PictureUrl"].ToString()
                            };
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
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

            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "./Imagens/";
            string fileName = Request.Form["PictureUrl"];
            string localFilePath = Path.Combine(localPath, fileName);



            FriendModel friends = null;

            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "CreateFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                string id = Request.Form["Id"];
                string firstName = Request.Form["FirstName"];
                string lastName = Request.Form["LastName"];
                string birthDate = Request.Form["BirthDate"];
                string pictureUrl = await CreateBlob(localFilePath);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlCommand.Parameters.AddWithValue("@FirstName", firstName);
                sqlCommand.Parameters.AddWithValue("@LastName", lastName);
                sqlCommand.Parameters.AddWithValue("@BirthDate", birthDate);
                sqlCommand.Parameters.AddWithValue("@PictureUrl", pictureUrl);

                try
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
            return View(friends);
        }

        // GET: FriendController/Edit/5
        public ActionResult Edit(int id)
        {


            FriendModel friend = null;

            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "ReadFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    using (var reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            friend = new FriendModel
                            {
                                Id = (int)reader["Id"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                BirthDate = (DateTime)reader["BirthDate"],
                                PictureUrl = reader["PictureUrl"].ToString()
                            };
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return View(friend);
        }

        // POST: FriendController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, IFormCollection collection)
        {

            string pictureUrl = "";

            // Create a local file in the ./data/ directory for uploading and downloading
            string localPath = "./Imagens/";
            string fileName = Request.Form["PictureUrlNovo"];
            string fileUrlOld = Request.Form["PictureUrl"];
            string fileNameOld = "";

            if (fileUrlOld != string.Empty)
            {
                var uri = new Uri(fileUrlOld);
                fileNameOld = Path.GetFileName(uri.LocalPath);
            }
          

            string localFilePath = Path.Combine(localPath, fileName);

           
            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "UpdateFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                string firstName = Request.Form["FirstName"];
                string lastName = Request.Form["LastName"];
                string birthDate = Request.Form["BirthDate"];
                if (fileName != string.Empty)
                {
                    if (fileUrlOld != string.Empty)
                    {
                        DeleteBlob(fileNameOld);
                    }
                    pictureUrl = await CreateBlob(localFilePath);
                }

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);
                sqlCommand.Parameters.AddWithValue("@FirstName", firstName);
                sqlCommand.Parameters.AddWithValue("@LastName", lastName);
                sqlCommand.Parameters.AddWithValue("@BirthDate", birthDate);
                sqlCommand.Parameters.AddWithValue("@PictureUrl", pictureUrl);

                try
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: FriendController/Delete/5
        public ActionResult Delete(int id)
        {
            FriendModel friend = null;

            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "ReadFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    using (var reader = sqlCommand.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        if (reader.Read())
                        {
                            friend = new FriendModel
                            {
                                Id = (int)reader["Id"],
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                BirthDate = (DateTime)reader["BirthDate"],
                                PictureUrl = reader["PictureUrl"].ToString()
                            };
                        }
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return View(friend);
        }

        // POST: FriendController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {

            string fileUrl = Request.Form["PictureUrl"];
            string fileName = "";

            if (fileUrl != string.Empty)
            {
                var uri = new Uri(fileUrl);
                fileName = "/Imagens/" + Path.GetFileName(uri.LocalPath);
                DeleteBlob(fileName);
            }

            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var connection = new SqlConnection(connectionString))
            {
                var procedureName = "DeleteFriend";
                var sqlCommand = new SqlCommand(procedureName, connection);

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    connection.Close();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> CreateBlob(string fileName)

        {
            string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storageardovino;AccountKey=rIWRyN0vVMDBY5b1mPK+gOCKOhowKUajYFQ0eOsbDQCFEk5T7YglInupgt6ILzdyQPaQ6qEja9tF+AStkGRo1g==;EndpointSuffix=core.windows.net";

            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var blobContainerName = "imagensardovino";
            var blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);

            BlobClient blobClient = blobContainer.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
            await blobClient.UploadAsync(fileName);
            return blobClient.Uri.AbsoluteUri;
        }

        private async void DeleteBlob(string fileName)

        {
            string StorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=storageardovino;AccountKey=rIWRyN0vVMDBY5b1mPK+gOCKOhowKUajYFQ0eOsbDQCFEk5T7YglInupgt6ILzdyQPaQ6qEja9tF+AStkGRo1g==;EndpointSuffix=core.windows.net";

            BlobServiceClient blobServiceClient = new BlobServiceClient(StorageConnectionString);
            var blobContainerName = "imagensardovino";
            var blobContainer = blobServiceClient.GetBlobContainerClient(blobContainerName);

            BlobClient blobClient = blobContainer.GetBlobClient(fileName);

            await blobClient.DeleteIfExistsAsync();
        }

        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }


    }
}
