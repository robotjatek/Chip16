
using EmuCore;

using FluentAssertions;

using Moq;

using Xunit;

namespace EmulatorTests
{
    public class BusTests
    {
        [Fact]
        public void ExecutesCpuCycle()
        {
            var cpu = Mock.Of<ICPU>();
            var sut = new Bus(cpu, Mock.Of<IGPU>());
            sut.ExecuteCycle();

            Mock.Get(cpu).Verify(c => c.Step(), Times.Once);
        }

        [Fact]
        public void ReadsAndWritesMemoryCorrectly()
        {
            unchecked
            {
                var sut = new Bus(Mock.Of<ICPU>(), Mock.Of<IGPU>());

                sut.Write(0x10, (short)0xabcd);
                var data = sut.Read16(0x10);
                data.Should().Be((short)0xabcd);
            }
        }

        [Fact]
        public void Reads4BytesCorrectly()
        {
            unchecked
            {
                var sut = new Bus(Mock.Of<ICPU>(), Mock.Of<IGPU>());
                sut.Write(0x10, (short)0xc0de);
                sut.Write(0x12, (short)0xdead);
                var data = sut.Read(0x10);
                data.Should().Be((int)0xdeadc0de);
            }
        }

        [Fact]
        public void FillsMemoryCorrectly()
        {
            unchecked
            {
                var sut = new Bus(Mock.Of<ICPU>(), Mock.Of<IGPU>());
                sut.FillMemory(new byte[] { 0xde, 0xc0, 0xad, 0xde });
                var data = sut.Read(0);
                data.Should().Be((int)0xdeadc0de);
            }
        }

        [Fact]
        public void FillsMemoryCorrectlyWithOffset()
        {
            unchecked
            {
                var sut = new Bus(Mock.Of<ICPU>(), Mock.Of<IGPU>());
                sut.FillMemory(new byte[] { 0xde, 0xc0, 0xad, 0xde }, 2);
                var empty = sut.Read16(0);
                empty.Should().Be(0);
                var data = sut.Read(2);
                data.Should().Be((int)0xdeadc0de);
            }
        }

        [Fact]
        public void SendsCommandToGPU()
        {
            var command = GPUCommands.NOP;
            var parameters = new byte[3];
            var gpu = Mock.Of<IGPU>();
            var sut = new Bus(Mock.Of<ICPU>(), gpu);
            sut.SendCommandToGPU(command, parameters);

            Mock.Get(gpu).Verify(g => g.AcceptCommand(GPUCommands.NOP, parameters));
        }
    }
}