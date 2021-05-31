using System;
using System.IO;
using MongoDBConnection.utils;
using Newtonsoft.Json;
using Risk.Regression.Test.Contract.Utils;

namespace MongoDBConnection
{
    class Program
    {
        static void Main(string[] args)
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

        }
    }
}
