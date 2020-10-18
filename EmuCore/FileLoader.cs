using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Text;

namespace EmuCore
{
    public class FileLoader
    {
        private readonly byte[] HEADER_MAGIC = Encoding.ASCII.GetBytes(new[] { 'C', 'H', '1', '6' });

        private readonly IBus _bus;
        private readonly byte[] header = new byte[16];

        public byte SpecificationVersion { get; private set; }

        public int RomSize { get; private set; }

        public ushort StartAddress { get; private set; }

        public int CRC32 { get; private set; }

        public FileLoader(IBus bus)
        {
            _bus = bus;
        }

        public void LoadFile(string path)
        {
            var file = File.ReadAllBytes(path);
            ParseBytes(file);
        }

        private void ParseBytes(byte[] file)
        {
            var header = file.AsSpan().Slice(0, 16).ToArray();
            if (header.AsSpan().Slice(0, 4).SequenceEqual(HEADER_MAGIC))
            {
                ParseHeader(header);
                _bus.FillMemory(file.AsSpan().Slice(16).ToArray(), StartAddress); //TODO: set PC to start address
            }
            else
            {
                _bus.FillMemory(file);
            }
        }

        private void ParseHeader(byte[] header)
        {
            SpecificationVersion = header.Skip(5).First();
            RomSize = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan().Slice(6, 4));
            StartAddress = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan().Slice(0x0a, 2));
            CRC32 = BinaryPrimitives.ReadInt32LittleEndian(header.AsSpan().Slice(0x0c, 4));
        }
    }
}
