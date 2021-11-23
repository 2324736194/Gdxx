using System;
using EasyModbus;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 从站服务
    /// </summary>
    public sealed class ModbusSlave : IDisposable
    {
        private readonly ModbusServer server;
        private readonly ModbusServer.HoldingRegisters holdingRegisters;

        /// <summary>
        ///  Modbus 从站服务是否已启动
        /// </summary>
        public bool IsStarted { get; private set; }

        /// <summary>
        /// 构造 Modbus 从站服务
        /// </summary>
        /// <param name="port">端口号，默认 502。</param>
        public ModbusSlave(int port = 502)
        {
            server = new ModbusServer();
            server.Port = port;
            holdingRegisters = server.holdingRegisters;
        }

        /// <summary>
        /// 启动 Modbus 从站服务
        /// </summary>
        public void Start()
        {
            server.Listen();
            IsStarted = true;
        }

        /// <summary>
        /// 停止 Modbus 从站服务
        /// </summary>
        public void Stop()
        {
            if (IsStarted)
            {
                server.StopListening();
                IsStarted = false;
            }
        }

        /// <summary>
        /// 向 HoldingRegisters 写入浮点值，浮点数占 2 个位置。
        /// <para>例如：向索引地址 1 写入数据时，会占据索引 1 和 2。</para>
        /// <para>请注意写入时，索引地址是否被占用</para>
        /// </summary>
        /// <param name="index">数据索引，索引从 1 开始</param>
        /// <param name="value"></param>
        /// <param name="format"></param>
        public void WriteHoldingRegisters(int index, float value,ModbusSingleFormat format = ModbusSingleFormat.CDAB)
        {
            var registers = new short[2];
            registers[0] = holdingRegisters[index];
            registers[1] = holdingRegisters[index + 1];
            RegisterAssistant.Changed(ref registers, format, value);
            holdingRegisters[index] = registers[0];
            holdingRegisters[index + 1] = registers[1];
        }

        /// <summary>
        /// 向 HoldingRegisters 写入 <see cref="bool"/>
        /// <para>True：1</para>
        /// <para>False：0</para>
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        public void WriteHoldingRegisters(int index, bool value)
        {
            holdingRegisters[index] = (short) (value ? 1 : 0);
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Stop();
        }
    }
}