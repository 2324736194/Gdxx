using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Gdxx.Modbus;
using ModbusDemo.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Services.Dialogs;

namespace ModbusDemo.ViewModels
{
    public class ModbusDataItemViewModel : ViewModel
    {
        private IModbusData data;
        private object value;

        public object Value
        {
            get => value;
            set => this.SetValue(ref this.value, value);
        }

        public IModbusData Data
        {
            get => data;
            set => this.SetValue(ref data, value);
        }
        
        public ICommand WriteDialogCommand { get; }

        public ModbusDataItemViewModel()
        {
            WriteDialogCommand = new DelegateCommand(WriteDialogCommandExecuteMethod);
        }

        public ModbusDataItemViewModel(IModbusData data)
            : this()
        {
            Data = data;
        }

        private void WriteDialogCommandExecuteMethod()
        {
            var source = default(string);
            switch (Data.Code)
            {
                case ModbusCode.ReadCoilStatus:
                case ModbusCode.ReadHoldingRegister:
                    break;
                default:
                    MessageBox.Show("当前数据是只读的", "系统提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }
            switch (Data)
            {
                case ModbusBoolean data:
                    source = nameof(ModbusBooleanWriteDialog);
                    break;
                case ModbusSingle data:
                    source = nameof(ModbusSingleWriteDialog);
                    break;
                case ModbusInt32 data:
                    source = nameof(ModbusInt32WriteDialog);
                    break;
                default:
                    throw new NotImplementedException();
            }
            var parameters = new DialogParameters();
            parameters.Add(nameof(IModbusData), Data);
            parameters.Add(nameof(Value), Value);
            var ioc = Application.Current.PrismIoc();
            var service = ioc.DialogService;
            service.ShowDialog(source, parameters, null);
        }
    }
}