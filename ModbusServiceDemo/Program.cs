using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gdxx.Modbus;

namespace ModbusServiceDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("请输入端口号：");
            var read = Console.ReadLine();
            if (int.TryParse(read, out var port))
            {
                var service = new ModbusService(port);
                service.WriteHoldingRegisters(1, (float) 111.222);
                service.WriteHoldingRegisters(3, (float) 333.444);
                service.WriteHoldingRegisters(5, (float) 555.666);
                service.Start();
            }

            Console.ReadKey();
        }
    }
}
