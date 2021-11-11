using System.Collections.Generic;

namespace Gdxx.Modbus
{
    /// <summary>
    /// 
    /// </summary>
    public class ModbusCodeSet : IModbusCodeSet
    {
        /// <summary>
        /// 代码
        /// </summary>
        public ModbusCode Code { get; set; }

        /// <summary>
        /// 起始地址
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// 数据量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 数据列表
        /// </summary>
        public List<IModbusData> DataList { get; set; }
    }
}