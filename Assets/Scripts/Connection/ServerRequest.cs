using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using PDSim.Utils;
using System;
using UnityEngine;

namespace PDSim.Connection
{
    public abstract class ServerRequest : UnityThread
    {
        private const string ServerURL = "tcp://127.0.0.1:5556";
        private readonly Action<JObject> _callback;

        protected readonly JObject request;

        public bool responseAvailable = false;
        protected ServerRequest(Action<JObject> callback)
        {
            request = new JObject();
            _callback = callback;
        }

        private void SendTimedOutResponse()
        {
            Debug.Log("ServerRequest: SendTimedOutResponse");
            var jsonResponse = new JObject();
            jsonResponse.Add("status", "TO");
            SendResponse(jsonResponse);
        }

        private void SendResponse(JObject response)
        {
            _callback(response);
        }

        protected override void Run()
        {
            AsyncIO.ForceDotNet.Force();

            // Send request to PDSim Backend Server
            using (var socket = new RequestSocket())
            {
                // Create socket connection
                socket.Connect(ServerURL);
                //Thread Blocking call to server
                Debug.Log("Sending request: " + request.ToString());
                socket.SendFrame(request.ToString());

                bool received = false;
                JObject jsonResponse = new JObject();


                // Wait for response
                while (!received)
                {
                    if (socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(2000), out string response))
                    {
                        Debug.Log("Received response: " + response);
                        jsonResponse = JObject.Parse(response);
                        received = true;
                    }
                    else
                    {
                        Debug.Log("No response received");
                        break;
                    }
                }

                if (received)
                {
                    SendResponse(jsonResponse);
                }
                else
                {
                    SendTimedOutResponse();
                }

            }

            responseAvailable = true;
            NetMQConfig.Cleanup();

        }
    }
    //public abstract class ServerRequest : UnityThread
    //{
    //    private const string ServerURL = "tcp://127.0.0.1:5556";
    //    private readonly Action<JObject> _callback;

    //    protected readonly JObject request;

    //    public bool responseAvailable = false;
    //    protected ServerRequest(Action<JObject> callback)
    //    {
    //        request = new JObject();
    //        _callback = callback;
    //    }

    //    private void SendTimedOutResponse()
    //    {
    //        Debug.Log("ServerRequest: SendTimedOutResponse");
    //        var jsonResponse = new JObject();
    //        jsonResponse.Add("status", "TO");
    //        SendResponse(jsonResponse);
    //    }

    //    private void SendResponse(JObject response)
    //    {
    //        _callback(response);
    //    }

    //    protected override void Run()
    //    {
    //        AsyncIO.ForceDotNet.Force();

    //        // Send request to PDSim Backend Server
    //        using (var socket = new RequestSocket())
    //        {
    //            // Create socket connection
    //            socket.Connect(ServerURL);
    //            //Thread Blocking call to server
    //            Debug.Log("Sending request: " + request.ToString());
    //            socket.SendFrame(request.ToString());

    //            bool received = false;
    //            JObject jsonResponse = new JObject();


    //            // Wait for response
    //            while (!received)
    //            {
    //                if (socket.TryReceiveFrameString(TimeSpan.FromMilliseconds(2000), out string response))
    //                {
    //                    Debug.Log("Received response: " + response);
    //                    jsonResponse = JObject.Parse(response);
    //                    received = true;
    //                }
    //                else
    //                {
    //                    Debug.Log("No response received");
    //                    break;
    //                }
    //            }

    //            if (received)
    //            {
    //                SendResponse(jsonResponse);
    //            }
    //            else
    //            {
    //                SendTimedOutResponse();
    //            }

    //        }

    //        responseAvailable = true;
    //        NetMQConfig.Cleanup();

    //    }
    //}
}



