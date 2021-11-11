namespace Gdxx.Modbus
{
    public enum ModbusWriteCode
    {
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