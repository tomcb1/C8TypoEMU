using System;

namespace C8TypoEmu
{
    static class Emulator
    {
        // http://devernay.free.fr/hacks/chip8/C8TECH10.HTM#2.2
        static public byte[] registers = new byte[0x10]; // 16, 8-Bit registers - V0 to VF (VF doubles as a flag for some instructions)
        static public short registerI; // Generally used to store memory addresses
        static public short programCounter = 0x200; // 16-Bit program counter. Starts at 0x200
        static public byte stackPointer; // Stack pointer. Self explanatory
        static public short[] stack = new short[16]; // Holds up to 16, 16-Bit memory addresses that the interpreter should return to after subroutines
        static public byte delayTimer = 0x00;
        static public byte soundTimer = 0x00;
        static public byte[] memory = new byte[4096];
        static public byte[] currentROM;
        static public readonly byte[,] font = new byte[16,5] // This is our default font.
        {
            {0xF0, 0x90, 0x90, 0x90, 0xF0}, // 0
            {0x20, 0x60, 0x20, 0x20, 0x70}, // 1
            {0xF0, 0x10, 0xF0, 0x80, 0xF0}, // 2
            {0xF0, 0x10, 0xF0, 0x10, 0xF0}, // 3
            {0x90, 0x90, 0xF0, 0x10, 0x10}, // 4
            {0xF0, 0x80, 0xF0, 0x10, 0xF0}, // 5
            {0xF0, 0x80, 0xF0, 0x90, 0xF0}, // 6
            {0xF0, 0x10, 0x20, 0x40, 0x40}, // 7
            {0xF0, 0x90, 0xF0, 0x90, 0xF0}, // 8
            {0xF0, 0x90, 0xF0, 0x10, 0xF0}, // 9
            {0xF0, 0x90, 0xF0, 0x90, 0x90}, // A
            {0xE0, 0x90, 0xE0, 0x90, 0xE0}, // B
            {0xF0, 0x80, 0x80, 0x80, 0xF0}, // C
            {0xE0, 0x90, 0x90, 0x90, 0xE0}, // D
            {0xF0, 0x80, 0xF0, 0x80, 0xF0}, // E
            {0xF0, 0x80, 0xF0, 0x80, 0x80}  // F
        };
        static public UInt32[] display = new UInt32[64 * 32];

        static public void IncrementTimers()
        {
            if (delayTimer != 0)
            {
                delayTimer--;
            }
            if (soundTimer != 0)
            {
                soundTimer--;
            }
        }

        static public void ExecuteNextOpCode()
        {
            Byte[] OpCode = new Byte[2];
            OpCode[0] = memory[programCounter];
            programCounter++;
            OpCode[1] = memory[programCounter];
            programCounter++;

            // Split the opcode into a bunch of pieces
            byte firstNibble = (byte)(OpCode[0] >> 4); // Highest 4 bits of the instruction
            byte lastNibble = (byte)((byte)OpCode[1] & (byte)0xF); // Lowest 4 bits of the instruction.
            short addr = (short)(((OpCode[0] & 0xF) << 0x8) | OpCode[1]); // Lowest 12 bits of the instruction. (C# does not have byte literals. Why!)
            byte x = (byte)(OpCode[0] & 0xF); // Lower 4 bits of the high byte of the instruction.
            byte y = (byte)(OpCode[1] >> 4); // Upper 4 bits of the low byte of the instruction.
            byte kk = OpCode[1]; // Lowest 8 bits of the instruction.

            switch (firstNibble)
            {
                case 0x0: 
                {// Handle 0---
                    switch(OpCode[1]) 
                    {// Handle 00E-
                        case 0xE0:
                        {// Handle 00E0 - CLS
                            for (int i = 0; i < 64 * 32; i++)
                            {
                                display[i] = 0xFF000000;
                            }
                            break;
                        }
                        case 0xEE:
                        {// Handle 00EE - RET
                            programCounter = stack[stackPointer];
                            if (stackPointer != 0)
                            {
                                stackPointer--;
                            }
                            break;
                        }
                        default:
                        {// Handle 0nnn - SYS addr (Ignored)
                            break;
                        }
                    }
                    break;
                }
                case 0x1: 
                {// Handle 1nnn - JP addr
                    programCounter = addr;
                    break;
                }
                case 0x2:
                {// Handle 2nnn - CALL addr
                    stack[stackPointer] = programCounter;
                    stackPointer++;
                    programCounter = addr;
                    break;
                }
                case 0x3:
                {// Handle 3xkk - SE Vx, byte
                    if (registers[x] == kk)
                    {
                        programCounter += 2;
                    }
                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }
}

