﻿using System;
using System.ComponentModel;
using VMData = AdventOfCode.Intcode.IntcodeVM.VMData;
using VMStates = AdventOfCode.Intcode.IntcodeVM.VMStates;

namespace AdventOfCode.Intcode
{
    /// <summary>
    /// Intcode Instructions
    /// </summary>
    public static class Instructions
    {
        /// <summary>
        /// Intcode Opcodes
        /// </summary>
        public enum Opcodes
        {
            ADD = 1,    //Add
            MUL = 2,    //Multiply
            INP = 3,    //Input
            OUT = 4,    //Output
            JNZ = 5,    //Jump Not Zero
            JEZ = 6,    //Jump Equal Zero
            TLT = 7,    //Test Less Than
            TEQ = 8,    //Test Equals
            REL = 9,    //Relative Base Set
            
            NOP = 0,    //No Op
            HLT = 99    //Halt
        }
        
        /// <summary>
        /// Parameter modes
        /// </summary>
        public enum ParamModes
        {
            POSITION  = 0,
            IMMEDIATE = 1,
            RELATIVE  = 2
        }

        /// <summary>
        /// Operand Modes
        /// </summary>
        public readonly struct Modes
        {
            #region Fields
            /// <summary> First parameter mode </summary>
            public readonly ParamModes first;
            /// <summary> Second parameter mode </summary>
            public readonly ParamModes second;
            /// <summary> Third parameter mode </summary>
            public readonly ParamModes third;
            #endregion

            #region Constructors
            /// <summary>
            /// Creates a new set of Modes from the given input string
            /// </summary>
            /// <param name="modes">Value to parse the modes from</param>
            /// <exception cref="ArgumentException">If the input string has the inappropriate length</exception>
            /// <exception cref="InvalidEnumArgumentException">If one of the parsed modes is not a valid member of the enum</exception>
            public Modes(string modes)
            {
                if (modes.Length is not 3) throw new ArgumentException($"Modes length is invalid, got {modes.Length}, expected 3", nameof(modes));

                this.first  = (ParamModes)(modes[2] - '0');
                this.second = (ParamModes)(modes[1] - '0');
                this.third  = (ParamModes)(modes[0] - '0');
            }
            #endregion
        }

        /// <summary>
        /// Intcode operation delegate
        /// </summary>
        /// <param name="pointer">Pointer of the VM</param>
        /// <param name="relative">Relative base of the VM</param>
        /// <param name="data">Intcode VM data</param>
        /// <param name="modes">Operand modes</param>
        public delegate VMStates Instruction(ref int pointer, ref int relative, in VMData data, in Modes modes);
        
        #region Constants
        /// <summary>
        /// True Constant
        /// </summary>
        private const long TRUE = 1L;
        /// <summary>
        /// False Constant
        /// </summary>
        private const long FALSE = 0L;
        #endregion

        #region Static methods
        /// <summary>
        /// Decodes an opcode into it's associated instruction
        /// </summary>
        /// <param name="opcode">Opcode to decode</param>
        /// <returns>A Tuple containing the Instruction this Opcode refers to and it's parameter modes</returns>
        /// <exception cref="ArgumentException">If the Modes input string is of invalid length</exception>
        /// <exception cref="InvalidEnumArgumentException">If an invalid Opcodes or ParamModes is detected</exception>
        public static (Instruction instruction, Modes modes) Decode(long opcode)
        {
            string padded = opcode.ToString("D5");
            Modes modes = new(padded[..3]);
            Opcodes op = (Opcodes)int.Parse(padded[3..]);
            Instruction instruction = op switch
            {
                //Instructions
                Opcodes.ADD => Add,
                Opcodes.MUL => Mul,
                Opcodes.INP => Inp,
                Opcodes.OUT => Out,
                Opcodes.JNZ => Jnz,
                Opcodes.JEZ => Jez,
                Opcodes.TLT => Tlt,
                Opcodes.TEQ => Teq,
                Opcodes.REL => Rel,
                
                //Nop, Halt, and unknown
                Opcodes.NOP => Nop,
                Opcodes.HLT => Hlt,
                _           => throw new InvalidEnumArgumentException(nameof(opcode), (int)op, typeof(Opcodes))
            };

            return (instruction, modes);
        }

        /// <summary>
        /// ADD Instruction, adds the values of the first and second operands into the address of the third operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Add(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            ref long c = ref GetOperand(pointer + 3, relative, data.memory, modes.third);
                
            c = a + b;
            pointer += 4;
            return VMStates.RUNNING;
        }

        /// <summary>
        /// MUL Instruction, multiplies the values of the first and second operands into the address of the third operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Mul(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            ref long c = ref GetOperand(pointer + 3, relative, data.memory, modes.third);
            
            c = a * b;
            pointer += 4;
            return VMStates.RUNNING;
        }

        /// <summary>
        /// INP Instruction, gets a value from the input stream and puts it at the address of the first operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Inp(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            //Make sure we can get the input first
            if (!data.getInput(out long input)) return VMStates.STALLED;
            
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            
            a = input;
            pointer += 2;
            return VMStates.RUNNING;
        }

        /// <summary>
        /// OUT Instruction, puts the value of the first operand in the output stream
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Out(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            
            data.setOutput(a);
            pointer += 2;
            return VMStates.RUNNING;
        }
        
        /// <summary>
        /// JNZ Instruction, if the first operand is not zero, sets the pointer to the value of the second operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Jnz(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            
            pointer = a is not FALSE ? (int)b : pointer + 3;
            return VMStates.RUNNING;
        }
        
        /// <summary>
        /// JEZ Instruction, if the first operand is zero, sets the pointer to the value of the second operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Jez(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            
            pointer = a is FALSE ? (int)b : pointer + 3;
            return VMStates.RUNNING;
        }
        
        /// <summary>
        /// TLT Instruction, if the first operand is less than the second operand, sets the third operand to 1, otherwise, sets the third operand to 0
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Tlt(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            ref long c = ref GetOperand(pointer + 3, relative, data.memory, modes.third);
            
            c = a < b ? TRUE : FALSE;
            pointer += 4;
            return VMStates.RUNNING;
        }
        
        /// <summary>
        /// TEQ Instruction, if the first operand is equal to the second operand, sets the third operand to 1, otherwise, sets the third operand to 0
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Teq(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);
            ref long b = ref GetOperand(pointer + 2, relative, data.memory, modes.second);
            ref long c = ref GetOperand(pointer + 3, relative, data.memory, modes.third);
            
            c = a == b ? TRUE : FALSE;
            pointer += 4;
            return VMStates.RUNNING;
        }
        
        /// <summary>
        /// REL Instruction, sets the relative base to the first operand
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Rel(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            ref long a = ref GetOperand(pointer + 1, relative, data.memory, modes.first);

            relative += (int)a;
            pointer += 2;
            return VMStates.RUNNING;
        }

        /// <summary>
        /// NOP Instruction, increments pointer only
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        private static VMStates Nop(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            pointer++;
            return VMStates.RUNNING;
        }

        /// <summary>
        /// HLT Instruction, sets the pointer into the halted state
        /// </summary>
        /// <param name="pointer">Current VM pointer</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="data">VM specific data</param>
        /// <param name="modes">Parameter modes</param>
        /// ReSharper disable once RedundantAssignment
        private static VMStates Hlt(ref int pointer, ref int relative, in VMData data, in Modes modes)
        {
            pointer = IntcodeVM.HALT;
            return VMStates.HALTED;
        }

        /// <summary>
        /// Returns the an operands in memory for the instruction at the given pointer
        /// </summary>
        /// <param name="pointer">Pointer address of the operand to get</param>
        /// <param name="relative">Current VM relative base</param>
        /// <param name="memory">Memory of the VM</param>
        /// <param name="mode">Parameter mode</param>
        /// <returns>The operand for the given instruction</returns>
        /// <exception cref="InvalidEnumArgumentException">If an invalid ParamModes is detected</exception>
        /// ReSharper disable once SuggestBaseTypeForParameter - Cannot be IList because of the ref return
        private static ref long GetOperand(int pointer, int relative, long[] memory, ParamModes mode)
        {
            //ReSharper disable once ConvertSwitchStatementToSwitchExpression - Cannot use a switch expression because of the ref return 
            switch (mode)
            {
                case ParamModes.POSITION:
                    return ref memory[memory[pointer]];
                case ParamModes.IMMEDIATE:
                    return ref memory[pointer];
                case ParamModes.RELATIVE:
                    return ref memory[memory[pointer] + relative];
                
                default:
                    throw new InvalidEnumArgumentException(nameof(mode), (int)mode, typeof(ParamModes));
            }
        }
        #endregion
    }
}
