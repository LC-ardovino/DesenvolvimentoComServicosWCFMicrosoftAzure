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
using Microsoft.Extensions.Configuration;
using App.BLL.Models;
using App.DAL.Repositories;


namespace FunctionAppSocialNetwork
{
    public class FunctionUpdateViews
    {

        [FunctionName("FunctionUpdateViews")]
        public  void Run([QueueTrigger("friend-queue", Connection = "AzureWebJobsStorage")] string myQueueItem)
        {
            //log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");

            var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");

            using (var conn = new SqlConnection(connectionString))
            {
                string queryString = "UPDATE dbo.Friends SET Views =  ISNULL(Views,0) + 1 WHERE Id = " + myQueueItem;
                SqlCommand command = new SqlCommand(queryString, conn);
                command.Connection.Open();
                command.ExecuteNonQuery();
                command.Connection.Close();
            }
        }
    }
}
