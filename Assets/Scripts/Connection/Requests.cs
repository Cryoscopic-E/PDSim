using Newtonsoft.Json.Linq;
using System;

namespace PDSim.Connection
{
    public enum RequestType
    {
        TEST_CONNECTION,
        PARSE
    }


    public class BackendTestConnectionRequest : ServerRequest
    {
        public BackendTestConnectionRequest(Action<JObject> callback) : base(callback)
        {
            request.Add("request", "test");
        }
    }
    public class BackendParseRequest : ServerRequest
    {
        public BackendParseRequest(string domainPath, string problemPath, Action<JObject> cb) : base(cb)
        {
            request.Add("request", "parse");
            request.Add("domain_path", domainPath);
            request.Add("problem_path", problemPath);
        }
    }
}