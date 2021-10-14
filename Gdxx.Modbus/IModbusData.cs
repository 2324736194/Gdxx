using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public interface IModbusData
    {
        ModbusCode Code { get; }

        /// <summary>
        /// 起始地址
        /// </summary>
        int Start { get; }
            
        /// <summary>
        /// 数据量
        /// </summary>
        int Quantity { get; }

        /// <summary>
        /// 索引列表
        /// </summary>
        IReadOnlyList<int> IndexList { get; }
    }
}