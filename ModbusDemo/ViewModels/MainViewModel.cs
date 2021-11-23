using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Gdxx.Modbus;
using ModbusDemo.Views;
using Newtonsoft.Json;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class MainViewModel : NavigationAwareViewModel
    {
        private readonly ModbusPoll poll;
        private string iPAddress;
        private int port;
        private readonly string jsonPath;
        private readonly ModbusSet modbusSet;

        public ObservableCollection<IModbusCodeSet> CodeCollection { get; }

        public int Port
        {
            get => port;
            set => this.SetValue(ref port, value);
        }

        public string IPAddress
        {
            get => iPAddress;
            set => this.SetValue(ref iPAddress, value);
        }

        public ICommand ConnectCommand { get; }

        public ICommand LisentingCommand { get; }

        public ICommand AddCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand RemoveCommand { get; }

        public ICommand SetCommand { get; }

        public MainViewModel()
        {
            jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data.json");
            modbusSet = GetModbusSet(jsonPath);
            //
            poll = new ModbusPoll();
            poll.Connected += Poll_Connected;
            poll.Disconnected += Poll_Disconnected;
            //
            IPAddress = modbusSet.IPAddress;
            Port = modbusSet.Port;
            CodeCollection = new ObservableCollection<IModbusCodeSet>();
            //
            foreach (var codeSet in modbusSet.CodeSetList)
            {
                var item = new ModbusCodeDictionary(codeSet);
                modbusSet.BooleanList?.ForEach(p =>
                {
                    if (p.Code != item.Code) return;
                    item.Add(p, null);
                });
                modbusSet.SingleList?.ForEach(p =>
                {
                    if (p.Code != item.Code) return;
                    item.Add(p, null);
                });
                modbusSet.Int32List?.ForEach(p =>
                {
                    if (p.Code != item.Code) return;
                    item.Add(p, null);
                });
                CodeCollection.Add(item);
            }
            //
            ConnectCommand = new DelegateCommand(ConnectCommandExecuteMethod);
            LisentingCommand = new DelegateCommand(LisentingCommandExecuteMethod);
            AddCommand = new DelegateCommand(AddCommandExecuteMethod);
            EditCommand = new DelegateCommand<IModbusCodeSet>(EditCommandExecuteMethod);
            SetCommand = new DelegateCommand<IModbusCodeSet>(SetCommandExecuteMethod);
            RemoveCommand = new DelegateCommand<IModbusCodeSet>(RemoveCommandExecuteMethod);
            // 订阅事件
            OnSubscribe();
        }

        private void SetCommandExecuteMethod(IModbusCodeSet codeSet)
        {
            var parameters = new NavigationParameters();
            parameters.Add(nameof(IModbusCodeSet), codeSet);
            var ioc = Application.Current.PrismIoc();
            var manager = ioc.RegionManager;
            manager.RequestNavigate(MainWindow.ContentRegion, nameof(ModbusDataListView), parameters);
        }
        
        private void Poll_Disconnected(object sender, EventArgs e)
        {
            MessageBox.Show("已断开连接");
        }

        private void Poll_Connected(object sender, EventArgs e)
        {
            MessageBox.Show("连接成功");
        }

        private void OnSubscribe()
        {
            var ioc = Application.Current.PrismIoc();
            var aggregator = ioc.ContainerProvider.Resolve<IEventAggregator>();
            aggregator.GetEvent<PubSubEvent<ModbusDataWriteEventArgs>>().Subscribe(Action);
        }

        private void Action(ModbusDataWriteEventArgs args)
        {
            if (poll.IsConnected)
            {
                poll.Write(args);
            }
        }

        private void EditCommandExecuteMethod(IModbusCodeSet codeSet)
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusCodeSetDialogViewModel), 1);
            parameters.Add(nameof(IModbusCodeSet), codeSet);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(ModbusCodeSetDialog), parameters, ModbusCodeSetDialogCallback);
        }

        private void RemoveCommandExecuteMethod(IModbusCodeSet codeSet)
        {
            var result = MessageBox.Show($"是否要移除 {codeSet.Code} 配置？", "操作提示", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                CodeCollection.Remove(codeSet);
                Save();
            }
        }
        
        private void Save()
        {
            modbusSet.IPAddress = IPAddress;
            modbusSet.Port = Port;
            modbusSet.CodeSetList = CodeCollection.Select(p => new CodeSet()
            {
                Code = p.Code,
                Start = p.Start,
                Quantity = p.Quantity
            }).ToList();
            modbusSet.BooleanList = new List<ModbusBoolean>();
            modbusSet.SingleList = new List<ModbusSingle>();
            modbusSet.Int32List = new List<ModbusInt32>();
            foreach (var item in CodeCollection.OfType<ModbusCodeDictionary>())
            {
                foreach (var key in item.Keys)
                {
                    switch (key)
                    {
                        case ModbusBoolean boolean:
                            modbusSet.BooleanList.Add(boolean);
                            break;
                        case ModbusSingle single:
                            modbusSet.SingleList.Add(single);
                            break;
                        case ModbusInt32 int32:
                            modbusSet.Int32List.Add(int32);
                            break;
                    }
                }
            }

            var json = JsonConvert.SerializeObject(modbusSet);
            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            var file = File.Open(jsonPath, FileMode.Create);
            using (file)
            {
                file.Write(buffer, 0, buffer.Length);
            }
        }

        private ModbusSet GetModbusSet(string path)
        {
            if (File.Exists(path))
            {
                var reader = File.OpenText(path);
                using (reader)
                {
                    var json = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<ModbusSet>(json);
                }
            }

            return new ModbusSet();
        }

        private void AddCommandExecuteMethod()
        {
            var codeCollection = CodeCollection.Select(p => p.Code).ToList();
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusCodeSetDialogViewModel), 0);
            parameters.Add(nameof(ModbusCodeSetDialogViewModel.CodeCollection), codeCollection);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(ModbusCodeSetDialog), parameters, ModbusCodeSetDialogCallback);
        }

        private void ModbusCodeSetDialogCallback(IDialogResult dialogResult)
        {
            if (dialogResult.Result == ButtonResult.OK)
            {
                var set = dialogResult.Parameters.GetValue<IModbusCodeSet>(nameof(IModbusCodeSet));
                switch (set)
                {
                    // 新增
                    case ModbusCodeSet codeSet:
                        CodeCollection.Add(new ModbusCodeDictionary(codeSet));
                        break;
                    // 编辑
                    case ModbusCodeDictionary codeDictionary:
                        var index = CodeCollection.IndexOf(codeDictionary);
                        CodeCollection.RemoveAt(index);
                        CodeCollection.Insert(index, codeDictionary);
                        break;
                }

                Save();
            }
        }

        private void LisentingCommandExecuteMethod()
        {
            if (!CodeCollection.Any())
            {
                MessageBox.Show("请配置监听的 Modbus 数据");
                return;
            }

            if (!poll.IsConnected)
            {
                MessageBox.Show("请连接 Modbus");
                return;
            }

            var ioc = Application.Current.PrismIoc();
            var parameters = new NavigationParameters();
            parameters.Add(nameof(IModbusDataLisenting), poll.Lisenting);
            parameters.Add(nameof(CodeCollection), CodeCollection);
            var manager = ioc.RegionManager;
            manager.RequestNavigate(MainWindow.ContentRegion, nameof(ModbusLisentingView), parameters);
        }
        
        private void ConnectCommandExecuteMethod()
        {
            if (poll.IsConnected) return;
            poll.IPAddress = IPAddress;
            poll.Port = Port;
            poll.Connect();
        }
        
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var form = navigationContext.NavigationService.Journal.CurrentEntry.Uri.ToString();
            switch (form)
            {
                case nameof(ModbusLisentingView):
                    //poll.Lisented();
                    break;
                case nameof(ModbusDataListView):
                    ModbusDataListViewHandler(navigationContext.NavigationService.Journal.CurrentEntry.Parameters);
                    break;
            }
        }

        private void ModbusDataListViewHandler(NavigationParameters parameters)
        {
            var codeDictionary = parameters.GetValue<ModbusCodeDictionary>(nameof(IModbusCodeSet));
            var index = CodeCollection.IndexOf(codeDictionary);
            CodeCollection.RemoveAt(index);
            CodeCollection.Insert(index, codeDictionary);
            Save();
        }
    }
}