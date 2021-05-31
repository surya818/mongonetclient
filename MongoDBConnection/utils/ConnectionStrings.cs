using System;
using System.Collections.Generic;
using System.Text;

namespace MongoDBConnection.utils
{
    public class ConnectionStrings
    {
        public string DocumentDb { get; set; }
        public string RiskBastionServer { get; set; }
        public string AuthenticationKey { get; set; }
    }

    public class Root
    {
        public ConnectionStrings ConnectionStrings { get; set; }
    }

}
