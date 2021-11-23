using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Gdxx.Modbus;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
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
            pubSubEvent.Publish(new ModbusDataWriteEventArgs(Data, Value));
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            Title = "Modbus 数据写入";
            Data = parameters.GetValue<TModbusData>(nameof(IModbusData));
            Value = parameters.GetValue<TValue>(nameof(Value));
        }
    }
}