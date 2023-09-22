using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OrderItems
{
    public static class OrderDeliveryProcessor
    {
        [FunctionName(nameof(OrderDeliveryProcessor))]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
                databaseName: "cloudx-shop",
                containerName: "orders",
                Connection = "CosmosDBConnection", PartitionKey = "id", CreateIfNotExists = true)]
                IAsyncCollector<Order> orderItemsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<Order>(requestBody);

            await orderItemsOut.AddAsync(order);


            return new OkObjectResult("Done");
        }
    }

    public class Order
    {
        public string id { get; set; }

        [JsonPropertyName("shippingAddress")]
        public Address ShippingAddress { get; set; }

        [JsonPropertyName("orderItems")]
        public List<OrderItem> OrderItems { get; set; }

        [JsonPropertyName("finalPrice")]
        public decimal FinalPrice { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("street")]
        public string Street { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("zipCode")]
        public string ZipCode { get; set; }
    }

    public class OrderItem
    {
        [JsonPropertyName("units")]
        public int Units { get; set; }
        [JsonPropertyName("unitPrice")]
        public decimal UnitPrice { get; set; }
        [JsonPropertyName("catalogueItemId")]
        public int CatalogueItemId { get; set; }
    }
}
