using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Gdxx.Modbus;
using ImTools;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class ModbusLisentingViewModel : NavigationAwareViewModel
    {
        private IModbusDataLisenting lisenting;
        private IRegionNavigationJournal journal;
        private int codeIndex;

        public int CodeIndex
        {
            get => codeIndex;
            set => this.SetValue(ref codeIndex, value);
        }

        public ObservableDictionary<ModbusCode, List<ModbusDataItemViewModel>> Dictionary { get; }
        
        public ICommand StopCommand { get; }

        public ModbusLisentingViewModel()
        {
            Dictionary = new ObservableDictionary<ModbusCode, List<ModbusDataItemViewModel>>();
            StopCommand = new DelegateCommand(StopCommandExecuteMethod);
        }
            
        private void StopCommandExecuteMethod()
        {
            lisenting.DataChanged -= OnDataChanged;
            lisenting.Stop();
            journal.GoBack();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            journal = navigationContext.NavigationService.Journal;
            var parameters = navigationContext.Parameters;
            var collection = parameters.GetValue<IEnumerable<IModbusCodeSet>>(nameof(MainViewModel.CodeCollection));
            var codeSetSource = collection.OfType<ModbusCodeDictionary>().ToList();
            var dataList = new List<IModbusData>();
            Dictionary.Clear();
            foreach (var codeSet in codeSetSource)
            {
                var list = new List<ModbusDataItemViewModel>();
                var keyList = codeSet.Keys.ToList();
                foreach (var key in keyList)
                {
                    list.Add(new ModbusDataItemViewModel(key));
                }

                list = list.OrderBy(p => p.Data.DataAddress).ToList();
                dataList.AddRange(keyList);
                Dictionary.Add(codeSet.Code, list);
            }
            CodeIndex = 0;
            lisenting = parameters.GetValue<IModbusDataLisenting>(nameof(IModbusDataLisenting));
            lisenting.DataChanged += OnDataChanged;
            lisenting.Start(codeSetSource, dataList);
        }

        private void OnDataChanged(object sender, ModbusDataChangedEventArgs args)
        {
            foreach (var pair in args)
            {
                var data = pair.Key;
                var code = data.Code;
                if (!Dictionary.ContainsKey(code))
                {
                    Dictionary[code] = new List<ModbusDataItemViewModel>();
                }
                var list = Dictionary[code];
                var item = list.FirstOrDefault(p => p.Data == data);
                if (null == item)
                {
                    item = new ModbusDataItemViewModel()
                    {
                        Data = data
                    };
                    list.Add(item);
                }

                item.Value = pair.Value;
            }
        }
    }
}