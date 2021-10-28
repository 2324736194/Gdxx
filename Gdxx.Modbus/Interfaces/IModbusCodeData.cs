namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 代码数据
    /// </summary>
    public interface IModbusCodeData
    {
        /// <summary>
        /// 代码
        /// </summary>
        ModbusCode Code { get; }

        /// <summary>
        /// 数据地址
        /// <para>
        /// 数据地址范围在 <see cref="IModbusCodeSet.Start"/> ~ （<see cref="IModbusCodeSet.Start"/> + <see cref="IModbusCodeSet.Quantity"/>）之间
        /// </para>
        /// </summary>
        int DataAddress { get; }

        /// <summary>
        /// 数据索引  ，默认值为 -1
        /// <para>-1 表示当前未配置数据索引</para>
        /// <para>索引从 0 开始，最大索引为 15。</para>
        /// </summary>
        int DataIndex { get; }

        /// <summary>
        /// 是否位移，默认否。
        /// <para>默认情况下，数据索引的排列顺序为 0 ~ 15</para>
        /// <para>位移情况下，数据索引的排列顺序为 15 ~ 0</para>
        /// </summary>
        int IsOffset { get; }
    }

    public class ModbusCodeData : IModbusCodeData
    {
        public ModbusCode Code { get; set; }
        public int DataAddress { get; set; }
        public int DataIndex { get; set; }
        public int IsOffset { get; set; }
    }
}