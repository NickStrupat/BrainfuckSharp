using System;
using System.Collections.Generic;
using System.IO;

namespace BrainfuckSharp {
    class Program {
        private static void Main(String[] args) {
            if (args.Length < 1)
                throw new ArgumentException(String.Format("Usage: {0} file_path [--cache]", AppDomain.CurrentDomain.FriendlyName));
            Stream instructionStream = new FileStream(args[0], FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.RandomAccess);
            if (args.Length >= 2 && args[1] == "--cache") {
                var memoryStream = new MemoryStream((Int32)instructionStream.Length);
                instructionStream.CopyTo(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                instructionStream = memoryStream;
            }

            const Int32 stackSize = 30000;
            var stack = new Byte[stackSize];
            const Int32 stackStartIndex = 0;
            const Int32 stackEndIndex = stackSize - 1;
            Int32 stackIndex = stackStartIndex;

            var jumps = new Stack<Int64>(); // instruction indexes
            Int32 byteRead;
            while ((byteRead = instructionStream.ReadByte()) != -1) {
                var instruction = (Char)byteRead;
                switch (instruction) {
                    case '>':
                        ++stackIndex;
                        if (stackIndex == stackEndIndex)
                            stackIndex = stackStartIndex;
                        break;
                    case '<':
                        if (stackIndex == stackStartIndex)
                            stackIndex = stackEndIndex;
                        --stackIndex;
                        break;
                    case '+':
                        ++stack[stackIndex];
                        break;
                    case '-':
                        --stack[stackIndex];
                        break;
                    case '.':
                        Console.Write((Char)stack[stackIndex]);
                        break;
                    case ',':
                        stack[stackIndex] = (Byte)Console.Read();
                        break;
                    case '[':
                        if (stack[stackIndex] == 0) {
                            while (instructionStream.ReadByte() != ']')
                                ;
                        }
                        else
                            jumps.Push(instructionStream.Position);
                        break;
                    case ']':
                        if (stack[stackIndex] != 0)
                            instructionStream.Seek(jumps.Peek(), SeekOrigin.Begin);
                        else
                            jumps.Pop();
                        break;
                }
            }
        }
    }
}
