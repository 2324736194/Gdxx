using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Gdxx.Modbus;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class LisentingViewModel : NavigationAwareViewModel
    {
        private IRegionNavigationJournal journal;
        private ObservableDictionary<IModbusData, List<ModbusDataItemViewModel>> dictionary;

        public ObservableDictionary<IModbusData, List<ModbusDataItemViewModel>> Dictionary
        {
            get => dictionary;
            set => this.SetValue(ref dictionary, value);
        }
        
        public ICommand StopCommand { get; }

        public LisentingViewModel()
        {
            Dictionary = new ObservableDictionary<IModbusData, List<ModbusDataItemViewModel>>();
            StopCommand = new DelegateCommand(StopCommandExecuteMethod);
            var ioc = Application.Current.PrismIoc();
            var containerProvider = ioc.ContainerProvider;
            var aggregator = containerProvider.Resolve<IEventAggregator>();
            var pubSubEvent = aggregator.GetEvent<PubSubEvent<ModbusDataChangedEventArgs>>();
            pubSubEvent.Subscribe(Action);
        }
            
        private void Action(ModbusDataChangedEventArgs e)
        {
            foreach (var item in e.Dictionary)
            {
                var code = item.Key.Code;
                if (Dictionary.Keys.All(p => p.Code != code))
                {
                    Dictionary[item.Key] = new List<ModbusDataItemViewModel>();
                }
                var list = Dictionary[item.Key];
                foreach (var item1 in item.Value)
                {
                    var item2 = list.FirstOrDefault(p => p.Index == item1.Key);
                    if (null == item2)
                    {
                        item2 =new ModbusDataItemViewModel()
                        {
                            Code = code,
                            Index = item1.Key
                        };
                        list.Add(item2);
                    }

                    item2.ItemValue = item1.Value;
                }
            }
        }

        private void StopCommandExecuteMethod()
        {
            journal.GoBack();
        }
        
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            journal = navigationContext.NavigationService.Journal;
        }
    }

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
            var pubSubEvent = aggregator.GetEvent<PubSubEvent<ModbusWriteEventArgs>>();
            pubSubEvent.Publish(new ModbusWriteEventArgs(ModbusCode.ReadCoilStatus, Index, writeValue));
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            var item = parameters.GetValue<ModbusDataItemViewModel>(nameof(ModbusDataItemViewModel));
            Title = item.Code.ToString();
            Index = item.Index;
            WriteValue = (bool) item.ItemValue;
        }
    }

    public class ReadHoldingRegisterWriteDialogViewModel : DialogAwareViewModel
    {
        private int writeValue;
        private int index;

        public int Index
        {
            get => index;
            set => this.SetValue(ref index, value);
        }

        public int WriteValue
        {
            get => writeValue;
            set => this.SetValue(ref writeValue, value);
        }

        public ICommand WriteCommand { get; }

        public ReadHoldingRegisterWriteDialogViewModel()
        {
            WriteCommand = new DelegateCommand(WriteCommandExecuteMethod);
        }

        private void WriteCommandExecuteMethod()
        {
            var ioc = Application.Current.PrismIoc();
            var aggregator = ioc.ContainerProvider.Resolve<IEventAggregator>();
            var pubSubEvent = aggregator.GetEvent<PubSubEvent<ModbusWriteEventArgs>>();
            pubSubEvent.Publish(new ModbusWriteEventArgs(ModbusCode.ReadHoldingRegister, Index, writeValue));
            OnRequestClose(new DialogResult(ButtonResult.OK));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            var item = parameters.GetValue<ModbusDataItemViewModel>(nameof(ModbusDataItemViewModel));
            Title = item.Code.ToString();
            Index = item.Index;
            WriteValue = (int)item.ItemValue;
        }
    }

    public class ModbusDataItemViewModel : ViewModel
    {
        private int index;
        private ValueType itemValue;
        private ModbusCode code;
     
        public ModbusCode Code
        {
            get => code;
            set => this.SetValue(ref code, value);
        }

        public ValueType ItemValue
        {
            get => itemValue;
            set => this.SetValue(ref itemValue, value);
        }

        public int Index
        {
            get => index;
            set => this.SetValue(ref index, value);
        }

        public ICommand WriteDialogCommand { get; }

        public ModbusDataItemViewModel()
        {
            WriteDialogCommand = new DelegateCommand(WriteDialogCommandExecuteMethod);
        }

        private void WriteDialogCommandExecuteMethod()
        {
            var source = default(string);
            switch (Code)
            {
                case ModbusCode.ReadCoilStatus:
                    source = nameof(Views.ReadCoilStatusWriteDialog);
                    break;
                case ModbusCode.ReadHoldingRegister:
                    source = nameof(Views.ReadHoldingRegisterWriteDialog);
                    break;
                default:
                    MessageBox.Show("当前数据是只读的", "系统提示", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
            }
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusDataItemViewModel), this);
            var ioc = Application.Current.PrismIoc();
            var service = ioc.DialogService;
            service.ShowDialog(source, parameters, null);
        }
    }

    public class ModbusWriteEventArgs : EventArgs
    {
        public ModbusWriteEventArgs(ModbusCode writeCode, int writeIndex, ValueType writeValue)
        {
            WriteCode = writeCode;
            WriteIndex = writeIndex;
            WriteValue = writeValue;
        }

        public ModbusCode WriteCode { get; }
        public int WriteIndex { get; }
        public ValueType WriteValue { get; }
    }
}