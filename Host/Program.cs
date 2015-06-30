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
        static Dictionary<String, Type> HardwareTypes = new Dictionary<string, Type>();

        static void DiscoverHardware(System.Reflection.Assembly Assembly)
        {
            foreach (var type in Assembly.GetTypes())
            {
                var attr = type.GetCustomAttributes(false).FirstOrDefault(a => a is HardwareAttribute) as HardwareAttribute;
                if (attr != null && !HardwareTypes.ContainsKey(attr.ID))
                        HardwareTypes.Add(attr.ID, type);
            }
        }

        static void Main(string[] args)
        {
            DiscoverHardware(System.Reflection.Assembly.GetExecutingAssembly());

            /*
             * Input to support
             * [*]  Run hex coded ML 
             * [*]  Load binary file to memory and run
             * [*]  Set memory size
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
                var hardware = new List<String>();

                foreach (var arg in args)
                {
                    if (arg[0] == 'X')
                        arg.Split(' ').Skip(1).Select(s => Convert.ToByte(s, 16)).ToArray().CopyTo(Emulator.MEM, 0);
                    else if (arg[0] == 'B')
                        breakpoints.Add(Convert.ToUInt16(arg.Substring(1), 16));
                    else if (arg[0] == 'S')
                        stepping = true;
                    else if (arg[0] == 'F')
                        System.IO.File.ReadAllBytes(arg.Substring(1)).CopyTo(Emulator.MEM, 0);
                    else if (arg[0] == 'M')
                        Emulator.MEMORY_SIZE = Convert.ToUInt16(arg.Substring(1), 16);
                    else if (arg[0] == 'A')
                    {
                        var assembly = System.Reflection.Assembly.LoadFile(arg.Substring(1));
                        DiscoverHardware(assembly);
                    }
                    else if (arg[0] == 'H')
                        hardware.Add(arg.Substring(1));
                }

                System.Threading.Barrier barrier = new System.Threading.Barrier(2);
                var screenThread = new System.Threading.Thread(() =>
                {
                    var screen = new IN8.VisualHardwareGrid(Emulator);

                    foreach (var item in hardware)
                    {
                        var hargs = item.Split(' ');
                        var x = Convert.ToInt32(hargs[1]);
                        var y = Convert.ToInt32(hargs[2]);

                        screen.AddHardware(HardwareTypes[hargs[0]], 
                            new Microsoft.Xna.Framework.Point(Convert.ToInt32(hargs[1]), Convert.ToInt32(hargs[2])),
                            hargs.Skip(3).Select(s => Convert.ToByte(s, 16)).ToArray());
                    }
                    
                    barrier.SignalAndWait();
                    screen.Run();
                });
                screenThread.SetApartmentState(System.Threading.ApartmentState.STA);
                screenThread.Start();

                barrier.SignalAndWait();



                while (Emulator.STATE_FLAGS == 0)
                {
                    Emulator.Step();

                    if (stepping || breakpoints.Contains(Emulator.IP))
                    {
                        stepping = true;
                        Dump("BREAK");
                        Console.Write("WAITING FOR COMMAND");

                    TOP:
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.S) continue;
                        else if (key.Key == ConsoleKey.C) { stepping = false; continue; }
                        else { Console.Beep(); goto TOP; }
                    }
                }
                 
                Dump("STOP");
                Console.Write("PRESS ANY KEY TO EXIT");
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
            Console.Write("IN8 EMULATOR : ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(Message);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  CLOCK: {0:X8}", Emulator.CLOCK);

            Console.SetCursorPosition(48, 0);
            Console.Write("[S] STEP [C] CONTINUE");

            Console.ForegroundColor = ConsoleColor.Blue;
            DrawBox(105, 1, 119, 9);
            Console.SetCursorPosition(107, 1);
            Console.Write("REGISTERS");
            DisplayRegister("SF ", Emulator.STATE_FLAGS, 106, 2);
            DisplayRegister("A ", Emulator.A, 106, 3);
            DisplayRegister("B ", Emulator.B, 113, 3);
            DisplayRegister("C ", Emulator.C, 106, 4);
            DisplayRegister("O ", Emulator.O, 113, 4);
            DisplayRegister("D ", Emulator.D, 106, 5);
            DisplayRegister("E ", Emulator.E, 113, 5);
            DisplayRegister("H ", Emulator.H, 106, 6);
            DisplayRegister("L ", Emulator.L, 113, 6);
            DisplayRegister("SP ", Emulator.SP, 106, 7);
            DisplayRegister("IP ", Emulator.IP, 106, 8);

            Console.ForegroundColor = ConsoleColor.Blue;
            DrawBox(0, 1, 104, 10);
            Console.SetCursorPosition(4, 1);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("INSTRUCTION MEMORY");
            DisplayMemoryBlock((ushort)((Emulator.IP - (4 * 32)) & 0xFFE0), 32, 8, 2, 2);

            Console.ForegroundColor = ConsoleColor.Blue;
            DrawBox(0, 11, 104, 20);
            Console.SetCursorPosition(4, 11);
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("STACK MEMORY");
            DisplayMemoryBlock((ushort)((Emulator.SP - (4 * 32)) & 0xFFE0), 32, 8, 2, 12);

            Console.ForegroundColor = ConsoleColor.Blue;
            DrawBox(105, 10, 119, 38);
            Console.SetCursorPosition(107, 10);
            Console.Write("DISASSEMBLY");

            //27 slots
            ushort startIP = (ushort)(Emulator.IP - 13);
            var nextBytesCounter = 0;
            for (int i = 0; i < 27; ++i)
            {
                var decoded = "";

                if (nextBytesCounter > 0)
                {
                    --nextBytesCounter;
                    decoded = String.Format("{0:X2}", Emulator.MEM[startIP]);
                }
                else
                    Disassembly.DecodeInstruction(Emulator.MEM[startIP], out decoded, out nextBytesCounter);

                if (startIP == Emulator.IP) Console.ForegroundColor = ConsoleColor.Red;
                else Console.ForegroundColor = ConsoleColor.Green;

                Console.SetCursorPosition(106, 11 + i);
                Console.Write("       ");
                Console.SetCursorPosition(106, 11 + i);
                Console.Write(decoded);

                ++startIP;
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            DrawBox(0, 21, 104, 38);
            Console.SetCursorPosition(4, 21);
            Console.Write("HARDWARE");

            int hrow = 22;
            for (int p = 0; p < 256; ++p)
            {
                if (Emulator.HARDWARE[p] != null)
                {
                    Console.SetCursorPosition(1, hrow);
                    Console.Write("{0:X2}: {1}", p, Emulator.HARDWARE[p].DebugString);
                    ++hrow;
                }
            }
            
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
