namespace MiControl.Effects
{
    public interface IMiEffect
    {
        int Duration { get; set; }
        
        void Execute(MiController controller);
    }
}