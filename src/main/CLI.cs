using Neo;
using Neo.Bytecode;
using System;
using System.IO;
using System.Text;

namespace Neo.CLI {
    public sealed class CLI {
        private static void ShowUsage() {
            Console.WriteLine("Usage: neo [-h|-v|<file>]");
        }

        private static void ShowVersion() {
            Console.WriteLine($"Neo {VM.VERSION}");
        }

        internal static void Main(string[] args) {
            Console.OutputEncoding = Encoding.UTF8;

            if (args.Length < 1) {
                ShowVersion();
                ShowUsage();
                return;
            }

            var pargs = new string[args.Length - 1];
            for (var i = 0; i < pargs.Length; i++) {
                pargs[i] = args[i + 1];
            }

            switch (args[0]) {
                case "-h":
                    ShowUsage();
                    break;
                case "-v":
                    ShowVersion();
                    break;
                default:
                    var vm = new VM();
                    vm.Run(args[0], pargs);
                    break;
            }
        }
    }
}