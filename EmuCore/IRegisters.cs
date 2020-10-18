namespace EmuCore
{
    public interface IRegisters
    {
        ushort PC { get; set; }
        ushort SP { get; set; }
        short[] GP { get; }
        byte FLAGS { get; set; }
        void IncrementPC();
        void IncrementSP();

        void SetCarryFlag(bool value);

        void SetZeroFlag(bool value);

        void SetOverflowFlag(bool value);

        void SetNegativeFlag(bool value);
    }
}
