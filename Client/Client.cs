using System;
using LiteNetLib;
using LiteNetLib.Utils;

namespace Client
{
    public class Client
    {
        private readonly DateTime _start = new DateTime(1970, 1, 1);

        private readonly string _host;
        private readonly int _port;
        private readonly EventBasedNetListener _listener = new EventBasedNetListener();
        private readonly NetManager _netManager;

        private NetPeer _peer;
        private readonly byte[] _message;

        private bool _connected;
        private bool _connecting;
        private bool _disconnecting;

        public Client(int maxConnections, string connectKey, string host, int port)
        {
            _host = host;
            _port = port;
            _netManager = new NetManager(_listener, maxConnections, connectKey);
            _message = new byte[50000];
            for (int index = 0; index < _message.Length; index++)
            {
                _message[index] = Byte.MaxValue;
            }
        }

        public bool Start()
        {
            if (_netManager.Start())
            {
                _listener.PeerConnectedEvent += OnPeerConnected;
                _listener.PeerDisconnectedEvent += OnPeerDisconnected;
                _listener.NetworkReceiveEvent += OnNetworkReceive;
                _listener.NetworkErrorEvent += OnNetworkErrorEvent;

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

            _netManager.Stop();
        }
        
        private readonly Random _random = new Random();

        public void Update()
        {
            _netManager.PollEvents();

            if (!_connected && !_connecting)
            {
                _connecting = true;
                _netManager.Connect(_host, _port);
            }

            if (_connected && !_disconnecting && _random.NextDouble() > 0.5)
            {
                _disconnecting = true;
                _netManager.DisconnectPeer(_peer);
            }

            if (_connected)
            {
                _peer.Send(_message, SendOptions.ReliableOrdered);
                Log("Send");
            }
        }

        private void OnPeerConnected(NetPeer peer)
        {
            _peer = peer;
            _connected = true;
            _connecting = false;
            _disconnecting = false;
            Log("Connected");
        }

        private void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectinfo)
        {
            _connected = false;
            _connecting = false;
            _disconnecting = false;
            _peer = null;
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
