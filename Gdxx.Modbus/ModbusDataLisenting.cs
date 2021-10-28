using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdxx.Modbus
{
    internal class ModbusDataLisenting : IModbusDataLisenting
    {
        private bool lisenting;
        private Dictionary<IModbusCodeData, ValueType> dataDictionary;
        private readonly DataChangedHandler dataChangedHandler;

        /// <summary>
        /// Modbus 数据更新后
        /// </summary>
        public event EventHandler<ModbusDataChangedEventArgs> DataChanged;

        public ModbusDataLisenting(DataChangedHandler dataChangedHandler)
        {
            if (null == dataChangedHandler)
            {
                throw new NotImplementedException("请检查代码逻辑，Modbus 数据更新处理未实现");
            }

            this.dataChangedHandler = dataChangedHandler;
        }

        public async void Start(IReadOnlyList<IModbusCodeData> list)
        {
            if (null == list)
            {
                throw new ArgumentNullException(nameof(list), "请设置要监听的 Modbus 数据列表");
            }

            if (lisenting)
            {
                throw new Exception("Modbus 数据监听已启动");
            }

            try
            {
                lisenting = true;
                dataDictionary = new Dictionary<IModbusCodeData, ValueType>();
                while (lisenting)
                {
                    var dictionary = await Task.Run(() => dataChangedHandler.Invoke(dataDictionary));
                    OnDataChanged(dictionary);
                    await Task.Delay(100);
                }
            }
            finally
            {
                lisenting = false;
            }
        }

        private void OnDataChanged(IReadOnlyDictionary<IModbusCodeData, ValueType> dictionary)
        {
            DataChanged?.Invoke(this, new ModbusDataChangedEventArgs(dictionary));
        }

        public void Stop()
        {
            lisenting = false;
        }
    }

    public interface IModbusDataLisenting
    {
        event EventHandler<ModbusDataChangedEventArgs> DataChanged;
    }
}