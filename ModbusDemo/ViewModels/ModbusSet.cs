using System.Collections.Generic;
using Gdxx.Modbus;

namespace ModbusDemo.ViewModels
{
    public class ModbusSet
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public List<ModbusData> DataList { get; set; } = new List<ModbusData>();
    }
}