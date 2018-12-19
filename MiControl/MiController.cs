using System;
using System.Net;
using System.Linq;
using System.Drawing;
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
        private int _sequentialByte = 0x01;
        private byte[] _lastUsedCommand;

        public MiController(string ip, int port = 5987, byte zone = 1)
            : this(new IPEndPoint(IPAddress.Parse(ip), port))
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

        public void Close()
        {
            Connected = false;
            Open = false;
            _client.Shutdown(SocketShutdown.Both);
            _client.Close();
        }

        public byte[] Receive(int bufferSize = 128)
        {
            byte[] buffer = new byte[bufferSize];
            int bytesAvailable = _client.Receive(buffer);
            return buffer;
        }

        private void SendRaw(byte[] rawCommand)
        {
            _client.Send(rawCommand);
        }

        private void SendCommand(byte[] command, bool updateLastUsed = true)
        {
            byte[] prefix = GetCommandPrefix(BridgeId);
            byte[] toSum = MiHelpers.Combine(command, new byte[] { Zone, 0x00 });
            byte[] cmd = MiHelpers.Combine(prefix, toSum, new byte[] { (byte)toSum.Sum(x => x) });

            if (updateLastUsed)
                _lastUsedCommand = command;

            SendRaw(cmd);
        }

        private async Task KeepAliveAsync()
        {
            byte[] keepAliveCommand = new byte[] { 0xD0, 0x00, 0x00, 0x00, 0x02, BridgeId, 0x00 };

            while (Connected)
            {
                SendRaw(keepAliveCommand);
                await Task.Delay(5000);
            }

            if (Open)
                Connect(); // try to re-connect, but this might be hopeless
        }

        private byte[] GetCommandPrefix(byte id)
            => new byte[] { 0x80, 0x00, 0x00, 0x00, 0x11, id, 0x00, 0x00, (byte)(++_sequentialByte & 0xFF), 0x00 };

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

        public void SetWhite()
            => SendCommand(MiCommands.SetWhite);

        public void SetPartyMode(MiPartyMode mode)
            => SendCommand(MiCommands.SetPartyMode(mode));

        public void PlayEffect(MiEffect effect)
            => Task.Factory.StartNew(() => ExecuteEffect(effect));

        private async void ExecuteEffect(MiEffect effect)
        {
            int counter = effect.EndType == MiEffectEnd.Once ? 1 : effect.IterationCount;

            while (counter > 0 || effect.EndType == MiEffectEnd.Infinite)
            {
                foreach (IMiEffectBase effectPart in effect.EffectParts)
                {
                    switch (effectPart.GetType().Name)
                    {
                        case nameof(MiColorEffect):
                        {
                            MiColorEffect part = (MiColorEffect)effectPart;
                            SendCommand(MiCommands.SetColor(part.EffectColor), false);
                            break;
                        }
                        case nameof(MiBrightnessEffect):
                        {
                            MiBrightnessEffect part = (MiBrightnessEffect)effectPart;
                            SendCommand(MiCommands.SetBrightness(part.Percentage), false);
                            break;
                        }
                        case nameof(MiLastCommandEffect):
                        {
                            MiLastCommandEffect part = (MiLastCommandEffect)effectPart;

                            if (_lastUsedCommand != null)
                                SendCommand(_lastUsedCommand, false);
                            break;
                        }
                        //TODO: we definitely need other cases here at some point
                        // also possibly not hardcode the classnames like that
                        // also save last used command (other than effects) so that we can play effects and then go back to whatever the lights were doing previously
                    }

                    await Task.Delay(effectPart.Duration);
                }

                counter--;
            }
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
