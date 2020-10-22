
using EmuCore;

using FluentAssertions;

using Xunit;

namespace EmulatorTests
{
    public class GPUTests
    {
        [Fact]
        public void TestSPR()
        {
            var sut = new GPU();
            sut.AcceptCommand(GPUCommands.SPR, new byte[] { 0, 10, 20 });
            sut.SpriteWidth.Should().Be(10);
            sut.SpriteHeight.Should().Be(20);
        }
    }
}
