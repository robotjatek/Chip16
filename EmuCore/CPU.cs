using System;
using System.Collections.Generic;

namespace EmuCore
{
    public readonly struct Instruction
    {
        public Instruction(byte opcode, Span<byte> parameters)
        {
            if (parameters.Length != 3)
            {
                throw new ArgumentException(nameof(parameters));
            }

            Opcode = opcode;
            Parameters = parameters.ToArray();
        }

        public byte Opcode { get; }
        public byte[] Parameters { get; }
    }

    public class CPU : ICPU
    {
        private readonly Registers _registers = new Registers();
        private readonly Dictionary<byte, Action<Instruction, IRegisters, IBus>> _actions;

        public IBus Bus { get; set; }

        public CPU()
        {
            _actions = new InstructionsetFactory().CreateInstructionset(); //TODO: remove this hard dependency
        }

        public void Step()
        {
            var fullOpcode = Fetch();
            var instruction = Decode(fullOpcode);
            Execute(instruction);
        }

        private int Fetch()
        {
            var instruction = Bus.Read(_registers.PC);
            _registers.IncrementPC();
            return instruction;
        }

        private Action Decode(int fullOpcode)
        {
            var bytes = BitConverter.GetBytes(fullOpcode);
            var instruction = new Instruction(bytes[0], bytes.AsSpan().Slice(1));
            var operation = _actions[instruction.Opcode];

            return () => operation(instruction, _registers, Bus);
        }

        private void Execute(Action instruction)
        {
            instruction();
        }
    }
}
