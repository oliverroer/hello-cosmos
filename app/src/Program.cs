using System;
using Microsoft.Azure.Documents;

namespace HelloCosmos {
    public class Program {
        public static void Main(string[] args) {
            const string endpointUri = "http://host.docker.internal:8082";
            const string primaryKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
            var demo = new GetStartedDemo(endpointUri, primaryKey);
            demo.Run().Wait();
        }
    }
}
