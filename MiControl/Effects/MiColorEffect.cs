using System.Drawing;

namespace MiControl.Effects
{
    public class MiColorEffect : IMiEffectBase
    {
        public int Duration { get; set; }
        public Color EffectColor { get; set; }

        public MiColorEffect(Color effectColor, int duration)
        {
            EffectColor = effectColor;
            Duration = duration;
        }
    }
}
