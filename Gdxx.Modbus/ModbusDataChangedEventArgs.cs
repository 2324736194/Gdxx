using System;
using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public class ModbusDataChangedEventArgs : EventArgs
    {
        public ModbusDataChangedEventArgs(IReadOnlyDictionary<IModbusData, IReadOnlyDictionary<int, ValueType>> dictionary)
        {   
            Dictionary = dictionary;
        }

        public IReadOnlyDictionary<IModbusData, IReadOnlyDictionary<int, ValueType>> Dictionary { get; }
    }
}