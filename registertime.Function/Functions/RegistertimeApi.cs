using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Table;
using registertime.Common.Models;
using registertime.Common.Responses;
using registertime.Function.Entities;

namespace registertime.Function.Functions
{
    public static class RegistertimeApi
    {
        [FunctionName(nameof(CreateRegistertimeIn))]
        public static async Task<IActionResult> CreateRegistertimeIn(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = "registertime")] HttpRequest req,
            [Table("registertime", Connection ="AzureWebJobsStorage")] CloudTable registertimeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new entrance.");

            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Registertime registertime = JsonConvert.DeserializeObject<Registertime>(requestBody);

            if(registertime.IdEmployee == 0 || registertime.IdEmployee < 0 )
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "ID employee is equal 0 or less 0"
                }
                 );

            }else if (registertime.Type < 0 || registertime.Type > 1)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Type is different number 1 o number 0"
                }
                 );
            }

            RegistertimeEntity registertimeEntity = new RegistertimeEntity
            {
                ETag = "*",
                PartitionKey= "REGISTERTIME",
                RowKey = Guid.NewGuid().ToString(),
                IdEmployee = registertime.IdEmployee,
                Time = registertime.Time,
                Type = registertime.Type,
                Consolidate = registertime.Consolidate
            };

            TableOperation addOperation = TableOperation.Insert(registertimeEntity);
            await registertimeTable.ExecuteAsync(addOperation);
            string message = "New register time in table";
            log.LogInformation(message);

            return new OkObjectResult( new Response
            {
                IsSuccess= true,
                Message = message,
                Result = registertimeEntity
            });
        }
    }
}
