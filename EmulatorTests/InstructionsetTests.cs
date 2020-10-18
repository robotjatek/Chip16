
using EmuCore;

using FluentAssertions;

using Moq;

using Xunit;

namespace EmulatorTests
{
    public class InstructionsetTests
    {
        [Fact]
        public void TestJMP()
        {
            var sut = new InstructionsetFactory().CreateInstructionset();

            var instruction = new Instruction(Opcodes.JMP, new byte[] { 0x00, 0xcd, 0xab });
            var bus = new Mock<IBus>();
            var registers = new Mock<IRegisters>();

            var operation = sut[Opcodes.JMP];
            operation(instruction, registers.Object, bus.Object);

            registers.VerifySet(r => r.PC = 0xABCD);
        }

        [Fact]
        public void TestLDI()
        {
            var sut = new InstructionsetFactory().CreateInstructionset();

            var instruction = new Instruction(Opcodes.LDI, new byte[] { 0xa, 0xcd, 0xab });
            var bus = new Mock<IBus>();
            var registers = new Mock<IRegisters>();
            var gp = new short[16];
            registers.Setup(r => r.GP).Returns(gp);

            var operation = sut[Opcodes.LDI];
            operation(instruction, registers.Object, bus.Object);

            unchecked
            {
                gp[0xa].Should().Be((short)0xabcd);
            }
        }

        [Fact]
        public void TestSTM()
        {
            unchecked
            {
                var sut = new InstructionsetFactory().CreateInstructionset();
                var instruction = new Instruction(Opcodes.STM, new byte[] { 0x2, 0xcd, 0xab });
                var bus = new Mock<IBus>();
                var registers = new Mock<IRegisters>();
                var gp = new short[16];
                short value = (short)0xdead;
                gp[0x2] = value;
                registers.Setup(r => r.GP).Returns(gp);

                var operation = sut[Opcodes.STM];
                operation(instruction, registers.Object, bus.Object);
                bus.Verify(b => b.Write(0xabcd, (short)0xdead));
            }
        }

        [Fact]
        public void TestLDM()
        {
            unchecked
            {
                var sut = new InstructionsetFactory().CreateInstructionset();
                var instruction = new Instruction(Opcodes.LDM, new byte[] { 0x1, 0xcd, 0xab });
                var bus = new Mock<IBus>();
                bus.Setup(b => b.Read16(It.IsAny<ushort>())).Returns((short)0xdead);

                var gp = new short[16];
                var registers = new Mock<IRegisters>();
                registers.Setup(r => r.GP).Returns(gp);

                var operation = sut[Opcodes.LDM];
                operation(instruction, registers.Object, bus.Object);

                bus.Verify(b => b.Read16(0xabcd));
                gp[0x1].Should().Be((short)0xdead);
            }
        }

        [Fact]
        public void TestMOV()
        {
            unchecked
            {
                var sut = new InstructionsetFactory().CreateInstructionset();
                var instruction = new Instruction(Opcodes.MOV, new byte[] { 0xab, 0, 0 });
                var bus = new Mock<IBus>();
                var gp = new short[16];
                gp[0x0a] = (short)0xdead;
                var registers = new Mock<IRegisters>();
                registers.Setup(r => r.GP).Returns(gp);

                var operation = sut[Opcodes.MOV];
                operation(instruction, registers.Object, bus.Object);
                gp[0x0b].Should().Be((short)0xdead);
            }
        }

        [Fact]
        public void TestCall()
        {
            unchecked
            {
                var sut = new InstructionsetFactory().CreateInstructionset();
                var instruction = new Instruction(Opcodes.CALL, new byte[] { 0, 0xcd, 0xab });
                var bus = new Mock<IBus>();
                var registers = new Mock<IRegisters>();
                registers.Setup(r => r.PC).Returns(0xeeee);
                registers.Setup(r => r.SP).Returns(0xff00);

                var operation = sut[Opcodes.CALL];
                operation(instruction, registers.Object, bus.Object);

                //store pc to [sp]
                bus.Verify(b => b.Write(0xff00, (short)0xeeee));

                //inc sp by 2
                registers.Verify(r => r.IncrementSP());

                //set pc to 0xabcd
                registers.VerifySet(r => r.PC = 0xabcd);
            }
        }

        [Fact]
        public void TestNOP()
        {
            var registers = Mock.Of<IRegisters>();
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.NOP, new byte[] { 0, 0, 0 });
            var operation = sut[Opcodes.NOP];
            operation(instruction, registers, bus);
        }

        [Fact]
        public void TSTISetsZeroFlag()
        {
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(new short[16]);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.TSTI, new byte[] { 0x0a, 0x00, 0x00 });
            var operation = sut[Opcodes.TSTI];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TSTIUnsetsZeroFlag()
        {
            var registers = Mock.Of<IRegisters>();
            var gp = new short[16];
            gp[0] = unchecked((short)0xffff);
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.TSTI, new byte[] { 0x00, 0xff, 0xff });
            var operation = sut[Opcodes.TSTI];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
        }

        [Fact]
        public void TSTISetsNegativeFlag()
        {
            var registers = Mock.Of<IRegisters>();
            var gp = new short[16];
            gp[0] = unchecked((short)0xffff);
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.TSTI, new byte[] { 0x00, 0x00, 0xf0 });
            var operation = sut[Opcodes.TSTI];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TSTIUnsetsNegativeFlag()
        {
            var registers = Mock.Of<IRegisters>();
            var gp = new short[16];
            gp[0] = unchecked((short)0xffff);
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.TSTI, new byte[] { 0x00, 0x00, 0x0f });
            var operation = sut[Opcodes.TSTI];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Fact]
        public void TestJPerformsJMPWhenParameterIsNotZero()
        {
            var registers = Mock.Of<IRegisters>();
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.J, new byte[] { 0x1, 0xcd, 0xab });
            var operation = sut[Opcodes.J];
            operation(instruction, registers, bus);

            Mock.Get(registers).VerifySet(r => r.PC = 0xabcd);
        }

        [Fact]
        public void TestJDoesNotPerformJMPWhenTheParameterIsZero()
        {
            var registers = Mock.Of<IRegisters>();
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.J, new byte[] { 0x0, 0xcd, 0xab });
            var operation = sut[Opcodes.J];
            operation(instruction, registers, bus);

            Mock.Get(registers).VerifySet(r => r.PC = 0xabcd, Times.Never);
        }

        [Fact]
        public void TestXOR()
        {
            var gp = new short[16];
            gp[0] = 0;
            gp[1] = unchecked((short)0xffff);
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.XOR, new byte[] { 0x10, 0, 0 });
            var operation = sut[Opcodes.XOR];
            operation(instruction, registers, bus);

            gp[0].Should().Be(unchecked((short)0xffff));
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestXORSetsZeroFlag()
        {
            var gp = new short[16];
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.XOR, new byte[] { 0x10, 0, 0 });
            var operation = sut[Opcodes.XOR];
            operation(instruction, registers, bus);

            gp[0].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Fact]
        public void TestADDI()
        {
            var gp = new short[16];
            gp[1] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 1, 0 });
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            gp[1].Should().Be(2);
        }

        [Fact]
        public void TestADDISetsZeroFlag()
        {
            var gp = new short[16];
            gp[1] = -1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 1, 0 });
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            gp[1].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestADDISetsNegtiveFlag()
        {
            var gp = new short[16];
            gp[1] = -1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 0xff, 0xff }); //0xffff == -1
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            gp[1].Should().Be(-2);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestADDISetsOverflowFlag()
        {
            var gp = new short[16];
            gp[1] = unchecked((short)0x7fff);
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 0xff, 0x7f });
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(true));
        }

        [Fact]
        public void TestADDISetsOverflowFlag2()
        {
            var gp = new short[16];
            gp[1] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 0xff, 0x7f });
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(true));
        }

        [Fact]
        public void TestADDISetsCarryFlag()
        {
            var gp = new short[16];
            gp[1] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instructrion = new Instruction(Opcodes.ADDI, new byte[] { 0x1, 0xff, 0xff });
            var operation = sut[Opcodes.ADDI];
            operation(instructrion, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }

        [Fact]
        public void TestDiv2()
        {
            var gp = new short[16];
            gp[2] = 10;
            gp[4] = 5;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.DIV2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.DIV2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(2);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetCarryFlag(false));
        }

        [Fact]
        public void TestDiv2SetsZeroFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = 5;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.DIV2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.DIV2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestDiv2SetsNegativeFlag()
        {
            var gp = new short[16];
            gp[2] = -10;
            gp[4] = 5;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.DIV2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.DIV2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(-2);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestDiv2SetsNegativeFlag2()
        {
            var gp = new short[16];
            gp[2] = 10;
            gp[4] = -5;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.DIV2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.DIV2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(-2);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestDiv2SetsCarryFlag()
        {
            var gp = new short[16];
            gp[2] = 3;
            gp[4] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.DIV2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.DIV2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(1);
            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }

        [Fact]
        public void TestRET()
        {
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.SP).Returns(0);
            var bus = Mock.Of<IBus>();
            Mock.Get(bus).Setup(b => b.Read16(0)).Returns(unchecked((short)0xabcd));
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.RET, new byte[] { 0, 0, 0 });
            var operation = sut[Opcodes.RET];
            operation(instruction, registers, bus);

            //Decrease SP by 2
            Mock.Get(registers).Verify(r => r.DecrementSP());
            //set PC to [SP]
            Mock.Get(registers).VerifySet(r => r.PC = 0xabcd);
        }

        [Fact]
        public void TestMUL2()
        {
            var gp = new short[16];
            gp[2] = 2;
            gp[4] = 3;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.MUL2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.MUL2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(6);
        }

        [Fact]
        public void TestMUL2SetsZeroFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = 3;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.MUL2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.MUL2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestMUL2SetsNegativeFlag()
        {
            var gp = new short[16];
            gp[2] = -2;
            gp[4] = 3;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.MUL2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.MUL2];
            operation(instruction, registers, bus);

            gp[8].Should().Be(-6);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestMUL2SetsCarryFlag()
        {
            var gp = new short[16];
            gp[2] = 0x2000;
            gp[4] = 0x0100;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.MUL2, new byte[] { 0x42, 0x08, 0 });
            var operation = sut[Opcodes.MUL2];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }
    }
}
