namespace EmuCore
{
    public interface ICPU
    {
        IBus Bus { get; set; }
        void Step();
    }
}
