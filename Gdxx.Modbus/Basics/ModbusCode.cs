namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 代码
    /// </summary>
    public enum ModbusCode
    {
        /// <summary>
        /// 读取线圈状态
        /// </summary>
        ReadCoilStatus ,

        /// <summary>
        /// 读取离散输入状态
        /// </summary>
        ReadInputStatus ,

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        ReadHoldingRegister ,

        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        ReadInputRegister,
    }
}