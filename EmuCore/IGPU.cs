namespace EmuCore
{
    public interface IGPU
    {
        IBus Bus { get; set; }

        void AcceptCommand(GPUCommands command, byte[] parameters);

    }
}