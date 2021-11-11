namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 数据写入
    /// </summary>
    public interface IModbusDataWrite
    {
        /// <summary>
        /// Modbus 数据
        /// </summary>
        IModbusData Data { get;  }

        /// <summary>
        /// 值
        /// </summary>
        object Value { get;  }
    }
}