using System;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 浮点数据
    /// </summary>
    public class ModbusSingle : ModbusData
    {
        /// <summary>
        /// 浮点数据格式化方式
        /// </summary>
        public ModbusSingleFormat SingleFormat { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return nameof(Single);
        }
    }
}