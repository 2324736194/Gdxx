namespace Gdxx.Modbus
{
    /// <inheritdoc />
    public abstract class ModbusData : IModbusData
    {
        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public ModbusCode Code { get; set; }

        /// <inheritdoc />
        public int DataAddress { get; set; }
    }
}