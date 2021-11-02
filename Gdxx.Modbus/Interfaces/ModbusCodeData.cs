namespace Gdxx.Modbus
{
    /// <inheritdoc />
    public class ModbusCodeData : IModbusCodeData
    {
        /// <inheritdoc />
        public ModbusCode Code { get; set; }

        /// <inheritdoc />
        public int DataAddress { get; set; }

        /// <inheritdoc />
        public int DataIndex { get; set; }

        /// <inheritdoc />
        public int IsOffset { get; set; }
    }
}