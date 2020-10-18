using System;
using System.Buffers.Binary;

namespace EmuCore
{
    public class Bus : IBus
    {
        private const int MEMORY_SIZE_BYTES = 65536;
        private readonly byte[] _memory = new byte[MEMORY_SIZE_BYTES];
        private readonly ICPU _cpu;
        private readonly GPU _gpu;
        public int Cycles { get; private set; } = 0;

        public Bus(ICPU cpu)
        {
            _cpu = cpu;
            _gpu = new GPU(this);
        }

        public void ExecuteCycle()
        {
            _cpu.Step();
            Cycles++;
        }

        public void FillMemory(byte[] bytes, ushort startAddress = 0)
        {
            if (bytes.Length + startAddress > MEMORY_SIZE_BYTES)
            {
                throw new ArgumentException();
            }
            Array.Copy(bytes, 0, _memory, startAddress, bytes.Length);
        }

        public int Read(ushort address)
        {
            var value = BinaryPrimitives.ReadInt32LittleEndian(_memory.AsSpan().Slice(address, 4));
            return value;
        }

        public short Read16(ushort address)
        {
            var value = BinaryPrimitives.ReadInt16LittleEndian(_memory.AsSpan().Slice(address, 2));
            return value;
        }

        public void Write(ushort address, short value)
        {
            var bytes = BitConverter.GetBytes(value);
            Array.Copy(bytes, 0, _memory, address, bytes.Length);
        }
    }
}
