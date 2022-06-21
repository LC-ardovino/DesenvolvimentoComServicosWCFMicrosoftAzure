using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Data.SqlClient;


namespace FunctionAppSocialNetwork
{
    public class FunctionUpdateViews
    {
        [FunctionName("FunctionUpdateViews")]
        public  void Run([QueueTrigger("friend-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            //log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            
            // Get the connection string from app settings and use it to create a connection.
            var connectionString = "Data Source=infnetsql.database.windows.net;Initial Catalog=InfnetDBSample;User ID=ServerAdmin;Password=luizufrj@123;Connect Timeout=30;Encrypt=True;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

            using (var conn = new SqlConnection(connectionString))
            {
                var procedureName = "UpdateDetailsViews";
                var sqlCommandUpdate = new SqlCommand(procedureName, conn);

                sqlCommandUpdate.CommandType = CommandType.StoredProcedure;
                sqlCommandUpdate.Parameters.AddWithValue("@Id", myQueueItem);

                conn.Open();
                sqlCommandUpdate.ExecuteNonQuery();
            }
        }
    }
}
