using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IN8
{
    class Program
    {
        static Emulator Emulator;

        static void Main(string[] args)
        {
            /*
             * Input to support
             * [*]  Run hex coded ML 
             * [ ]  Load binary file to memory and run
             * [ ]  Set memory size
             * [ ]  Binding hardware
             * [*]  Single step debugging mode
             * [*]  Break points
             */

            Console.SetWindowSize(120, 40);

            try
            {
                Emulator = new Emulator();
                Emulator.MEM[0] = 0xCA;
                var breakpoints = new List<ushort>();
                bool stepping = false;

                int argI = 0;

                while (true)
                {
                    if (argI >= args.Length) break;

                    if (args[argI] == "-X")
                    {
                        args.Skip(argI + 1).Take(args.Length - argI).Select(s => Convert.ToByte(s, 16)).ToArray().CopyTo(Emulator.MEM, 0);
                        argI = args.Length;
                    }
                    else if (args[argI] == "-B")
                    {
                        breakpoints.Add(Convert.ToUInt16(args[argI + 1], 16));
                        argI += 2;
                    }
                    else if (args[argI] == "-S")
                    {
                        stepping = true;
                        argI += 1;
                    }
                }

                while (Emulator.STATE_FLAGS == 0)
                {
                    Emulator.Step();

                    if (stepping || breakpoints.Contains(Emulator.IP))
                    {
                        stepping = true;
                        Dump("BREAK");
                        Console.Write("PRESS ANY KEY TO STEP");
                        Console.ReadKey();
                    }
                }
                 
                Dump("STOP");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception. " + e.Message);
                Console.ReadKey();
            }

        }

            private static void Dump(String Message)
            {
                //Console.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.SetCursorPosition(0, 0);
                Console.Write(new String(' ', 128));
                Console.SetCursorPosition(0, 0);
                Console.Write("IN8 EMULATOR : {0}", Message);

                Console.ForegroundColor = ConsoleColor.Blue;
                DrawBox(109, 0, 119, 12);
                DisplayRegister("SF", Emulator.STATE_FLAGS, 110, 1);
                DisplayRegister("A ", Emulator.A, 110, 2);
                DisplayRegister("B ", Emulator.B, 110, 3);
                DisplayRegister("C ", Emulator.C, 110, 4);
                DisplayRegister("D ", Emulator.D, 110, 5);
                DisplayRegister("E ", Emulator.E, 110, 6);
                DisplayRegister("H ", Emulator.H, 110, 7);
                DisplayRegister("L ", Emulator.L, 110, 8);
                DisplayRegister("SP", Emulator.SP, 110, 9);
                DisplayRegister("IP", Emulator.IP, 110, 10);
                DisplayRegister("O ", Emulator.O, 110, 11);

                Console.ForegroundColor = ConsoleColor.Blue;
                DrawBox(0, 1, (32 * 3) + 8, 19);
                Console.SetCursorPosition(1, 2);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("INSTRUCTION MEMORY");
                DisplayMemoryBlock((ushort)((Emulator.IP - (8 * 32)) & 0xFFE0), 32, 16, 2, 3);

                Console.ForegroundColor = ConsoleColor.Blue;
                DrawBox(0, 20, (32 * 3) + 8, 38);
                Console.SetCursorPosition(1, 21);
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("STACK MEMORY");
                DisplayMemoryBlock((ushort)((Emulator.SP - (8 * 32)) & 0xFFE0), 32, 16, 2, 22);

                Console.SetCursorPosition(0, 39);
            }

        private static void DisplayRegister(String Name, byte Value, int x, int y)
        {
            Console.SetCursorPosition(x,y);
            Console.Write("{0}: {1:X2}", Name, Value);
        }

        private static void DisplayRegister(String Name, ushort Value, int x, int y)
        {
            Console.SetCursorPosition(x, y);
            Console.Write("{0}: {1:X4}", Name, Value);
        }

        private static void DisplayMemoryBlock(ushort offset, int stride, int rows, int x, int y)
        {
            for (int i = 0; i < rows; ++i)
            {
                Console.SetCursorPosition(x, y + i);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.Write("{0:X4}: ", offset);
                var fg = ConsoleColor.DarkGreen;

                if ((offset / 32) % 2 == 1)
                {
                    Console.BackgroundColor = ConsoleColor.DarkBlue;
                    fg = ConsoleColor.Green;
                }
                else Console.BackgroundColor = ConsoleColor.Black;


                for (int j = 0; j < stride; ++j)
                {

                    if (offset == Emulator.IP) Console.ForegroundColor = ConsoleColor.Red;
                    else if (offset == Emulator.SP) Console.ForegroundColor = ConsoleColor.Blue;
                    else Console.ForegroundColor = fg;

                    if (offset < Emulator.MEMORY_SIZE)
                        Console.Write("{0:X2} ", Emulator.MEM[offset]);
                    else
                        Console.Write("-- ");

                    offset += 1;
                }
            }

            Console.BackgroundColor = ConsoleColor.Black;
        }

        private const char Horizontal = '\u2500';
        private const char Vertical = '\u2502';
        private const char UpperLeftCorner = '\u250c';
        private const char UpperRightCorner = '\u2510';
        private const char LowerLeftCorner = '\u2514';
        private const char LowerRightCorner = '\u2518';

        private static void DrawBox(int left, int top, int right, int bottom)
        {
            Console.SetCursorPosition(left, top);
            Console.Write(UpperLeftCorner);
            Console.SetCursorPosition(left, bottom);
            Console.Write(LowerLeftCorner);
            Console.SetCursorPosition(right, top);
            Console.Write(UpperRightCorner);
            Console.SetCursorPosition(right, bottom);
            Console.Write(LowerRightCorner);

            for (int i = left + 1; i < right; ++i)
            {
                Console.SetCursorPosition(i, top);
                Console.Write(Horizontal);
                Console.SetCursorPosition(i, bottom);
                Console.Write(Horizontal);
            }

            for (int i = top + 1; i < bottom; ++i)
            {
                Console.SetCursorPosition(left, i);
                Console.Write(Vertical);
                Console.SetCursorPosition(right, i);
                Console.Write(Vertical);
            }
        }
    }
}
