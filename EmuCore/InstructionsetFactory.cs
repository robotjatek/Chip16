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
                { Opcodes.XOR, XOR },
                { Opcodes.ADDI, ADDI },
                { Opcodes.DIV2, DIV2 },
                { Opcodes.RET, RET },
                { Opcodes.MUL2, MUL2 },
                { Opcodes.ADD, ADD },
                { Opcodes.SUB, SUB },
                { Opcodes.CMPI, CMPI },
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

        private readonly Action<Instruction, IRegisters, IBus> ADDI = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var operand1 = registers.GP[x];
            var operand2 = BitConverter.ToInt16(instruction.Parameters, 1);
            var result = (short)(operand1 + operand2);
            registers.GP[x] = result;

            registers.SetZeroFlag(registers.GP[x] == 0);
            registers.SetNegativeFlag(registers.GP[x] < 0);
            var overflow = CheckOverflow(operand1, operand2, result);
            registers.SetOverflowFlag(overflow);

            var carry = (((ushort)operand1 + (ushort)operand2) & 0x10000) != 0;
            registers.SetCarryFlag(carry);
        };

        private readonly Action<Instruction, IRegisters, IBus> DIV2 = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var y = instruction.Parameters[0] >> 4;
            var z = instruction.Parameters[1] & 0b1111;

            var result = (short)(registers.GP[x] / registers.GP[y]);
            registers.GP[z] = result;
            registers.SetNegativeFlag(result < 0);
            registers.SetZeroFlag(result == 0);
            registers.SetCarryFlag(registers.GP[x] % registers.GP[y] != 0);
        };

        private readonly Action<Instruction, IRegisters, IBus> RET = (instruction, registers, bus) =>
        {
            registers.DecrementSP();
            registers.PC = (ushort)bus.Read16(registers.SP);
        };

        private readonly Action<Instruction, IRegisters, IBus> MUL2 = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var y = instruction.Parameters[0] >> 4;
            var z = instruction.Parameters[1] & 0b1111;

            var result = registers.GP[x] * registers.GP[y];
            registers.GP[z] = (short)result;

            registers.SetZeroFlag(result == 0);
            registers.SetNegativeFlag(result < 0);
            registers.SetCarryFlag(result > (result & 0xffff));
        };

        private readonly Action<Instruction, IRegisters, IBus> ADD = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var y = instruction.Parameters[0] >> 4;
            var operand1 = registers.GP[x];
            var operand2 = registers.GP[y];
            var result = (short)(operand1 + operand2);

            registers.GP[x] = result;
            registers.SetZeroFlag(registers.GP[x] == 0);
            registers.SetNegativeFlag(registers.GP[x] < 0);
            var overflow = CheckOverflow(operand1, operand2, result);
            registers.SetOverflowFlag(overflow);

            var carry = (((ushort)operand1 + (ushort)operand2) & 0x10000) != 0;
            registers.SetCarryFlag(carry);
        };

        private readonly Action<Instruction, IRegisters, IBus> SUB = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var y = instruction.Parameters[0] >> 4;
            var operand1 = registers.GP[x];
            var operand2 = registers.GP[y];
            var result = (short)(operand1 - operand2);

            registers.GP[x] = result;
            registers.SetZeroFlag(registers.GP[x] == 0);
            registers.SetNegativeFlag(registers.GP[x] < 0);

            var overflow = CheckOverflow(operand1, operand2, result);
            registers.SetOverflowFlag(overflow);

            var carry = CheckSubCarry(operand1, operand2);
            registers.SetCarryFlag(carry);
        };

        private readonly Action<Instruction, IRegisters, IBus> CMPI = (instruction, registers, bus) =>
        {
            var x = instruction.Parameters[0] & 0b1111;
            var operand1 = registers.GP[x];
            var operand2 = BitConverter.ToInt16(instruction.Parameters, 1);
            var result = (short)(operand1 - operand2);
            registers.SetZeroFlag(result == 0);
            registers.SetNegativeFlag(result < 0);

            var overflow = CheckOverflow(operand1, operand2, result);
            registers.SetOverflowFlag(overflow);

            var carry = CheckSubCarry(operand1, operand2);
            registers.SetCarryFlag(carry);
        };

        private static bool CheckOverflow(short operand1, short operand2, short result)
        {
            return (operand1 > 0 && operand2 > 0 && result < 0) || (operand1 < 0 && operand2 < 0 && result > 0);
        }

        private static bool CheckSubCarry(short operand1, short operand2)
        {
            return (((ushort)operand1 - (ushort)operand2) & 0x10000) != 0;
        }
    }
}
