﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MusicBeePlugin.AndroidRemote.Settings;
using MusicBeePlugin.AndroidRemote.Utilities;
using MusicBeePlugin.Tools;
using ServiceStack.Text;

namespace MusicBeePlugin.AndroidRemote.Networking
{
    class ServiceDiscovery
    {
        private const int mPort = 45345;
        private static readonly IPAddress MulticastAddress = IPAddress.Parse("239.1.5.10");
        private UdpClient mListener;
        private readonly static ServiceDiscovery Disc = new ServiceDiscovery();

        public static ServiceDiscovery Instance
        {
            get { return Disc; }
        }

        private ServiceDiscovery()
        {
        }

        public void Start()
        {
            mListener = new UdpClient(mPort, AddressFamily.InterNetwork) {EnableBroadcast = true};
            mListener.JoinMulticastGroup(MulticastAddress);
            mListener.BeginReceive(OnDataReceived, null);
        }

        private void OnDataReceived(IAsyncResult ar)
        {
            IPEndPoint mEndPoint = new IPEndPoint(IPAddress.Any, mPort);
            byte[] request = mListener.EndReceive(ar, ref mEndPoint);
            string mRequest = Encoding.UTF8.GetString(request);
            JsonObject incoming = JsonObject.Parse(mRequest);

            if (incoming.Get("context").Contains("discovery"))
            {
                List<string> addresses = NetworkTools.GetPrivateAddressList();
                string clientAddress = incoming.Get("address");
                string interfaceAddress = String.Empty;
                int minDistance = 9001; // It's over nine thousand... 
                foreach (string address in addresses)
                {
                    int distance = LevenshteinDistance.CalculateDistances(clientAddress, interfaceAddress);
                    if (minDistance > distance)
                    {
                        minDistance = distance;
                        interfaceAddress = address;
                    }
                }

                Dictionary<string, object> notify = new Dictionary<string, object>();
                notify.Add("context","notify");
                notify.Add("address", interfaceAddress);
                notify.Add("computer", Environment.GetEnvironmentVariable("COMPUTERNAME"));
                notify.Add("port", UserSettings.Instance.ListeningPort);
                byte[] response = Encoding.UTF8.GetBytes(JsonSerializer.SerializeToString(notify));
                mListener.Send(response, response.Length, mEndPoint);                      
            }
            mListener.BeginReceive(OnDataReceived, null);
        }

    }
}