
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
            Mock.Get(registers).Verify(r => r.SetCarryFlag(false));
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

        [Fact]
        public void TestADD()
        {
            var gp = new short[16];
            gp[2] = 10;
            gp[4] = 5;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            gp[2].Should().Be(15);
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetOverflowFlag(false));
            Mock.Get(registers).Verify(r => r.SetCarryFlag(false));
        }

        [Fact]
        public void TestADDSetsZeroFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = 0;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
            gp[2].Should().Be(0);
        }

        [Fact]
        public void TestADDSetsZeroFlag2()
        {
            var gp = new short[16];
            gp[2] = -1;
            gp[4] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
            gp[2].Should().Be(0);
        }

        [Fact]
        public void TestADDSetsNegativeFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = -1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
            gp[2].Should().Be(-1);
        }

        [Fact]
        public void TestADDSetsOverflowFlag()
        {
            var gp = new short[16];
            gp[2] = 0x7fff;
            gp[4] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(true));
            gp[2].Should().BeNegative();
        }

        [Fact]
        public void TestADDSetsCarryFlag()
        {
            var gp = new short[16];
            gp[2] = unchecked((short)0xffff);
            gp[4] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.ADD, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.ADD];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }

        [Fact]
        public void TestSUB()
        {
            var gp = new short[16];
            gp[2] = 8;
            gp[4] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            gp[2].Should().Be(6);
            Mock.Get(registers).Verify(r => r.SetCarryFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
            Mock.Get(registers).Verify(r => r.SetOverflowFlag(false));
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
        }

        [Fact]
        public void TestSUBSetsZeroFlag()
        {
            var gp = new short[16];
            gp[2] = 8;
            gp[4] = 8;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            gp[2].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestSUBSetsZeroFlag2()
        {
            var gp = new short[16];
            gp[2] = -8;
            gp[4] = -8;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            gp[2].Should().Be(0);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestSUBSetsNegativeFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = 8;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            gp[2].Should().BeNegative();
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestSUBSetsOverflowFlag()
        {
            var gp = new short[16];
            gp[2] = 0x1;
            gp[4] = 0x7fff;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(true));
        }

        [Fact]
        public void TestSUBSetsCarryFlag()
        {
            var gp = new short[16];
            gp[2] = 0;
            gp[4] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.SUB, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.SUB];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }

        [Fact]
        public void TestCMPISetsZeroFlag()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0x02, 0x0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(true));
        }

        [Fact]
        public void TestCMPISetsZeroFlagToFalse()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0x00, 0x0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
        }

        [Fact]
        public void TestCMPISetsNegativeFlagToTrue()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0x04, 0x0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(true));
        }

        [Fact]
        public void TestCMPISetsNegativeFlagToFalse()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0x00, 0x0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Fact]
        public void TestCMPISetsNegativeFlagToFalseWhenTheResultIsPositive()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0x01, 0x0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Fact]
        public void TestCMPISetsOverflowFlagToTrue()
        {
            var gp = new short[16];
            gp[2] = 0x1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0xff, 0x7f });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(true));
        }

        [Fact]
        public void TestCMPISetsOverflowFlagToFalse()
        {
            var gp = new short[16];
            gp[2] = 0x1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0, 0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetOverflowFlag(false));
        }

        [Fact]
        public void TestCMPISetsCarryFlagToTrue()
        {
            var gp = new short[16];
            gp[2] = 0x0;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0, 1 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(true));
        }

        [Fact]
        public void TestCMPISetsCarryFlagToFalse()
        {
            var gp = new short[16];
            gp[2] = 0x1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.CMPI];
            var instruction = new Instruction(Opcodes.CMPI, new byte[] { 0x2, 0, 0 });
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetCarryFlag(false));
        }

        [Fact]
        public void TestPUSHF()
        {
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).SetupGet(r => r.SP).Returns(0xabcd);
            Mock.Get(registers).SetupGet(r => r.FLAGS).Returns(0b10101010);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.PUSHF];
            var instruction = new Instruction(Opcodes.PUSHF, new byte[] { 0, 0, 0 });
            operation(instruction, registers, bus);

            Mock.Get(bus).Verify(b => b.Write(0xabcd, 0b10101010));
            Mock.Get(registers).Verify(r => r.IncrementSP());

        }

        [Fact]
        public void TestPOP()
        {
            var gp = new short[16];
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            Mock.Get(registers).SetupGet(r => r.SP).Returns(0xabcd);

            var bus = Mock.Of<IBus>();
            Mock.Get(bus).Setup(b => b.Read16(It.IsAny<ushort>())).Returns(unchecked((short)0xdead));

            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.POP];
            var instruction = new Instruction(Opcodes.POP, new byte[] { 0x02, 0, 0 });
            operation(instruction, registers, bus);

            //SP--
            Mock.Get(registers).Verify(r => r.DecrementSP());
            //RX = Read16(SP)
            Mock.Get(bus).Verify(b => b.Read16(0xabcd));
            gp[2].Should().Be(unchecked((short)0xdead));
        }

        [Fact]
        public void TestSHL()
        {
            var gp = new short[16];
            gp[2] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL];
            var instruction = new Instruction(Opcodes.SHL, new byte[] { 0x2, 1, 0 });

            operation(instruction, registers, bus);

            gp[2].Should().Be(4);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Theory]
        [InlineData(2, 15, true)]
        [InlineData(2, 0, false)]
        public void TestSHLSetsTheZeroFlag(short operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL];
            var instruction = new Instruction(Opcodes.SHL, new byte[] { 0x2, operand2, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(flag));
        }

        [Theory]
        [InlineData(0x0, 0, false)]
        [InlineData(0x0, 5, false)]
        [InlineData(-1, 0, true)]
        [InlineData(0x1, 15, true)]
        [InlineData(0b0100_0000_0000_0000, 1, true)]
        public void TestSHLSetsTheNegativeFlag(short operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL];
            var instruction = new Instruction(Opcodes.SHL, new byte[] { 0x2, operand2, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(flag));
        }

        [Fact]
        public void TestSHR()
        {
            var gp = new short[16];
            gp[2] = 4;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHR];
            var instruction = new Instruction(Opcodes.SHR, new byte[] { 0x2, 1, 0 });

            operation(instruction, registers, bus);

            gp[2].Should().Be(2);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Theory]
        [InlineData(2, 3, true)]
        [InlineData(2, 0, false)]
        public void TestSHRSetsTheZeroFlag(short operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHR];
            var instruction = new Instruction(Opcodes.SHR, new byte[] { 0x2, operand2, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(flag));
        }

        [Theory]
        [InlineData(0x0, 0, false)]
        [InlineData(0x0, 5, false)]
        [InlineData(-1, 0, true)]
        [InlineData(0b0100_0000_0000_0000, 1, false)]
        [InlineData(0b1000_0000_0000_0000, 1, false)]
        public void TestSHRSetsTheNegativeFlag(int operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = (short)operand1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHR];
            var instruction = new Instruction(Opcodes.SHR, new byte[] { 0x2, operand2, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(flag));
        }

        [Fact]
        public void TestOR()
        {
            var gp = new short[16];
            gp[2] = 1;
            gp[4] = 2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.OR, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.OR];
            operation(instruction, registers, bus);

            gp[2].Should().Be(3);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }

        [Theory]
        [InlineData(0, 0, true)]
        [InlineData(1, 0, false)]
        [InlineData(0, 1, false)]
        public void TestORSetsZeroFlag(short operand1, short operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            gp[4] = operand2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.OR, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.OR];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(flag));
        }

        [Theory]
        [InlineData(0, 0, false)]
        [InlineData(0b1000_0000_0000_0000, 0, true)]
        [InlineData(0, 0b1000_0000_0000_0000, true)]
        public void TestORSetsNegativeFlag(int operand1, int operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = (short)operand1;
            gp[4] = (short)operand2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);
            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var instruction = new Instruction(Opcodes.OR, new byte[] { 0x42, 0, 0 });
            var operation = sut[Opcodes.OR];
            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(flag));
        }

        [Fact]
        public void TestSHL2()
        {
            var gp = new short[16];
            gp[2] = 2;
            gp[4] = 1;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL2];
            var instruction = new Instruction(Opcodes.SHL2, new byte[] { 0x42, 0, 0 });

            operation(instruction, registers, bus);

            gp[2].Should().Be(4);
            Mock.Get(registers).Verify(r => r.SetZeroFlag(false));
            Mock.Get(registers).Verify(r => r.SetNegativeFlag(false));
        }


        [Theory]
        [InlineData(2, 15, true)]
        [InlineData(2, 0, false)]
        public void TestSHL2SetsTheZeroFlag(short operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            gp[4] = operand2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL2];
            var instruction = new Instruction(Opcodes.SHL2, new byte[] { 0x42, 0, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetZeroFlag(flag));
        }

        [Theory]
        [InlineData(0x0, 0, false)]
        [InlineData(0x0, 5, false)]
        [InlineData(-1, 0, true)]
        [InlineData(0x1, 15, true)]
        [InlineData(0b0100_0000_0000_0000, 1, true)]
        public void TestSHL2SetsTheNegativeFlag(short operand1, byte operand2, bool flag)
        {
            var gp = new short[16];
            gp[2] = operand1;
            gp[4] = operand2;
            var registers = Mock.Of<IRegisters>();
            Mock.Get(registers).Setup(r => r.GP).Returns(gp);

            var bus = Mock.Of<IBus>();
            var sut = new InstructionsetFactory().CreateInstructionset();
            var operation = sut[Opcodes.SHL2];
            var instruction = new Instruction(Opcodes.SHL2, new byte[] { 0x42, 0, 0 });

            operation(instruction, registers, bus);

            Mock.Get(registers).Verify(r => r.SetNegativeFlag(flag));
        }
    }
}
