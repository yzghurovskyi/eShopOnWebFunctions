using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System;
using Polly;

namespace OrderItemsReserver
{
    public static class OrderItemsReserver
    {
        [FunctionName(nameof(OrderItemsReserver))]
        public static async Task Run(
            [ServiceBusTrigger("orders", Connection = "ServiceBusConnection")] string queueOrderItem,
            [Blob("order-items/{rand-guid}.json", FileAccess.Write, Connection = "OrderStorageConnection")] Stream output,
            ILogger log)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
            log.LogInformation("C# HTTP trigger function processed a request.");
            try
            {

                await policy.ExecuteAsync(async Task () =>
                {
                    log.LogInformation("Stepping out in function");
                    var outputStream = new StreamWriter(output);
                    //throw new Exception("Custom exception");
                    await outputStream.WriteAsync(queueOrderItem);
                    await outputStream.FlushAsync();
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                throw;
            }
        }
    }
}
