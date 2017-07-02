using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlusLibSharp.Scripting
{
    public enum BfInstruction : ushort
    {
        /// <summary>
        /// Pushes a <see cref="uint"/> value onto the top of the stack.
        /// </summary>
        PushUInt32 = 0,

        /// <summary>
        /// Pushes a <see cref="float"/> value onto the top of the stack.
        /// </summary>
        PushFloat = 1,

        /*
        /// <summary>
        /// [Needs verification] Pushes a 'variable' onto the top of the stack.
        /// </summary>
        PushVariable = 2,
        */

        /// <summary>
        /// [Verified] Pushes the result of the last native function call onto the top of the stack.
        /// </summary>
        PushResult = 4,

        /// <summary>
        /// Pops an integer value off the top of the stack.
        /// </summary>
        PopInt = 5,

        /// <summary>
        /// Pops a float value off the top of the stack.
        /// </summary>
        PopFloat = 6,

        /// <summary>
        /// [Verified] Indicates the beginning of a procedure.
        /// </summary>
        BeginProcedure = 7,

        /// <summary>
        /// [Verified] Calls a native function in the game.
        /// </summary>
        CallNative = 8,

        // Verified with PCSX2 debugger
        // Pops the return address after return
        /// <summary>
        /// Return from a procedure or branch by jumping to the return address on the bottom of the stack. If stack is empty, this does nothing.
        /// </summary>
        Return = 9,

        /// <summary>
        /// [Verified] Calls a procedure
        /// </summary>
        CallProcedure = 0xB,

        /// <summary>
        /// [Verified] Jump to a label.
        /// </summary>
        Jump = 0xD,

        // Verified with PCSX2 debugger
        /// <summary>
        /// Pops 1 value off the stack and adds it to the value currently on top of the stack.
        /// </summary>
        Add = 0xE,

        /// <summary>
        /// [Needs stack evaluation] Pops 2 values off the stack, subtracts the first with the second, and pushes the result to the stack.
        /// </summary>
        Subtract = 0xF,

        Multiply = 0x10,

        Division = 0x11,

        // ???
        Minus = 0x12,

        // is this bitwise not
        Not = 0x13,
        
        /// <summary>
        /// [Needs stack evaluation] Pops 2 values off the stack, checks if the first is equal to the second, pushing 1 (true) to the stack if they are equal.
        /// </summary>
        Equal = 0x14,
        
        NotEqualZero = 0x15,     

        /// <summary>
        /// [Needs stack evaluation] Pops 2 values off the stack, checks if the first is not equal to the second, pushing 1 (true) to the stack if they're not equal.
        /// </summary>
        NotEqual = 0x16,

        /// <summary>
        /// [Needs stack evaluation] Pops a value off the stack, and jumps if that value is equal to 0 (false).
        /// </summary>
        JumpIfFalse = 0x1C,

        /// <summary>
        /// [Verified] Pushes a <see cref="ushort"/> value onto the top of the stack.
        /// </summary>
        PushUInt16 = 0x1D,

        /// <summary>
        /// [Verified] Pushes the value of the local variable onto the top of the stack.
        /// </summary>
        PushVariable = 0x1E,

        /// <summary>
        /// [Needs stack evaluation] Pops the value off the stack and stores the value of it in the specified variable.
        /// </summary>
        SetVariable = 0x20,
    }
}
