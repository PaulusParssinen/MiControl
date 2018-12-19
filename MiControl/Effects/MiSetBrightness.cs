namespace MiControl.Effects
{
    public class MiSetBrightness : IMiEffect
    {
        public int Duration { get; set; }
        public byte Percentage { get; private set; }

        public MiSetBrightness(byte percentage, int duration = 0)
        {
            Duration = duration;
            Percentage = percentage;
        }

        public void Execute(MiController controller)
        {
            controller.SendCommand(MiCommands.SetBrightness(Percentage));;
        }
    }
}
