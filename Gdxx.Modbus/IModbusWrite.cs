using System;

namespace Gdxx.Modbus
{
    public interface IModbusWrite
    {
        ModbusCode WriteCode { get; }

        int WriteIndex { get; }

        ValueType WriteValue { get; }
    }
}