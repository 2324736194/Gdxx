using System;
using Gdxx.Modbus;

namespace ModbusDemo.ViewModels
{
    public class ModbusDataWriteEventArgs : EventArgs, IModbusDataWrite
    {
        public IModbusData Data { get; set; }

        public object Value { get; set; }
    }
}