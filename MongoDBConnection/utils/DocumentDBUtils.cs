using System.Text;

namespace Risk.Regression.Test.Contract.Utils
{
    using System;
    using System.IO;
    using System.Security.Cryptography;


    using MongoDB.Driver;


    using Renci.SshNet;


    public class DocumentDbUtils
    {
        private static string localhostIp = "127.0.0.1";
        private static uint documentDbLocalBindPort = GetRandomPort();

        protected static string PrivateKeyFilePath = "ssh-risk.pem";

        private IMongoDatabase InitDocumentDb()
        {
            var riskBastionServerConnectionString = "";
            var documentDbConnectionString = "";
            var docDbUtis = new DocumentDbUtils(riskBastionServerConnectionString, documentDbConnectionString);
            var client = docDbUtis.CreateDocumentDbClient(PrivateKeyFilePath);
            var dbName = documentDbConnectionString.Split(",")[4];
            var database = docDbUtis.OpenDatabase(client, dbName);
            Console.WriteLine("Document DB Client created at Cluster ID: " + database.Client.Cluster.ClusterId);
            return database;
        }
        public string GenerateDocumentDbPrivateKey(string filepath,string pemBase64DecodedContent)
        {
            byte[] decodedBytes = Convert.FromBase64String(pemBase64DecodedContent);
            string pemString = Encoding.UTF8.GetString(decodedBytes);

            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }

            StreamWriter writer = File.CreateText(filepath);
            writer.WriteLine(pemString);
            writer.Close();
            return filepath;
        }
        private static uint GetRandomPort()
        {
            Random r = new Random();
            int rInt = r.Next(8000, 9000); 
            var port = Convert.ToUInt32(rInt);
            Console.WriteLine("Random port: "+port);
            return port;

        }

        private string _riskBastionServerConnectionString;
        private string _documentDbConnectionString;

        public DocumentDbUtils(string riskBastionServerConnectionString, string documentDbConnectionString)
        {
            _riskBastionServerConnectionString = riskBastionServerConnectionString;
            _documentDbConnectionString = documentDbConnectionString;
        }

        private string[] GetRiskDocumentDbConnectionDetails()
        {
            string connectionString = _documentDbConnectionString;
            string[] connectionDetail = new string[4];

            if (connectionString != null)
            {
                connectionDetail = connectionString.Split(",");
            }

            return connectionDetail;
        }

        private string[] GetRiskBastionServerDetails()
        {
            string connectionString = _riskBastionServerConnectionString;

            string[] connectionDetail = new string[3];

            if (connectionString != null)
            {
                connectionDetail = connectionString.Split(",");
            }

            return connectionDetail;
        }


        private string GetRiskDocumentDbConnectionStringWithLocalhostTunnel(string username, string password)
        {
            string template = "mongodb://<username>:<password>@localhost:<port>";
            template = template.Replace("<username>", username);
            template = template.Replace("<password>", password);
            string connectionString = template.Replace("<port>", documentDbLocalBindPort.ToString());
            Console.WriteLine("Final Connection string"+connectionString);
            return connectionString;
        }

        private void CreateSshTunnelAndPortForward(string privateKeyPath)
        {
            string[] sshServerDetails = GetRiskBastionServerDetails();
            string sshServerHost = sshServerDetails[0];
            int sshServerPort = Int32.Parse(sshServerDetails[1]);
            string sshServerUsername = sshServerDetails[2];

            Renci.SshNet.ConnectionInfo conn = new ConnectionInfo(
                sshServerHost,
                sshServerPort,
                sshServerUsername,
                new PrivateKeyAuthenticationMethod(sshServerUsername, new PrivateKeyFile(privateKeyPath, "")));

            SshClient sshClient = new SshClient(conn);
            sshClient.Connect();

            string[] connectionDetails = GetRiskDocumentDbConnectionDetails();

            ForwardedPortLocal forwardedPortLocal = new ForwardedPortLocal(
                localhostIp,
                documentDbLocalBindPort,
                connectionDetails[0],
                uint.Parse(connectionDetails[1]));

            sshClient.AddForwardedPort(forwardedPortLocal);
            forwardedPortLocal.Start();
        }

        public MongoClient CreateDocumentDbClient(string privateKeyPath)
        {
            CreateSshTunnelAndPortForward(privateKeyPath);
            string[] connectionDetails = GetRiskDocumentDbConnectionDetails();
            string connectionString = GetRiskDocumentDbConnectionStringWithLocalhostTunnel(connectionDetails[2], connectionDetails[3]);
            MongoClient client = new MongoClient(connectionString);
            return client;
        }

        public IMongoDatabase OpenDatabase(MongoClient client, string databaseName)
        {
            return client.GetDatabase(databaseName);
        }
    }
}