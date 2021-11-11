using System;
using System.Collections.Generic;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 数据监听
    /// </summary>
    public interface IModbusDataLisenting
    {
        /// <summary>
        /// 开始监听
        /// </summary>
        /// <param name="codeSetSource"></param>
        /// <param name="dataSource"></param>
        void Start(IEnumerable<IModbusCodeSet> codeSetSource, IEnumerable<IModbusData> dataSource);

        /// <summary>
        /// 停止监听
        /// </summary>
        void Stop();

        /// <summary>
        /// 数据更改后
        /// </summary>
        event EventHandler<ModbusDataChangedEventArgs> DataChanged;
    }
}