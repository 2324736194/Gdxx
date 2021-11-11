namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 代码设置
    /// </summary>
    public interface IModbusCodeSet
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