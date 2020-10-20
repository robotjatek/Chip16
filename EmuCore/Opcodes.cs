namespace EmuCore
{
    public static class Opcodes
    {
        // 0x - Misc/Video/Audio
        public const byte NOP = 0x00;
        public const byte CLS = 0x01;
        public const byte VBLNK = 0x02;
        public const byte BGC = 0x03;
        public const byte SPR = 0x04;
        public const byte DRW1 = 0x05;
        public const byte DRW2 = 0x06;
        public const byte RND = 0x07;
        public const byte FLIP = 0x08;
        public const byte SND0 = 0x09;
        public const byte SND1 = 0x0A;
        public const byte SND2 = 0x0B;
        public const byte SND3 = 0x0C;
        public const byte SNP = 0x0D;
        public const byte SNG = 0x0E;

        // 1x - Jumps(Branches)
        public const byte JMP = 0x10;
        public const byte J = 0x12;
        public const byte CALL = 0x14;
        public const byte RET = 0x15;

        // 2x - Loads
        public const byte LDI = 0x20;
        public const byte LDM = 0x22;
        public const byte MOV = 0x24;

        // 3x - Stores
        public const byte STM = 0x30;

        // 4x - Addidion
        public const byte ADDI = 0x40;
        public const byte ADD = 0x41;

        // 5x - Subtraction
        public const byte SUB = 0x51;
        public const byte CMPI = 0x53;

        // 6x - Bitwise AND
        public const byte TSTI = 0x63;

        // 7x - Bitwise OR
        public const byte OR = 0x71;

        // 8x - Bitwise XOR
        public const byte XOR = 0x81;

        // 9x - Multiplication
        public const byte MUL2 = 0x92;

        // Ax - Division
        public const byte DIV2 = 0xA2;

        // Bx - Logical/Arithmetic Shifts
        public const byte SHL = 0xB0;
        public const byte SHR = 0xB1;

        // Cx - Push/Pop
        public const byte POP = 0xC1;
        public const byte PUSHF = 0xC4;
    }
}
