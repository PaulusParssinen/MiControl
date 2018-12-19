using System.Collections.Generic;
using System.Drawing;

namespace MiControl.Effects
{
    public class MiEffect
    {
        public List<IMiEffectBase> EffectParts { get; private set; }

        public MiEffectEnd EndType { get; private set; }
        public int IterationCount { get; private set; }

        public MiEffect()
        {
            EffectParts = new List<IMiEffectBase>();
            EndType = MiEffectEnd.Once;
        }

        public MiEffect SetColor(Color color, int duration = 500)
        {
            EffectParts.Add(new MiColorEffect(color, duration));
            return this;
        }

        public MiEffect SetBrightness(byte percentage, int duration = 500)
        {
            EffectParts.Add(new MiBrightnessEffect(percentage, duration));
            return this;
        }

        public MiEffect UseLastCommand(int duration = 500)
        {
            EffectParts.Add(new MiLastCommandEffect(duration));
            return this;
        }

        public MiEffect End(MiEffectEnd endType, int iterations = 0)
        {
            EndType = endType;
            IterationCount = iterations;
            return this;
        }
    }
}
