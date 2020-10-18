namespace EmuCore
{
    public class Registers : IRegisters
    {
        class FlagPositions
        {
            public const byte CARRY = 0b00000010;
            public const byte ZERO = 0b00000100;
            public const byte OVERFLOW = 0b01000000;
            public const byte NEGATIVE = 0b10000000;
        }

        private const ushort START_ROM = 0;
        private const ushort START_STACK = 0xFDF0;
        private const ushort START_IO = 0xFFF0;

        public ushort PC { get; set; } = START_ROM;
        public ushort SP { get; set; } = START_STACK;
        public short[] GP { get; } = new short[16];

        /*
         * Bit 0 RESERVED
         * Bit 1 Carry
         * Bit 2 Zero
         * Bit 3 RESERVED
         * Bit 4 RESERVED
         * Bit 5 RESERVED
         * Bit 6 Overflow
         * Bit 7 Negative
         */
        public byte FLAGS { get; set; } = 0;

        public void IncrementPC()
        {
            PC += 4;
        }

        public void IncrementSP()
        {
            SP += 2;
        }

        public void SetCarryFlag(bool value)
        {
            if (value)
            {
                EnableFlag(FlagPositions.CARRY);
            }
            else
            {
                DisableFlag(FlagPositions.CARRY);
            }
        }

        public void SetNegativeFlag(bool value)
        {
            if (value)
            {
                EnableFlag(FlagPositions.NEGATIVE);
            }
            else
            {
                DisableFlag(FlagPositions.NEGATIVE);
            }
        }

        public void SetOverflowFlag(bool value)
        {
            if (value)
            {
                EnableFlag(FlagPositions.OVERFLOW);
            }
            else
            {
                DisableFlag(FlagPositions.OVERFLOW);
            }
        }

        public void SetZeroFlag(bool value)
        {
            if (value)
            {
                EnableFlag(FlagPositions.ZERO);
            }
            else
            {
                DisableFlag(FlagPositions.ZERO);
            }
        }

        private void EnableFlag(byte mask)
        {
            FLAGS |= mask;
        }

        private void DisableFlag(byte mask)
        {
            FLAGS &= unchecked((byte)~mask);
        }

        public void DecrementSP()
        {
            SP -= 2;
        }
    }
}
