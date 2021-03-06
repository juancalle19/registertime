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
        [FunctionName(nameof(CreateRegistertime))]
        public static async Task<IActionResult> CreateRegistertime(
            [HttpTrigger(AuthorizationLevel.Anonymous,"post", Route = "registertime")] HttpRequest req,
            [Table("registertime", Connection ="AzureWebJobsStorage")] CloudTable registertimeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new register.");


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
                Time = DateTime.UtcNow,
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

        [FunctionName(nameof(UpdateRegistertimeIn))]
        public static async Task<IActionResult> UpdateRegistertimeIn(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "registertime/{id}")] HttpRequest req,
            [Table("registertime", Connection = "AzureWebJobsStorage")] CloudTable registertimeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for registertime: {id}, recieved");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Registertime registertime = JsonConvert.DeserializeObject<Registertime>(requestBody);

            //validate id
            TableOperation findOperation = TableOperation.Retrieve<RegistertimeEntity>("REGISTERTIME", id);
            TableResult findResult = await registertimeTable.ExecuteAsync(findOperation);

            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "NOT FOUND"
                }
                 );
            }

            //Update
            RegistertimeEntity registertimeEntity = (RegistertimeEntity)findResult.Result;

            if(registertime.Type == 0 || registertime.Type == 1)
            {
                registertimeEntity.Type = registertime.Type;
            }

            if(registertime.IdEmployee > 0)
            {
                registertimeEntity.IdEmployee = registertime.IdEmployee;
            }

            registertimeEntity.Time = registertime.Time;

            registertimeEntity.Consolidate = registertime.Consolidate;
           

            TableOperation addOperation = TableOperation.Replace(registertimeEntity);
            await registertimeTable.ExecuteAsync(addOperation);
            string message = $"Update register time in table, register table: {id}";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = registertimeEntity
            });
        }

        [FunctionName(nameof(GetAll))]
        public static async Task<IActionResult> GetAll(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registertime")] HttpRequest req,
            [Table("registertime", Connection = "AzureWebJobsStorage")] CloudTable registertimeTable,
            ILogger log)
        {
            log.LogInformation("Get All register");

            TableQuery<RegistertimeEntity> query = new TableQuery<RegistertimeEntity>();
            TableQuerySegment<RegistertimeEntity> registertime = await registertimeTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = registertime
            });
        }

        [FunctionName(nameof(GetRegisterById))]
        public static IActionResult GetRegisterById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registertime/{id}")] HttpRequest req,
            [Table("registertime", "REGISTERTIME", "{id}", Connection = "AzureWebJobsStorage")] RegistertimeEntity registertimeEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get registertime by id: {id}, recieved");

            if (registertimeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "NOT FOUND"
                });
            }
            string message = $"Registertime: {id}, retrieved";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = registertimeEntity
            });
        }

        [FunctionName(nameof(DeleteRegistertime))]
        public static async Task<IActionResult> DeleteRegistertime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "registertime/{id}")] HttpRequest req,
            [Table("registertime", "REGISTERTIME", "{id}", Connection = "AzureWebJobsStorage")] RegistertimeEntity registertimeEntity,
            [Table("registertime", Connection = "AzureWebJobsStorage")] CloudTable registertimeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"delete registertime id: {id}, recieved");

            if (registertimeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "NOT FOUND"
                });
            }

            await registertimeTable.ExecuteAsync(TableOperation.Delete(registertimeEntity));
            string message = $"Registertime: {id}, deleted";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = registertimeEntity
            });
        }



    }
}
