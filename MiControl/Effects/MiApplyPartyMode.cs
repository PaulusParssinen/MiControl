namespace MiControl.Effects
{
    public class MiApplyPartyMode : IMiEffect
    {
        public int Duration { get; set; }
        public MiPartyMode PartyMode { get; set; }

        public MiApplyPartyMode(MiPartyMode partyMode, int duration = 0)
        {
            Duration = duration;
            PartyMode = partyMode;
        }

        public void Execute(MiController controller)
        {
            controller.SendCommand(MiCommands.SetPartyMode(PartyMode));
        }
    }
}
