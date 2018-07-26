using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using System.Net.Http;
using Newtonsoft.Json;

namespace HelloCosmos {
    public class GetStartedDemo {
        private DocumentClient Client;

        public GetStartedDemo(string endpointUri, string primaryKey) {
            Client = new DocumentClient(new Uri(endpointUri), primaryKey, new ConnectionPolicy() {
                EnableEndpointDiscovery = false
            });
        }

        public async Task Run() {
            await Client.CreateDatabaseIfNotExistsAsync(new Database { Id = "FamilyDB_oa" });

            await Client.CreateDocumentCollectionIfNotExistsAsync(UriFactory.CreateDatabaseUri("FamilyDB_oa"), new DocumentCollection { Id = "FamilyCollection_oa" });

            Family andersenFamily = new Family {
                Id = "Andersen.1",
                LastName = "Andersen",
                Parents = new Parent[] {
                new Parent { FirstName = "Thomas" },
                    new Parent { FirstName = "Mary Kay" }
                },
                Children = new Child[] {
                    new Child {
                        FirstName = "Henriette Thaulow",
                        Gender = "female",
                        Grade = 5,
                        Pets = new Pet[] {
                            new Pet { GivenName = "Fluffy" }
                        }
                    }
                },
                Address = new Address { State = "WA", County = "King", City = "Seattle" },
                IsRegistered = true
            };

            await CreateFamilyDocumentIfNotExists("FamilyDB_oa", "FamilyCollection_oa", andersenFamily);

            Family wakefieldFamily = new Family {
                Id = "Wakefield.7",
                LastName = "Wakefield",
                Parents = new Parent[] {
                    new Parent { FamilyName = "Wakefield", FirstName = "Robin" },
                    new Parent { FamilyName = "Miller", FirstName = "Ben" }
                },
                Children = new Child[] {
                    new Child {
                        FamilyName = "Merriam",
                        FirstName = "Jesse",
                        Gender = "female",
                        Grade = 8,
                        Pets = new Pet[] {
                            new Pet { GivenName = "Goofy" },
                            new Pet { GivenName = "Shadow" }
                        }
                    },
                    new Child {
                        FamilyName = "Miller",
                        FirstName = "Lisa",
                        Gender = "female",
                        Grade = 1
                    }
                },
                Address = new Address { State = "NY", County = "Manhattan", City = "NY" },
                IsRegistered = false
            };

            await CreateFamilyDocumentIfNotExists("FamilyDB_oa", "FamilyCollection_oa", wakefieldFamily);

            ExecuteSimpleQuery("FamilyDB_oa", "FamilyCollection_oa");

            andersenFamily.Children[0].Grade = 6;

            await ReplaceFamilyDocument("FamilyDB_oa", "FamilyCollection_oa", "Andersen.1", andersenFamily);

            ExecuteSimpleQuery("FamilyDB_oa", "FamilyCollection_oa");

            await DeleteFamilyDocument("FamilyDB_oa", "FamilyCollection_oa", "Andersen.1");

            await Client.DeleteDatabaseAsync(UriFactory.CreateDatabaseUri("FamilyDB_oa"));
        }

        private async Task CreateFamilyDocumentIfNotExists(string databaseName, string collectionName, Family family) {
            try {
                await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, family.Id));
                WriteToConsoleAndPromptToContinue("Found {0}", family.Id);
            } catch (DocumentClientException de) {
                if (de.StatusCode == HttpStatusCode.NotFound) {
                    await Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), family);
                    WriteToConsoleAndPromptToContinue("Created Family {0}", family.Id);
                } else {
                    throw;
                }
            }
        }

        private void ExecuteSimpleQuery(string databaseName, string collectionName) {
            // Set some common query options
            FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

            // Here we find the Andersen family via its LastName
            IQueryable<Family> familyQuery = Client.CreateDocumentQuery<Family>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(f => f.LastName == "Andersen");

            // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
            Console.WriteLine("Running LINQ query...");
            foreach (Family family in familyQuery) {
                Console.WriteLine("\tRead {0}", family);
            }

            // Now execute the same query via direct SQL
            IQueryable<Family> familyQueryInSql = Client.CreateDocumentQuery<Family>(
                    UriFactory.CreateDocumentCollectionUri(databaseName, collectionName),
                    "SELECT * FROM Family WHERE Family.LastName = 'Andersen'",
                    queryOptions);

            Console.WriteLine("Running direct SQL query...");
            foreach (Family family in familyQueryInSql) {
                Console.WriteLine("\tRead {0}", family);
            }
        }

        private async Task ReplaceFamilyDocument(string databaseName, string collectionName, string familyName, Family updatedFamily) {
            await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, familyName), updatedFamily);
            WriteToConsoleAndPromptToContinue("Replaced Family {0}", familyName);
        }

        private async Task DeleteFamilyDocument(string databaseName, string collectionName, string documentName) {
            await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(databaseName, collectionName, documentName));
            Console.WriteLine("Deleted Family {0}", documentName);
        }

        private static void WriteToConsoleAndPromptToContinue(string format, params object[] args) {
            Console.WriteLine(format, args);
        }
    }
}
