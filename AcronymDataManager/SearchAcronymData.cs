using AcronymDataManager.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AcronymDataManager
{
    public static class SearchAcronymData
    {
        [FunctionName("SearchAcronymData")]
        public static async Task<object> Run([HttpTrigger(WebHookType = "genericJson")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info($"Webhook was triggered!");

            var documentClient = new DocumentClient(new Uri(ConfigurationManager.AppSettings["DocumentUri"]), ConfigurationManager.AppSettings["DocumentKey"]);

            string jsonContent = await req.Content.ReadAsStringAsync();
            var input = JsonConvert.DeserializeObject<QueryInput>(jsonContent);

            if (input == null || String.IsNullOrWhiteSpace(input.Acronym))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, new
                {
                    error = "Please pass a valid acronym."
                });
            }

            var databaseName = ConfigurationManager.AppSettings["DatabaseName"];
            var database = await documentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = databaseName });
            var databaseLink = UriFactory.CreateDatabaseUri(databaseName);

            var wordDefinitionCollection = await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseLink, new DocumentCollection() { Id = "WordDefinition" });
            var wordDefinitionCollectionLink = UriFactory.CreateDocumentCollectionUri(databaseName, "WordDefinition");

            var wordDefinitionQuery = documentClient.CreateDocumentQuery<WordDefinition>(wordDefinitionCollectionLink);
            var results = wordDefinitionQuery.Where(wd => wd.Acronym == input.Acronym);

            return req.CreateResponse(HttpStatusCode.OK, results);
        }
    }
}
