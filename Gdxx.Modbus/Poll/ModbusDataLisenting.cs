using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gdxx.Modbus
{
    internal class ModbusDataLisenting : IModbusDataLisenting
    {
        private bool lisenting;
        private IReadOnlyList<ModbusCodeDictionary> dataList;
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

        public async void Start(IEnumerable<IModbusCodeSet> codeSetSource,IEnumerable<IModbusData> dataSource)
        {   
            if (null == codeSetSource)
            {
                throw new ArgumentNullException(nameof(codeSetSource), "请设置要监听的 Modbus Code 数据");
            }
            
            if (lisenting)
            {
                throw new Exception("Modbus 数据监听已启动");
            }

            try
            {
                lisenting = true;
                dataList = codeSetSource.Select(reader => new ModbusCodeDictionary(reader, dataSource.Where(p=>p.Code == reader.Code))).ToList();
                while (lisenting)
                {
                    var dictionary = await Task.Run(() => dataChangedHandler.Invoke(dataList));
                    OnDataChanged(dictionary);
                    await Task.Delay(100);
                }
            }
            finally
            {
                lisenting = false;
            }
        }
            
        private void OnDataChanged(IReadOnlyDictionary<IModbusData, object> dictionary)
        {
            DataChanged?.Invoke(this, new ModbusDataChangedEventArgs(dictionary));
        }

        public void Stop()
        {
            lisenting = false;
        }
    }
}