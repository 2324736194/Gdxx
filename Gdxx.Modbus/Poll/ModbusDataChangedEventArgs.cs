using System;
using System.Collections;
using System.Collections.Generic;

namespace Gdxx.Modbus
{
    /// <inheritdoc />
    public class ModbusDataChangedEventArgs : EventArgs, IReadOnlyDictionary<IModbusData, object>
    {
        private readonly IReadOnlyDictionary<IModbusData, object> dictionary;

        /// <inheritdoc />
        public ModbusDataChangedEventArgs(IReadOnlyDictionary<IModbusData, object> dictionary)
        {
            this.dictionary = dictionary;
        }

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<IModbusData, object>> GetEnumerator()
        {
            return dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) dictionary).GetEnumerator();
        }

        /// <inheritdoc />
        public int Count
        {
            get => dictionary.Count;
        }

        /// <inheritdoc />
        public bool ContainsKey(IModbusData key)
        {
            return dictionary.ContainsKey(key);
        }

        /// <inheritdoc />
        public bool TryGetValue(IModbusData key, out object value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc />
        public object this[IModbusData key]
        {
            get => dictionary[key];
        }

        /// <inheritdoc />
        public IEnumerable<IModbusData> Keys
        {
            get => dictionary.Keys;
        }

        /// <inheritdoc />
        public IEnumerable<object> Values
        {
            get => dictionary.Values;
        }
    }
}