namespace PDSim.Connection
{
    public class BackendTestConnectionRequest : NetMqClient
    {
        public BackendTestConnectionRequest()
        {
            request.Add("request", "test");
        }
    }
    public class BackendParseRequest : NetMqClient
    {
        public BackendParseRequest(string domainPath, string problemPath)
        {
            request.Add("request", "parse");
            request.Add("domain_path", domainPath);
            request.Add("problem_path", problemPath);
        }
    }

    public class ProtobufRequest : NetMqClientBytes
    {
        public ProtobufRequest()
        {
            request.Add("request", "test-proto");
        }
    }
}