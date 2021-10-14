using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Gdxx.Modbus;

namespace Gdxx.ConsoleDemo
{
    class ModbusProgram
    {
        private bool running = true;

        public async Task Run()
        {
            var service = new ModbusService();
            service.IPAddress = "127.0.0.1";
            service.Port = 502;
            service.ModbusConnectedChanged += Service_ModbusConnectedChanged;
            service.Connect();
            while (running)
            {
                await Task.Delay(100);
            }
        }

        private void Service_ModbusConnectedChanged(object sender, ModbusConnectedChangedEventArgs e)
        {
            if (e.IsConnected)
            {
                var service = (ModbusService)sender;
                var list = new List<ModbusData>();
                list.Add(new ModbusData()
                {
                    Code = ModbusCode.ReadCoilStatus,
                    Start = 0,
                    Quantity = 100,
                    IndexList = new List<int>()
                    {
                        0, 1, 2, 3, 4,
                        8, 9, 10, 11,
                        16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27,
                        32, 33, 34, 35, 36, 37,
                        48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 58,
                        64, 65, 66, 67, 68, 69,
                        72, 73, 74,
                        76, 77,
                        80, 81, 82, 83, 84, 85, 86, 87
                    }
                });
                list.Add(new ModbusData()
                {
                    Code = ModbusCode.ReadHoldingRegister,
                    Start = 0,
                    Quantity = 50,
                    IndexList = new List<int>()
                    {
                        0, 1, 2, 3, 4, 5,
                        7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28,
                        30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43
                    }
                });
                service.ModbusDataChanged += Service_ModbusDataChanged;
                service.Lisenting(list);
            }
            else
            {
                Console.WriteLine("连接失败");
                running = false;
            }
        }

        private void Service_ModbusDataChanged(object sender, ModbusDataChangedEventArgs e)
        {
            foreach (var item in e.Dictionary)
            {
                if (item.Value.Any())
                {
                    Console.WriteLine("===== {0}", item.Key.Code);
                    foreach (var item1 in item.Value)
                    {
                        var index = item1.Key.ToString("0000");
                        Console.WriteLine("{0}：{1}", index, item1.Value);
                    }
                    Console.WriteLine("=====");
                    Console.WriteLine();
                    Console.WriteLine();
                }
            }
        }
    }
}