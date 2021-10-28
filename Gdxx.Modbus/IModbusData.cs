﻿using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public interface IModbusData
    {
        /// <summary>
        /// 代码
        /// </summary>
        ModbusCode Code { get; }

        /// <summary>
        /// 起始地址
        /// </summary>
        int Start { get; }
            
        /// <summary>
        /// 数据量
        /// </summary>
        int Quantity { get; }
    }
}