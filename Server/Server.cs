using System;
using System.Collections.Generic;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Server
{
    class Server
    {
        private readonly DateTime _start = new DateTime(1970, 1, 1);

        private readonly EventBasedNetListener _listener = new EventBasedNetListener();
        private readonly NetManager _netManager;

        private readonly ICollection<NetPeer> _peers = new HashSet<NetPeer>();
        private readonly byte[] _message;

        public Server(int maxConnections, string connectKey)
        {
            _netManager = new NetManager(_listener, maxConnections, connectKey);
            _message = new byte[50000];
            for (int index = 0; index < _message.Length; index++)
            {
                _message[index] = Byte.MaxValue;
            }
        }

        public bool Start(int port)
        {
            if (_netManager.Start(port))
            {
                _listener.PeerConnectedEvent += OnPeerConnected;
                _listener.PeerDisconnectedEvent += OnPeerDisconnected;
                _listener.NetworkReceiveEvent += OnNetworkReceive;
                _listener.NetworkErrorEvent += OnNetworkErrorEvent;

                Log("Started on port: " + port);

                return true;
            }

            return false;
        }

        public void Stop()
        {
            _listener.PeerConnectedEvent -= OnPeerConnected;
            _listener.PeerDisconnectedEvent -= OnPeerDisconnected;
            _listener.NetworkReceiveEvent -= OnNetworkReceive;
            _listener.NetworkErrorEvent -= OnNetworkErrorEvent;

            Log("Stopped");

            _netManager.Stop();
        }

        public void Update()
        {
            _netManager.PollEvents();

            if (_peers.Count > 0)
            {
                foreach (NetPeer netPeer in _peers)
                {
                    netPeer.Send(_message, SendOptions.ReliableOrdered);
                }

                Log("Send");
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _peers.Add(peer);
            Log("Connected");
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            _peers.Remove(peer);
            Log("Disconnected");
        }

        private void OnNetworkReceive(NetPeer peer, NetDataReader reader)
        {
            Log("Received, bytes: " + reader.Data.Length);
        }

        private void OnNetworkErrorEvent(NetEndPoint endPoint, int socketErrorCode)
        {
            Log("OnNetworkErrorEvent");
        }

        private long GetTime()
        {
            return (long)DateTime.UtcNow.Subtract(_start).TotalMilliseconds;
        }

        private void Log(string msg)
        {
            Console.WriteLine(GetTime() + ": " + msg);
        }
    }
}
