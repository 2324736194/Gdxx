using System;
using System.Collections;
using System.Collections.Generic;

namespace Gdxx.Modbus
{
    public class ModbusDataChangedEventArgs : EventArgs, IReadOnlyDictionary<IModbusCodeData, ValueType>
    {
        private readonly IReadOnlyDictionary<IModbusCodeData, ValueType> dictionary;

        /// <inheritdoc />
        public ModbusDataChangedEventArgs(IReadOnlyDictionary<IModbusCodeData, ValueType> dictionary)
        {
            this.dictionary = dictionary;
        }

        public IEnumerator<KeyValuePair<IModbusCodeData, ValueType>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) dictionary).GetEnumerator();
        }

        public int Count
        {
            get => dictionary.Count;
        }

        public bool ContainsKey(IModbusCodeData key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool TryGetValue(IModbusCodeData key, out ValueType value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public ValueType this[IModbusCodeData key]
        {
            get => dictionary[key];
        }

        public IEnumerable<IModbusCodeData> Keys
        {
            get => dictionary.Keys;
        }

        public IEnumerable<ValueType> Values
        {
            get => dictionary.Values;
        }
    }
}