using System;
using System.Drawing;

namespace EmuCore
{
    public class GPU : IGPU
    {
        byte BG { get; set; } //Nibble!

        public byte SpriteWidth { get; set; }

        public byte SpriteHeight { get; set; }

        bool HorizontalFlip { get; set; }

        bool VerticalFlip { get; set; }

        private readonly Color[] palette = new[]
        {
            Color.FromArgb(0,0,0), //Transparent in foreground
            Color.FromArgb(0,0,0),
            Color.FromArgb(0x88,0x88,0x88),
            Color.FromArgb(0xBF,0x39,0x32),
            Color.FromArgb(0xDE,0x7A,0xAE),
            Color.FromArgb(0x4C,0x3D,0x21),
            Color.FromArgb(0x90,0x5F,0x25),
            Color.FromArgb(0xE4,0x94,0x52),
            Color.FromArgb(0xEA,0xD9,0x79),
            Color.FromArgb(0x53,0x7A,0x3B),
            Color.FromArgb(0xAB,0xD5,0x4A),
            Color.FromArgb(0x25,0x2E,0x38),
            Color.FromArgb(0x00,0x46,0x7F),
            Color.FromArgb(0x68,0xAB,0xCC),
            Color.FromArgb(0xBC,0xDE,0xE4),
            Color.FromArgb(0xFF,0xFF,0xFF),
        };

        public IBus Bus { get; set; }

        public void AcceptCommand(GPUCommands command, byte[] parameters)
        {
            switch (command)
            {
                case GPUCommands.SPR:
                    SPR(parameters);
                    break;
                case GPUCommands.NOP:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void SPR(byte[] parameters)
        {
            var width = parameters[1];
            var height = parameters[2];
            SpriteWidth = width;
            SpriteHeight = height;
        }
    }
}
