namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 浮点类型格式化，表示存放字节的方式。
    /// <para>一个字母表示2个字节</para>
    /// </summary>
    public enum ModbusSingleFormat
    {
        /// <summary>
        /// Big endian
        /// </summary>
        ABCD,

        /// <summary>
        /// Big endian byte swap
        /// </summary>
        BADC,

        /// <summary>
        /// Little endian byte swap
        /// </summary>
        CDAB,

        /// <summary>
        /// Little endian 
        /// </summary>
        DCBA
    }
}