using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing;

namespace MiControl
{
    public class MiController
    {
        public IPEndPoint Endpoint { get; private set; }
        public bool Connected { get; private set; }
        public bool Open { get; set; }
        public byte BridgeId { get; private set; }
        public byte Zone { get; private set; }

        private readonly Socket _client;
        private static Random _randomizer = new Random();

        private const byte _filler = 0x00;

        private int _sequentialByte = 0x01;

        private byte[] _getPrefixBytes(byte id)
            => new byte[] { 0x80, _filler, _filler, _filler, 0x11, id, _filler, _filler, (byte)(++_sequentialByte & 0xFF), _filler };

        public MiController(string ip, byte zone = 1, int port = 5987)
        {
            Endpoint = new IPEndPoint(IPAddress.Parse(ip), port);
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Connected = false;
            Open = true;
            Zone = zone;
        }

        public bool Connect()
        {
            _client.Connect(Endpoint);

            if (_client.Connected)
            {
                _client.Send(MiCommands.Handshake);
                byte[] data = Receive();

                BridgeId = data[19];
            }

            Connected = _client.Connected;
            Task.Factory.StartNew(KeepAlive);
            return Connected;
        }

        public void Close()
        {
            Connected = false;
            Open = false;
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public byte[] Receive()
        {
            byte[] responseBuffer = new byte[128];
            int bytesAvailable = _client.Receive(responseBuffer);
            return responseBuffer;
        }

        public void SendRaw(byte[] rawCommand)
        {
            _client.Send(rawCommand);
        }

        public void SendCommand(byte[] command)
        {
            byte[] prefix = _getPrefixBytes(BridgeId);
            byte[] toSum = MiHelpers.Combine(command, new byte[] { Zone, _filler });
            byte[] cmd = MiHelpers.Combine(prefix, toSum, new byte[] { (byte)toSum.Sum(x => x) });

            SendRaw(cmd);
        }

        private async void KeepAlive()
        {
            byte[] keepAliveCommand = new byte[] { 0xD0, _filler, _filler, _filler, 0x02, BridgeId, _filler };

            while (Connected)
            {
                SendRaw(keepAliveCommand);
                await Task.Delay(5000);
            }

            if (Open)
                Connect(); // try to re-connect, but this might be hopeless
        }

        public void TurnOn()
            => SendCommand(MiCommands.TurnOn);

        public void TurnOff()
            => SendCommand(MiCommands.TurnOff);

        public void NightMode()
            => SendCommand(MiCommands.NightMode);

        public void SetBrightness(byte percentage)
            => SendCommand(MiCommands.SetBrightness(percentage));

        public void SetColor(Color color)
            => SendCommand(MiCommands.SetColor(color));
    }
}
