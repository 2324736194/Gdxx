using System;
using Gdxx.Modbus;

namespace ModbusDemo.ViewModels
{
    public class ModbusDataWriteEventArgs : EventArgs, IModbusDataWrite
    {
        public IModbusData Data { get; }

        public object Value { get; }

        public ModbusDataWriteEventArgs(IModbusData data, object value)
        {
            Data = data;
            Value = value;
        }
    }
}