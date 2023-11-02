namespace PDSim.Connection
{
    public class BackendTestConnectionRequest : NetMqClientJson
    {
        public BackendTestConnectionRequest()
        {
            request.Add("request", "ping");
        }
    }

    public class ProtobufRequest : NetMqClientBytes
    {
        public ProtobufRequest(string requestType)
        {
            request.Add("request", requestType);
        }
    }
}