namespace MiControl.Effects
{
    public class MiBrightnessEffect : IMiEffectBase
    {
        public int Duration { get; set; }
        public byte Percentage { get; private set; }

        public MiBrightnessEffect(byte percentage, int duration)
        {
            Percentage = percentage;
            Duration = duration;
        }
    }
}
