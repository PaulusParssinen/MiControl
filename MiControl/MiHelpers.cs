using System;
using System.Drawing;

namespace MiControl
{
    public static class MiHelpers
    {
        //Thanks Jon Skeet
        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        //Thanks Jon Skeet
        public static byte[] Combine(byte[] first, byte[] second, byte[] third)
        {
            byte[] ret = new byte[first.Length + second.Length + third.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            Buffer.BlockCopy(third, 0, ret, first.Length + second.Length,
                             third.Length);
            return ret;
        }

        //Thanks https://github.com/happyleavesaoc
        public static byte GetMilightHue(Color color)
        {
            float hue = color.GetHue() / 360;
            hue = (int)Math.Ceiling(hue * 255);
            hue = (176 - hue) % 256;
            hue = (255 - hue - 0x37) % 256;

            return (byte)(hue % 256);
        }
    }
}
