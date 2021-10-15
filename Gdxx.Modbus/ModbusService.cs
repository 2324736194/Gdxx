using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyModbus;

namespace Gdxx.Modbus
{
    public sealed class ModbusService
    {
        private readonly ModbusClient client;
        private bool lisenting;
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

        public async void Lisenting(IReadOnlyList<IModbusData> list)
        {
            if (null == list)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (!client.Connected)
            {
                throw new Exception("请先连接 Modbus");
            }

            if (lisenting) return;
            try
            {
                lisenting = true;
                if (null == modbusDictionary)
                {
                    modbusDictionary = new Dictionary<IModbusData, Dictionary<int, ValueType>>();
                }

                while (lisenting)
                {
                    var changed = await Task.Run(() => GetChangedDictionary(list));
                    OnModbusDataChanged(new ModbusDataChangedEventArgs(changed));
                    await Task.Delay(100);
                }
            }
            finally
            {
                lisenting = false;
            }
        }

        public void Lisented()
        {
            lisenting = false;
        }

        private IReadOnlyDictionary<IModbusData, IReadOnlyDictionary<int, ValueType>> GetChangedDictionary(IReadOnlyList<IModbusData> list)
        {
            try
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
                            var coils = ReadHandler(item, (start, quantity) => client.ReadCoils(start, quantity));
                            ChangedHandler(coils.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                            break;
                        case ModbusCode.ReadInputStatus:
                            var discreteInputs = ReadHandler(item, (start, quantity) => client.ReadDiscreteInputs(start, quantity));
                            ChangedHandler(discreteInputs.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                            break;
                        case ModbusCode.ReadHoldingRegister:
                            var holdingRegisters = ReadHandler(item, (start, quantity) => client.ReadHoldingRegisters(start, quantity));
                            ChangedHandler(holdingRegisters.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                            break;
                        case ModbusCode.ReadInputRegister:
                            var inputRegisters = ReadHandler(item, (start, quantity) => client.ReadInputRegisters(start, quantity));
                            ChangedHandler(inputRegisters.OfType<ValueType>().ToArray(), item, dictionary, changedDictionary);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                return result.ToDictionary(p => p.Key, p => (IReadOnlyDictionary<int, ValueType>)p.Value);
            }
            catch (Exception ex)
            {
                throw new Exception("请检查 Modbus 服务器配置（连接地址或数据量长度）。", ex);
            }
        }
            
        private List<T> ReadHandler<T>(IModbusData item, ModbusReadHandler<T> read)
        {
            var max = 100;
            var length = item.Quantity / max;
            if (item.Quantity % max > 0)
            {
                length++;
            }
            var list = new List<T>();
            var index = item.Start;
            for (var i = 0; i < length; i++)
            {
                var count = item.Quantity - i * max;              
                if (count > max)
                {
                    count = max;
                }
                var result = read.Invoke(index, count);
                list.AddRange(result);
                index += 100;
            }
            return list;
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

        private void Check()
        {
            if (!client.Connected)
            {
                throw new Exception("请先连接 Modbus");
            }
        }

        public void Write(int index, bool value)
        {
            Check();
            client.WriteSingleCoil(index, value);
        }

        public void Write(int index, int value)
        {
            Check();
            client.WriteSingleRegister(index, value);
        }
    }

    public delegate IEnumerable<T> ModbusReadHandler<T>(int start, int quantity);

    public class ModbusConnectedChangedEventArgs : EventArgs
    {
        public ModbusConnectedChangedEventArgs(bool isConnected)
        {
            IsConnected = isConnected;
        }

        public bool IsConnected { get; }
    }
}