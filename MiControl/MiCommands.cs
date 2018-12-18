using System.Drawing;

namespace MiControl
{
    public static class MiCommands
    {
        public static readonly byte[] Handshake = new byte[] {
                0x20, 0x00, 0x00, 0x00, 0x16, 0x02, 0x62,
                0x3a, 0xd5, 0xed, 0xa3, 0x01, 0xae, 0x08,
                0x2d, 0x46, 0x61, 0x41, 0xa7, 0xf6, 0xdc,
                0xaf, 0xfe, 0xf7, 0x00, 0x00, 0x1e };

        public static readonly byte[] TurnOn = new byte[] { 49, 0, 0, 7, 3, 1, 1, 1, 1 };
        public static readonly byte[] TurnOff = new byte[] { 49, 0, 0, 7, 3, 2, 2, 2, 2 };
        public static readonly byte[] NightMode = new byte[] { 49, 0, 0, 7, 3, 6, 6, 6, 6 };
        public static readonly byte[] SetWhite = new byte[] { 49, 0, 0, 7, 3, 5, 5, 5, 5 };

        public static byte[] SetBrightness(byte percentage)
        {
            percentage %= 100;
            return new byte[] { 49, 0, 0, 7, 2, percentage, percentage, percentage, percentage };
        }

        public static byte[] SetColor(Color color)
        {
            byte hue = MiHelpers.GetMilightHue(color);
            return new byte[] { 49, 0, 0, 7, 1, hue, hue, hue, hue };
        }

        public static byte[] SetPartyMode(MiMode mode)
        {
            return new byte[] { 49, 0, 0, 7, 4, (byte)mode, (byte)mode, (byte)mode, (byte)mode };
        }
    }
}
