using System;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace Gdxx.ConsoleDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var program = new ModbusProgram();
            program.Run().Wait();
            Console.ReadLine();
        }
    }
}
