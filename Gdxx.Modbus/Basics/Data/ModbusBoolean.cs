using System;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 布尔型数据
    /// </summary>
    public class ModbusBoolean : ModbusData
    {
        /// <summary>
        /// 是否允许拆分
        /// </summary>
        public bool CanSplit { get; set; } = false;

        /// <summary>
        /// 是否启用拆分
        /// </summary>
        public bool IsEnabledSplit { get; set; } = false;

        /// <summary>
        /// 拆分数据后的索引位置。
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 是否位移，默认不位移。
        /// <para>位移数据的索引顺序为 15~0</para>
        /// <para>不位移数据的索引顺序为 0~15</para>
        /// </summary>
        public bool IsOffset { get; set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return nameof(Boolean);
        }
    }
}