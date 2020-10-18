using System;
using System.Collections.Generic;

namespace EmuCore
{
    public class InstructionsetFactory
    {
        public Dictionary<byte, Action<Instruction, IRegisters, IBus>> CreateInstructionset()
        {
            return new Dictionary<byte, Action<Instruction, IRegisters, IBus>>()
            {
                { Opcodes.JMP, JMP },
                { Opcodes.LDI, LDI },
                { Opcodes.STM, STM },
                { Opcodes.LDM, LDM },
                { Opcodes.MOV, MOV },
                { Opcodes.CALL, CALL },
                { Opcodes.NOP, NOP },
                { Opcodes.TSTI, TSTI },
                { Opcodes.J, J },
                { Opcodes.XOR, XOR }
            };
        }

        private readonly Action<Instruction, IRegisters, IBus> NOP = (i, r, b) =>
        {
            //No operation
        };

        private readonly Action<Instruction, IRegisters, IBus> JMP = (instruction, registers, _) =>
        {
            var address = BitConverter.ToUInt16(instruction.Parameters, 1);
            registers.PC = address;
        };

        private readonly Action<Instruction, IRegisters, IBus> LDI = (i, r, b) =>
        {
            var register = i.Parameters[0];
            var value = BitConverter.ToInt16(i.Parameters, 1);
            r.GP[register] = value;
        };

        private readonly Action<Instruction, IRegisters, IBus> STM = (instruction, registers, bus) =>
        {
            var register = instruction.Parameters[0];
            var value = registers.GP[register];
            var address = BitConverter.ToUInt16(instruction.Parameters, 1);
            bus.Write(address, value);
        };

        private readonly Action<Instruction, IRegisters, IBus> LDM = (instruction, registers, bus) =>
        {
            var register = instruction.Parameters[0];
            var address = BitConverter.ToUInt16(instruction.Parameters, 1);
            var value = bus.Read16(address);
            registers.GP[register] = value;
        };

        private readonly Action<Instruction, IRegisters, IBus> MOV = (instruction, registers, bus) =>
        {
            var param = instruction.Parameters[0];
            var y = param >> 4;
            var x = param & 0b1111;

            registers.GP[x] = registers.GP[y];
        };

        private readonly Action<Instruction, IRegisters, IBus> CALL = (instruction, registers, bus) =>
        {
            bus.Write(registers.SP, (short)registers.PC);
            registers.IncrementSP();
            var value = BitConverter.ToUInt16(instruction.Parameters, 1);
            registers.PC = value;
        };

        private readonly Action<Instruction, IRegisters, IBus> TSTI = (instruction, registers, bus) =>
        {
            var registerIndex = instruction.Parameters[0];
            var immediateValue = BitConverter.ToInt16(instruction.Parameters, 1);
            var result = registers.GP[registerIndex] & immediateValue;
            registers.SetZeroFlag(result == 0);
            registers.SetNegativeFlag(result < 0);
        };

        private readonly Action<Instruction, IRegisters, IBus> J = (instruction, registers, bus) =>
        {
            if (instruction.Parameters[0] != 0)
            {
                var address = BitConverter.ToUInt16(instruction.Parameters, 1);
                registers.PC = address;
            }
        };

        private readonly Action<Instruction, IRegisters, IBus> XOR = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var y = instruction.Parameters[0] >> 4;

            registers.GP[x] = (short)(registers.GP[x] ^ registers.GP[y]);
            registers.SetZeroFlag(registers.GP[x] == 0);
            registers.SetNegativeFlag(registers.GP[x] < 0);
        };
    }
}
