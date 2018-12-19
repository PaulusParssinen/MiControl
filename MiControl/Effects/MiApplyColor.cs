using System.Drawing;

namespace MiControl.Effects
{
    public class MiApplyColor : IMiEffect
    {
        public int Duration { get; set; }
        public Color Color { get; set; }

        public MiApplyColor(Color color, int duration = 0)
        {
            Duration = duration;
            Color = color;
        }

        public void Execute(MiController controller)
        {
            controller.SendCommand(MiCommands.ApplyColor(Color));
        }
    }
}
