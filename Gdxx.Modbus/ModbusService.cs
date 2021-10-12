﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyModbus;

namespace Gdxx.Modbus
{
    public sealed class ModbusService
    {
        private readonly ModbusClient client;
        private bool lisented;
        private Dictionary<IModbusData, Dictionary<int, ValueType>> modbusDictionary;

        public string IPAddress { get; set; }

        public int Port { get; set; }

        public bool IsConnected => client?.Connected ?? false;

        public event EventHandler<ModbusConnectedChangedEventArgs> ModbusConnectedChanged;

        public event EventHandler<ModbusDataChangedEventArgs> ModbusDataChanged;

        public ModbusService()
        {
            client = new ModbusClient();
            client.ConnectionTimeout = 3 * 1000;
            Port = -1;
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(IPAddress))
            {
                throw new NotImplementedException("IP 地址不能为空");
            }
                
            if (Port < 0)
            {
                throw new NotImplementedException("无效的端口号");
            }

            client.ConnectedChanged += Client_ConnectedChanged;
            client.Connect(IPAddress, Port);

        }

        private void Client_ConnectedChanged(object sender)
        {
            OnModbusConnectedChanged(new ModbusConnectedChangedEventArgs(client.Connected));
        }

        public async void Lisenting(IReadOnlyList<ModbusData> list)
        {
            if (null == list)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (!client.Connected)
            {
                throw new Exception("请先连接 Modbus");
            }

            if (lisented) return;
            try
            {
                lisented = true;
                if (null == modbusDictionary)
                {
                    modbusDictionary = new Dictionary<IModbusData, Dictionary<int, ValueType>>();
                }

                while (client.Connected)
                {
                    var changed = await Task.Run(() => GetChangedDictionary(list));
                    OnModbusDataChanged(new ModbusDataChangedEventArgs(changed));
                    await Task.Delay(100);
                }
            }
            finally
            {
                lisented = false;
            }
        }

        private IReadOnlyDictionary<IModbusData, IReadOnlyDictionary<int, ValueType>> GetChangedDictionary(IReadOnlyList<ModbusData> list)
        {
            var result = new Dictionary<IModbusData, Dictionary<int, ValueType>>();
            foreach (var item in list)
            {
                if (!modbusDictionary.ContainsKey(item))
                {
                    modbusDictionary[item] = new Dictionary<int, ValueType>();
                }

                var dictionary = modbusDictionary[item];
                var changedDictionary = result[item] = new Dictionary<int, ValueType>();
                switch (item.Code)
                {
                    case ModbusCode.ReadCoilStatus:
                        var coils = client.ReadCoils(item.Start, item.Quantity);
                        ChangedHandler(coils.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                        break;
                    case ModbusCode.ReadInputStatus:
                        var discreteInputs = client.ReadDiscreteInputs(item.Start, item.Quantity);
                        ChangedHandler(discreteInputs.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                        break;
                    case ModbusCode.ReadHoldingRegiste:
                        var holdingRegisters = client.ReadHoldingRegisters(item.Start, item.Quantity);
                        ChangedHandler(holdingRegisters.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                        break;
                    case ModbusCode.ReadInputRegiste:
                        var inputRegisters = client.ReadInputRegisters(item.Start, item.Quantity);
                        ChangedHandler(inputRegisters.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                        break;
                    case ModbusCode.WriteSingleCoil:
                    case ModbusCode.WriteSingleRegister:
                    case ModbusCode.WriteMultipleCoil:
                    case ModbusCode.WriteMultipleRegister:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result.ToDictionary(p => p.Key, p => (IReadOnlyDictionary<int, ValueType>) p.Value);
        }

        private void ChangedHandler(ValueType[] arrary, IModbusData data, Dictionary<int, ValueType> dictionary, Dictionary<int, ValueType> changedDictionary)
        {
            for (int i = 0; i < arrary.Length; i++)
            {
                var key = data.Start + i;
                var value = arrary[i];
                if (dictionary.ContainsKey(key))
                {
                    if (!Equals(dictionary[key], value))
                    {
                        dictionary[key] = value;
                        changedDictionary[key] = value;
                    }
                }
                else
                {
                    dictionary[key] = value;
                    changedDictionary[key] = value;
                }
            }
        }

        private void OnModbusDataChanged(ModbusDataChangedEventArgs e)
        {
            ModbusDataChanged?.Invoke(this, e);
        }

        private void OnModbusConnectedChanged(ModbusConnectedChangedEventArgs e)
        {
            ModbusConnectedChanged?.Invoke(this, e);
        }
    }

    public class ModbusConnectedChangedEventArgs : EventArgs
    {
        public ModbusConnectedChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; }
    }
}