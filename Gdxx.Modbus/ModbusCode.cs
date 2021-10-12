namespace Gdxx.Modbus
{
    public enum ModbusCode
    {
        /// <summary>
        /// 读取线圈状态
        /// </summary>
        ReadCoilStatus = 1,

        /// <summary>
        /// 读取离散输入状态
        /// </summary>
        ReadInputStatus = 2,

        /// <summary>
        /// 读取保持寄存器
        /// </summary>
        ReadHoldingRegiste = 3,

        /// <summary>
        /// 读取输入寄存器
        /// </summary>
        ReadInputRegiste = 4,

        /// <summary>
        /// 写线圈状态
        /// </summary>
        WriteSingleCoil = 5,

        /// <summary>
        /// 写单个保持寄存器
        /// </summary>
        WriteSingleRegister = 6,

        /// <summary>
        /// 写多个线圈状态
        /// </summary>
        WriteMultipleCoil = 15,

        /// <summary>
        /// 写多个保持寄存器
        /// </summary>
        WriteMultipleRegister = 16
    }
}