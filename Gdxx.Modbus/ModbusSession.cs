using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyModbus;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus 客户端与服务器对话
    /// <para>通讯协议为：TCP/IP</para>
    /// </summary>
    public sealed class ModbusSession
    {
        private readonly ModbusClient client;

        /// <summary>
        /// IP 地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => client?.Connected ?? false;

        /// <summary>
        /// Modbus 已连接 
        /// </summary>
        public event EventHandler Connected;

        /// <summary>
        /// Modbus 连接断开 
        /// </summary>
        public event EventHandler Disconnected;

        public IModbusDataLisenting Lisenting { get; }

        public ModbusSession()
        {
            client = new ModbusClient();
            client.ConnectionTimeout = 3 * 1000;
            Port = -1;
            Lisenting = new ModbusDataLisenting(DataChangedHandler);
        }

        private IReadOnlyDictionary<IModbusCodeData, ValueType> DataChangedHandler(IReadOnlyDictionary<IModbusCodeData, ValueType> dictionary)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            if (string.IsNullOrEmpty(IPAddress))
            {
                throw new Exception("IP 地址不能为空");
            }

            if (Port < 0)
            {
                throw new Exception("无效的端口号");
            }

            client.ConnectedChanged +=OnConnectedChanged;
            client.Connect(IPAddress, Port);
        }

        private void OnConnectedChanged(object sender)
        {
            if (client.Connected)
            {
                Connected?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                Disconnected?.Invoke(this, EventArgs.Empty);
                client.ConnectedChanged -= OnConnectedChanged;
            }
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
}