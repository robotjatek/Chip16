namespace EmuCore
{
    public static class Opcodes
    {
        // 0x - Misc/Video/Audio
        public static byte NOP = 0x00;
        public static byte CLS = 0x01;
        public static byte VBLNK = 0x02;
        public static byte BGC = 0x03;
        public static byte SPR = 0x04;
        public static byte DRW1 = 0x05;
        public static byte DRW2 = 0x06;
        public static byte RND = 0x07;
        public static byte FLIP = 0x08;
        public static byte SND0 = 0x09;
        public static byte SND1 = 0x0A;
        public static byte SND2 = 0x0B;
        public static byte SND3 = 0x0C;
        public static byte SNP = 0x0D;
        public static byte SNG = 0x0E;

        // 1x - Jumps(Branches)
        public static byte JMP = 0x10;
        public static byte J = 0x12;
        public static byte CALL = 0x14;

        // 2x - Loads
        public static byte LDI = 0x20;
        public static byte LDM = 0x22;
        public static byte MOV = 0x24;

        // 3x - Stores
        public static byte STM = 0x30;

        // 4x - Addidion
        public static byte ADDI = 0x40;

        // 6x - Bitwise AND
        public static byte TSTI = 0x63;

        // 8x -Bitwise XOR
        public static byte XOR = 0x81;
    }
}
