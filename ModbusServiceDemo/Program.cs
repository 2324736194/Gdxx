using System;
using System.Collections;
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
            var port = default(int);

            while (true)
            {
                Console.WriteLine("请输入端口号：");
                var read = Console.ReadLine();
                if (int.TryParse(read, out port))
                {
                    break;
                }
                Console.WriteLine("ERROR：请输入有效的端口号");
                Console.WriteLine("========================");
            }
            var service = new ModbusSlave(port);
            for (int i = 0; i < 3; i++)
            {
                var index = 1 + i * 2;
                var value = (float)(111 * index + 0.111 * (index + 1));
                service.WriteHoldingRegisters(index, value);
            }
            service.Start();
            Console.WriteLine("服务已启动");
            Console.WriteLine("========================");
            while (true)
            {
                Console.WriteLine("请输入要改变的索引：1、3、5");
                var read = Console.ReadLine();
                if (int.TryParse(read, out var index))
                {
                    switch (index)
                    {
                        case 1:
                        case 3:
                        case 5:
                            var random = new Random();
                            var next = random.Next(111,999);
                            var nextDouble = random.NextDouble();
                            var value = (next + (float)nextDouble);
                            Console.WriteLine("当前索引 {0}：{1}", index, value);
                            service.WriteHoldingRegisters(index, value);
                            break;
                        default:
                            continue;
                    }
                }
                Console.WriteLine("关闭服务请按 Esc");
                Console.WriteLine("========================");
                var key = Console.ReadKey();
                if (key.Key == ConsoleKey.Escape)
                {
                    return;
                }
                
            }
        }
    }
}
