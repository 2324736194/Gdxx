using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Gdxx.Modbus;
using ModbusDemo.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.ViewModels;

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
            switch (Data.Code)
            {
                case ModbusCode.ReadCoilStatus:
                case ModbusCode.ReadHoldingRegister:
                    break;
                default:
                    MessageBox.Show("当前数据是只读的", "系统提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }

            var source = default(string);
            switch (Data)
            {
                case ModbusBoolean boolean:

                    break;
                case ModbusSingle single:
                    source = nameof(ModbusSingleWriteDialog);
                    break;
                case ModbusInt32 int32:

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

    public class ModbusSingleWriteDialogViewModel : ModbusDataWriteDialogViewModel<ModbusSingle, float>
    {

    }

    public abstract class ModbusDataWriteDialogViewModel<TModbusData, TValue> : DialogAwareViewModel
        where TModbusData : IModbusData
    {
        private TModbusData data;
        private TValue value;

        public TValue Value
        {
            get => value;
            set => this.SetValue(ref this.value, value);
        }

        public TModbusData Data
        {
            get => data;
            set => this.SetValue(ref data, value);
        }

        public ICommand WriteCommand { get; }

        protected ModbusDataWriteDialogViewModel()
        {
            WriteCommand = new DelegateCommand(WriteCommandExecuteMethod);
        }

        private void WriteCommandExecuteMethod()
        {
            var ioc = Application.Current.PrismIoc();
            var aggregator = ioc.ContainerProvider.Resolve<IEventAggregator>();
            var pubSubEvent = aggregator.GetEvent<PubSubEvent<ModbusDataWriteEventArgs>>();
            pubSubEvent.Publish(new ModbusDataWriteEventArgs()
            {
                Data = Data,
                Value = Value
            });
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            Data = parameters.GetValue<TModbusData>(nameof(IModbusData));
            Value = parameters.GetValue<TValue>(nameof(Value));
        }
    }
}