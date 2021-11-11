using System;

namespace Gdxx.Modbus
{
    /// <inheritdoc />
    public class ModbusInt32 : ModbusData
    {
        /// <inheritdoc />
        public override string ToString()
        {
            return nameof(Int32);
        }
    }
}