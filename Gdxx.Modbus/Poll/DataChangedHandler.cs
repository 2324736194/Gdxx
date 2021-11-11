using System;
using System.Collections.Generic;
using EasyModbus;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 数据更新处理
    /// </summary>
    /// <param name="dataSource">Modbus 数据字典</param>
    /// <returns>Modbus 数据字典，仅包含已更新的数据</returns>
    public delegate IReadOnlyDictionary<IModbusData, object> DataChangedHandler(IReadOnlyList<ModbusCodeDictionary> dataSource);
}