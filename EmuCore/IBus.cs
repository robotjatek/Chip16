namespace EmuCore
{
    public interface IBus
    {
        void ExecuteCycle();

        int Read(ushort address);

        short Read16(ushort address);

        void FillMemory(byte[] bytes, ushort startAddress = 0);

        void Write(ushort address, short value);
    }
}
