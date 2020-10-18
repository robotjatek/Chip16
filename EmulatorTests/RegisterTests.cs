using EmuCore;

using FluentAssertions;

using Xunit;

namespace EmulatorTests
{
    public class RegisterTests
    {
        [Fact]
        public void RegistersInitializeCorrectly()
        {
            var sut = new Registers();

            sut.PC.Should().Be(0x00);
            sut.SP.Should().Be(0xFDF0);
            sut.GP.Length.Should().Be(16);
            sut.GP.Should().AllBeEquivalentTo(0);
            sut.FLAGS.Should().Be(0);
        }

        [Fact]
        public void IncrementsPCBy4()
        {
            var sut = new Registers();
            sut.IncrementPC();
            sut.PC.Should().Be(4);
        }

        [Fact]
        public void IncrementsSPBy2()
        {
            var sut = new Registers();
            sut.IncrementSP();
            sut.SP.Should().Be(0xFDF0 + 2);
        }

        [Fact]
        public void SetCarryFlag()
        {
            var sut = new Registers();
            sut.SetCarryFlag(true);
            sut.FLAGS.Should().Be(0b00000010);
        }

        [Fact]
        public void UnsetCarryFlag()
        {
            var sut = new Registers();
            sut.SetCarryFlag(false);
            sut.FLAGS.Should().Be(0b00000000);
        }

        [Fact]
        public void SetZeroFlag()
        {
            var sut = new Registers();
            sut.SetZeroFlag(true);
            sut.FLAGS.Should().Be(0b00000100);
        }

        [Fact]
        public void UnsetZeroFlag()
        {
            var sut = new Registers();
            sut.SetZeroFlag(false);
            sut.FLAGS.Should().Be(0b00000000);
        }

        [Fact]
        public void SetNegativeFlag()
        {
            var sut = new Registers();
            sut.SetNegativeFlag(true);
            sut.FLAGS.Should().Be(0b10000000);
        }

        [Fact]
        public void UnsetNegativeFlag()
        {
            var sut = new Registers();
            sut.SetNegativeFlag(false);
            sut.FLAGS.Should().Be(0b00000000);
        }

        [Fact]
        public void SetOveflowFlag()
        {
            var sut = new Registers();
            sut.SetOverflowFlag(true);
            sut.FLAGS.Should().Be(0b01000000);
        }

        [Fact]
        public void UnsetOveflowFlag()
        {
            var sut = new Registers();
            sut.SetOverflowFlag(false);
            sut.FLAGS.Should().Be(0b00000000);
        }
    }
}
