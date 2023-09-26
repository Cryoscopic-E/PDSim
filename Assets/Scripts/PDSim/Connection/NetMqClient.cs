using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using UnityEngine;

namespace PDSim.Connection
{

    public abstract class NetMqClient<T>
    {
        private const string _protocol = "tcp";
        private string _port;
        private string _ip;
        private int _timeout;

        protected readonly JObject request;

        public NetMqClient(string ip, string port, int timeout)
        {
            _ip = ip;
            _port = port;
            _timeout = timeout;
            request = new JObject();
        }

        public string GetServerAddress()
        {
            return string.Format("{0}://{1}:{2}", _protocol, _ip, _port);
        }

        public int GetTimeout()
        {
            return _timeout;
        }

        public abstract T Connect();
    }

    public class NetMqClientJson : NetMqClient<JObject>
    {

        public NetMqClientJson(string ip = "127.0.0.1", string port = "5556", int timeout = 2000) : base(ip, port, timeout) { }

        public override JObject Connect()
        {
            AsyncIO.ForceDotNet.Force();

            var jsonResponse = new JObject();

            // Send request to PDSim Backend Server
            using (var socket = new RequestSocket())
            {
                // Create socket connection
                socket.Connect(GetServerAddress());
                // Send request
                socket.SendFrame(request.ToString());
                // Receive response
                var received = false;

                // Wait for response
                while (!received)
                {
                    if (socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(2000), out var response))
                    {
                        //Debug.Log("Received response: " + response);
                        jsonResponse = JObject.Parse(response);
                        received = true;
                    }
                    else
                    {
                        Debug.LogWarning("No response received");
                        break;
                    }
                }

                if (!received)
                {
                    jsonResponse.Add("status", "TO");
                }

            }

            NetMQConfig.Cleanup();
            return jsonResponse;
        }
    }


    public class NetMqClientBytes : NetMqClient<byte[]>
    {
        public NetMqClientBytes(string ip = "127.0.0.1", string port = "5556", int timeout = 2000) : base(ip, port, timeout) { }

        public override byte[] Connect()
        {
            AsyncIO.ForceDotNet.Force();

            byte[] byteResponse = null;

            // Send request to PDSim Backend Server
            using (var socket = new RequestSocket())
            {
                // Create socket connection
                socket.Connect(GetServerAddress());
                // Send request
                socket.SendFrame(request.ToString());
                // Receive response
                var received = false;

                // Wait for response
                while (!received)
                {
                    if (socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(2000), out var response))
                    {
                        //Debug.Log("Received response: " + response);
                        byteResponse = response;
                        received = true;
                    }
                    else
                    {
                        Debug.LogWarning("No response received");
                        break;
                    }
                }


            }

            NetMQConfig.Cleanup();
            return byteResponse;
        }
    }
}



