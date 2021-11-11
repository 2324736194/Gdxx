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
    public class ReadCoilStatusWriteDialogViewModel : DialogAwareViewModel
    {
        private bool writeValue;
        private int index;

        public int Index
        {
            get => index;
            set => this.SetValue(ref index, value);
        }

        public bool WriteValue
        {
            get => writeValue;
            set => this.SetValue(ref writeValue, value);
        }

        public ICommand WriteCommand { get; }

        public ReadCoilStatusWriteDialogViewModel()
        {
            WriteCommand = new DelegateCommand(WriteCommandExecuteMethod);
        }

        private void WriteCommandExecuteMethod()
        {
            var ioc = Application.Current.PrismIoc();
            var aggregator = ioc.ContainerProvider.Resolve<IEventAggregator>();
            var pubSubEvent = aggregator.GetEvent<PubSubEvent<ModbusDataWriteEventArgs>>();
            pubSubEvent.Publish(new ModbusDataWriteEventArgs(ModbusCode.ReadCoilStatus, Index, writeValue));
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            var item = parameters.GetValue<ModbusDataItemViewModel>(nameof(ModbusDataItemViewModel));
            //Title = item.Code.ToString();
            //Index = item.Index;
            //WriteValue = (bool) item.ItemValue;
        }
    }
}