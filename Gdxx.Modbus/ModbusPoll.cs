using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EasyModbus;
using EasyModbus.Exceptions;

namespace Gdxx.Modbus
{
    /// <summary>
    /// Modbus Poll
    /// <para>通讯协议为：TCP/IP</para>
    /// </summary>
    public sealed class ModbusPoll
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

        /// <summary>
        /// Modbus 数据监听
        /// </summary>
        public IModbusDataLisenting Lisenting { get; }

        /// <summary>
        /// 实例化 Modbus Poll
        /// </summary>
        public ModbusPoll()
        {
            client = new ModbusClient();
            client.ConnectionTimeout = 3 * 1000;
            Port = -1;
            Lisenting = new ModbusDataLisenting(DataChangedHandler);
        }

        private IReadOnlyDictionary<IModbusData, object> DataChangedHandler(IReadOnlyList<ModbusCodeDictionary> dataSource)
        {
            var changedDictionary = new Dictionary<IModbusData, object>();
            foreach (var dictionary in dataSource)
            {
                switch (dictionary.Code)
                {
                    case ModbusCode.ReadCoilStatus:
                        var coils = ReadHandler(dictionary, (start, quantity) => client.ReadCoils(start, quantity));
                        var keys = dictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            var index = key.DataAddress - dictionary.Start;
                            var value = coils[index];
                            var oldValue = dictionary[key];
                            if (Equals(value, oldValue)) continue;
                            dictionary[key] = value;
                            changedDictionary[key] = value;
                        }
                        break;
                    case ModbusCode.ReadInputStatus:
                        var discreteInputs = ReadHandler(dictionary, (start, quantity) => client.ReadDiscreteInputs(start, quantity));
                        keys = dictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            var index = key.DataAddress - dictionary.Start;
                            var value = discreteInputs[index];
                            var oldValue = dictionary[key];
                            if (Equals(value, oldValue)) continue;
                            dictionary[key] = value;
                            changedDictionary[key] = value;
                            
                        }
                        break;
                    case ModbusCode.ReadHoldingRegister:
                        var holdingRegisters = ReadHandler(dictionary, (start, quantity) =>
                            client.ReadHoldingRegisters(start, quantity)
                            );
                        var registers = holdingRegisters.Select(p => (short) p);
                        var registerAssistant = new RegisterAssistant(registers, dictionary.Start);
                        keys = dictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            var oldValue = dictionary[key];
                            var value = registerAssistant.GetValue(key);
                            if (Equals(value, oldValue)) continue;
                            dictionary[key] = value;
                            changedDictionary[key] = value;
                        }   
                        break;  
                    case ModbusCode.ReadInputRegister:
                        var inputRegisters = ReadHandler(dictionary, (start, quantity) => 
                            client.ReadInputRegisters(start, quantity)
                            );
                        registers = inputRegisters.Select(p => (short)p);
                        registerAssistant = new RegisterAssistant(registers, dictionary.Start);
                        keys = dictionary.Keys.ToList();
                        foreach (var key in keys)
                        {
                            var oldValue = dictionary[key];
                            var value = registerAssistant.GetValue(key);
                            if (Equals(value, oldValue)) continue;
                            dictionary[key] = value;
                            changedDictionary[key] = value;
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return changedDictionary;
        }

        private List<T> ReadHandler<T>(ModbusCodeDictionary dictionary, ModbusReadHandler<T> handler)
        {
            var index = dictionary.Start;
            var quantity = dictionary.Quantity;
            var max = 100;
            var length = quantity / max;
            if (quantity % max > 0)
            {
                length++;
            }

            var list = new List<T>();
            for (var i = 0; i < length; i++)
            {
                var count = quantity - i * max;
                if (count > max)
                {
                    count = max;
                }

                try
                {
                    var result = handler.Invoke(index, count);
                    list.AddRange(result);
                }
                catch (FunctionCodeNotSupportedException)
                {
                    throw new Exception($"请启用函数代码：{dictionary.Code}。");
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message, ex.InnerException);
                }

                index += 100;
            }

            return list;
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <exception cref="Exception"></exception>
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

            if (IsConnected) return;
            client.ConnectedChanged +=OnConnectedChanged;
            try
            {

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
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
        
        private void Check()
        {
            if (!client.Connected)
            {
                throw new Exception("请先连接 Modbus");
            }
        }
        
        /// <summary>
        /// 写入 Modbus 数据
        /// </summary>
        /// <param name="write"></param>
        public void Write(IModbusDataWrite write)
        {
            Check();
            var code = write.Data.Code;
            switch (code)
            {
                case ModbusCode.ReadCoilStatus:
                    WriteReadCoilStatus(write);
                    break;
                case ModbusCode.ReadHoldingRegister:
                    WriteReadHoldingRegister(write);
                    break;
                default:
                    throw new Exception($"代码 {code} 是只读的，不允许写入。");
            }
        }

        private void WriteReadHoldingRegister(IModbusDataWrite write)
        {
            switch (write.Data)
            {
                case ModbusBoolean data:
                {
                    if (write.Value is bool value)
                    {
                        var registers = client.ReadHoldingRegisters(data.DataAddress, 1);
                        var register = (short) registers.Single();
                        RegisterAssistant.Changed(ref register, data, value);
                        client.WriteSingleRegister(data.DataAddress, register);
                        break;
                    }
                    
                    throw new Exception(GetErrorMessage(data));
                }

                case ModbusSingle data:
                {
                    if (write.Value is float value)
                    {
                        var registers = client.ReadHoldingRegisters(data.DataAddress, 2);
                        RegisterAssistant.Changed(ref registers, data, value);
                        client.WriteMultipleRegisters(data.DataAddress, registers);
                        break;
                    }

                    throw new Exception(GetErrorMessage(data));
                }

                case ModbusInt32 data:
                {
                    if (write.Value is int value)
                    {
                        client.WriteSingleRegister(data.DataAddress, value);
                        break;
                    }

                    throw new Exception(GetErrorMessage(data));
                }
            }
        }

        private string GetErrorMessage(IModbusData data)
        {
            var dataType = string.Empty;
            switch (data)
            {
                case ModbusSingle data1:
                    dataType = nameof(Single);
                    break;
                case ModbusInt32 data1:
                    dataType = nameof(Int32);
                    break;
                case ModbusBoolean data1:
                    dataType = nameof(Boolean);
                    break;
            }

            var builder = new StringBuilder();
            builder.AppendFormat("代码 {0} 中数据地址 {1} 只能写入 {2} 类型的数据", data.Code, data.DataAddress, dataType);
            return builder.ToString();
        }

        private void WriteReadCoilStatus(IModbusDataWrite write)
        {
            var data = write.Data;
            if (write.Value is bool value)
            {
                client.WriteSingleCoil(data.DataAddress, value);
                return;
            }

            var builder = new StringBuilder();
            builder.AppendFormat("代码 {0} 仅允许写入 {1} 类型的数据", ModbusCode.ReadCoilStatus, nameof(Boolean));
            throw new Exception(builder.ToString());
        }
    }
}