using System;
using System.IO;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDBConnection.utils;
using Newtonsoft.Json;
using Risk.Regression.Test.Contract.Utils;

namespace MongoDBConnection
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string configPath = "/app/secret.json";
            string pemPath = "/app/ssh.pem";
            string jsonStr = File.ReadAllText(configPath);
            Console.WriteLine(jsonStr);
            Root connection = JsonConvert.DeserializeObject<Root>(jsonStr);
            string authKey = connection.ConnectionStrings.AuthenticationKey;
            Console.WriteLine(authKey);
            DocumentDbUtils dbutils = new DocumentDbUtils(connection.ConnectionStrings.RiskBastionServer,connection.ConnectionStrings.DocumentDb);
            dbutils.GenerateDocumentDbPrivateKey(@pemPath, authKey);
            var client = dbutils.CreateDocumentDbClient(@pemPath);
            Console.WriteLine("Server description"+client.Cluster.Description);
            Console.WriteLine("ClusterId" + client.Cluster.ClusterId);
            var dbName = "risk-docdb-stage-cluster";
            var database = dbutils.OpenDatabase(client, dbName);
            var records = database.GetCollection<BsonDocument>("features");
            var query = "beneficiaryAccountId/" + "0690000031";
            Console.WriteLine("Document DB Filter query: " + query);
            var filter = Builders<BsonDocument>.Filter.Eq("_id", query);
            Console.WriteLine("Document DB Filter query: " + filter.ToJson());
            await records.DeleteManyAsync(filter);
            Console.WriteLine("Delete Successful!!!");

        }
    }
}
