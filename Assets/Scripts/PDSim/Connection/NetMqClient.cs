using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using PDSim.Utils;
using System;
using UnityEngine;

namespace PDSim.Connection
{
    public class NetMqClient
    {
        private const string Address = "tcp://127.0.0.1:5556";
        protected readonly JObject request;

        protected NetMqClient()
        {
            request = new JObject();
        }

        public JObject Connect()
        {
            AsyncIO.ForceDotNet.Force();

            var jsonResponse = new JObject();

            // Send request to PDSim Backend Server
            using (var socket = new RequestSocket())
            {
                // Create socket connection
                socket.Connect(Address);
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
}



