using System;
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
    public class MainViewModel : NavigationAwareViewModel,IDisposable
    {
        private readonly ModbusService service;
        private string iPAddress;
        private int port;
        private bool connected;
        private readonly string jsonPath;
        private readonly DispatcherTimer jsonTimer;
        private readonly ModbusSet set;

        public ObservableCollection<ModbusData> DataCollection { get; } 

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
        
        public MainViewModel()
        {
            jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "data.json");
            service = new ModbusService();
            service.ModbusDataChanged += Service_ModbusDataChanged;
            set = GetModbusSet(jsonPath);
            IPAddress = set.IPAddress;
            Port = set.Port;
            DataCollection = set.DataList.ToObservableCollection();
            service.ModbusConnectedChanged += Service_ModbusConnectedChanged;
            ConnectCommand = new DelegateCommand(ConnectCommandExecuteMethod);
            LisentingCommand = new DelegateCommand(LisentingCommandExecuteMethod);
            AddCommand = new DelegateCommand(AddCommandExecuteMethod);
            EditCommand = new DelegateCommand<ModbusData>(EditCommandExecuteMethod);
            RemoveCommand = new DelegateCommand<ModbusData>(RemoveCommandExecuteMethod);
            // 最后启动
            jsonTimer = new DispatcherTimer();
            jsonTimer.Interval = TimeSpan.FromSeconds(3);
            jsonTimer.Tick += JsonTimer_Tick;
            jsonTimer.Start();
            // 订阅事件
            OnSubscribe();
        }

        private void OnSubscribe()
        {
            var ioc = Application.Current.PrismIoc();
            var aggregator = ioc.ContainerProvider.Resolve<IEventAggregator>();
            aggregator.GetEvent<PubSubEvent<ModbusWriteEventArgs>>().Subscribe(Action);
        }

        private void Action(ModbusWriteEventArgs args)
        {
            if (service.IsConnected)
            {
                switch (args.WriteCode)
                {
                    case ModbusCode.ReadCoilStatus:
                        service.Write(args.WriteIndex, (bool) args.WriteValue);
                        break;
                    case ModbusCode.ReadHoldingRegister:
                        service.Write(args.WriteIndex, (int)args.WriteValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void EditCommandExecuteMethod(ModbusData data)
        {
            var codeCollection = DataCollection.Select(p => p.Code).ToList();
            codeCollection.Remove(data.Code);
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusDataDialogViewModel), 1);
            parameters.Add(nameof(ModbusDataDialogViewModel.CodeCollection), codeCollection);
            parameters.Add(nameof(ModbusData), data);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(ModbusDataDialog), parameters, ModbusDataDialogCallback);
        }

        private void RemoveCommandExecuteMethod(ModbusData data)
        {
            var result = MessageBox.Show($"是否要移除 {data.Code} 配置？", "操作提示", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                DataCollection.Remove(data);
                Save();
            }
        }

        private void JsonTimer_Tick(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            set.IPAddress = IPAddress;
            set.Port = Port;
            set.DataList = DataCollection.ToList();
            var json = JsonConvert.SerializeObject(set);
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
            var codeCollection = DataCollection.Select(p => p.Code).ToList();
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusDataDialogViewModel), 0);
            parameters.Add(nameof(ModbusDataDialogViewModel.CodeCollection), codeCollection);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(Views.ModbusDataDialog),parameters,ModbusDataDialogCallback);
        }

        private void ModbusDataDialogCallback(IDialogResult dialogResult)   
        {
            if (dialogResult.Result == ButtonResult.OK)
            {
                var data = dialogResult.Parameters.GetValue<ModbusData>(nameof(ModbusData));
                var item = DataCollection.FirstOrDefault(p => p.Code == data.Code);
                if (null == item)
                {
                    DataCollection.Add(data);
                }
                else
                {
                    var index = DataCollection.IndexOf(item);
                    DataCollection[index] = data;
                }
                Save();
            }
        }

        private void LisentingCommandExecuteMethod()
        {
            if (!DataCollection.Any())
            {
                MessageBox.Show("请配置监听的 Modbus 数据");
                return;
            }

            if (!service.IsConnected)
            {
                MessageBox.Show("请连接 Modbus");
                return;
            }
            var ioc = Application.Current.PrismIoc();
            var manager = ioc.RegionManager;
            manager.RequestNavigate(MainWindow.ContentRegion, nameof(LisentingView));
            service.Lisenting(DataCollection.ToList());
        }
                
        private void Service_ModbusDataChanged(object sender, ModbusDataChangedEventArgs e)
        {            
            var ioc = Application.Current.PrismIoc();
            var containerProvider = ioc.ContainerProvider;
            var aggregator = containerProvider.Resolve<IEventAggregator>();
            var pubSubEvent= aggregator.GetEvent<PubSubEvent<ModbusDataChangedEventArgs>>();
            pubSubEvent.Publish(e);
        }

        private void Service_ModbusConnectedChanged(object sender, ModbusConnectedChangedEventArgs e)
        {
            connected = e.IsConnected;
            MessageBox.Show(connected ? "连接成功" : "连接失败");
        }

        private void ConnectCommandExecuteMethod()
        {
            if (connected) return;
            connected = true;
            service.IPAddress = IPAddress;
            service.Port = Port;
            service.Connect();
        }

        public void Dispose()
        {
            jsonTimer.Stop();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var form = navigationContext.NavigationService.Journal.CurrentEntry.Uri.ToString();
            switch (form)
            {
                case nameof(Views.LisentingView):
                    service.Lisented();
                    break;
            }
        }
    }
}