using System;
using System.Net;
using System.Linq;
using System.Drawing;
using System.Threading;
using System.Net.Sockets;
using System.Threading.Tasks;

using MiControl.Effects;

namespace MiControl
{
    public class MiController : IDisposable
    {
        private static Random _rand = new Random();

        public IPEndPoint Endpoint { get; private set; }

        public bool Connected { get; private set; }
        public bool Open { get; set; }

        public byte BridgeId { get; private set; }
        public byte Zone { get; private set; }

        private readonly Socket _client;

        private byte _sequentialByte = 0x01;
        private byte[] _lastUsedCommand;


        public MiController(string ip, int port = 5987, byte zone = 1)
            : this(new IPEndPoint(IPAddress.Parse(ip), port), zone)
        { }
        public MiController(IPEndPoint endpoint, byte zone = 1)
        {
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            Endpoint = endpoint;
            Zone = zone;
            Open = true;
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
            
            Task.Factory.StartNew(KeepAliveAsync);
            return Connected = _client.Connected;
        }
        
        public byte[] Receive(int bufferSize = 128)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesAvailable = _client.Receive(buffer);
            return buffer;
        }

        public void SendCommand(byte[] command, bool updateLastUsed = true)
        {
            byte[] prefix = GetCommandPrefix(BridgeId);
            byte[] toSum = MiHelpers.Combine(command, new byte[] { Zone, 0x00 });
            byte[] cmd = MiHelpers.Combine(prefix, toSum, new byte[] { (byte)toSum.Sum(x => x) });

            if (updateLastUsed)
                _lastUsedCommand = command;

            _client.Send(cmd);
        }

        private async Task KeepAliveAsync()
        {
            byte[] keepAliveCommand = new byte[] { 0xD0, 0x00, 0x00, 0x00, 0x02, BridgeId, 0x00 };

            while (Connected)
            {
                _client.Send(keepAliveCommand);
                await Task.Delay(5000);
            }

            if (Open)
                Connect(); // try to re-connect, but this might be hopeless
        }

        private byte[] GetCommandPrefix(byte id)
            => new byte[] { 0x80, 0x00, 0x00, 0x00, 0x11, id, 0x00, 0x00, (byte)(++_sequentialByte & 0xFF), 0x00 };
        
        public void ApplyEffect(IMiEffect effect)
        {
            effect.Execute(this);
        }

        public void RunSequence(MiEffectSequence sequence, CancellationToken cancellationToken = default)
        {
            Task.Run(async () =>
            {
                foreach (var effect in sequence)
                {
                    effect.Execute(this);
                    await Task.Delay(effect.Duration);
                }
            }, cancellationToken);
        }

        public void ApplyColor(Color color) => ApplyEffect(new MiApplyColor(color));
        public void SetBrightness(byte percentage) => ApplyEffect(new MiSetBrightness(percentage));
        public void ApplyPartyMode(MiPartyMode partyMode) => ApplyEffect(new MiApplyPartyMode(partyMode));

        public void Close()
        {
            Connected = false;
            Open = false;

            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Connected)
                {
                    try
                    {
                        _client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
                        _client.Shutdown(SocketShutdown.Both);
                    }
                    catch { }
                }
                _client.Close();
            }
        }
    }
}
