using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public class ModbusData : IModbusData
    {
        public ModbusCode Code { get; set; }
        public int Start { get; set; }
        public int Quantity { get; set; }
        public IReadOnlyList<int> IndexList { get; set; }
    }
}