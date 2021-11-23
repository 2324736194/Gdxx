using System;
using System.Collections.Generic;

namespace Gdxx.Modbus
{
    /// <summary>
    /// 
    /// </summary>
    public class ModbusCodeDictionary : Dictionary<IModbusData, object>, IModbusCodeSet
    {
        /// <summary>
        /// 代码
        /// </summary>
        public ModbusCode Code { get; }

        /// <summary>
        /// 起始地址
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// 数据量
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="set"></param>
        /// <param name="dataList"></param>
        /// <exception cref="Exception"></exception>
        public ModbusCodeDictionary(IModbusCodeSet set,IEnumerable<IModbusData> dataList = null)
        {
            if (set.Quantity < 1)
            {
                throw new Exception($"请设置 {set.Code} 的数据量");
            }
            Code = set.Code;
            Start = set.Start;
            Quantity = set.Quantity;
            if (null != dataList)
            {
                foreach (var item in dataList)
                {
                    if (item.DataAddress < Start)
                    {
                        throw new Exception($"数据地址范围必须在 {Start} ~ {Start + Quantity} 之间");
                    }

                    Add(item, null);
                }
            }
        }
    }
}