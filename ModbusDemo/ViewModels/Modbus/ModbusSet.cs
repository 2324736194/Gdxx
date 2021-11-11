using System.Collections.Generic;
using Gdxx.Modbus;

namespace ModbusDemo.ViewModels
{
    public class ModbusSet
    {
        public string IPAddress { get; set; }
        public int Port { get; set; }
        public List<CodeSet> CodeSetList { get; set; }
        public List<ModbusBoolean> BooleanList { get; set; }
        public List<ModbusSingle> SingleList { get; set; }
        public List<ModbusInt32> Int32List { get; set; }
    }       

    public class CodeSet : IModbusCodeSet
    {
        public ModbusCode Code { get; set; }
        public int Start { get; set; }
        public int Quantity { get; set; }
    }
}