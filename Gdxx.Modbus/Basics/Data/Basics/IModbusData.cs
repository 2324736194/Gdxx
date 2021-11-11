namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 数据
    /// </summary>
    public interface IModbusData
    {
        /// <summary>
        /// 数据名称
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 代码
        /// </summary>
        ModbusCode Code { get;  }

        /// <summary>
        /// 数据地址
        /// <para>
        /// 数据地址范围在 <see cref="IModbusCodeSet.Start"/> ~ （<see cref="IModbusCodeSet.Start"/> + <see cref="IModbusCodeSet.Quantity"/>）之间
        /// </para>
        /// </summary>
        int DataAddress { get; }
    }
}