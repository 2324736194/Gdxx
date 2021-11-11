using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Gdxx.Modbus;
using ModbusDemo.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class ModbusDataListViewModel : NavigationAwareViewModel
    {
        private IRegionNavigationJournal journal;
        private ModbusCodeDictionary dictionary;
        private ModbusCode code;
        private int start;
        private int quantity;
        private ObservableCollection<IModbusData> dataCollection;
        private ObservableCollection<string> categoryCollection;
        private string category;

        public string Category
        {
            get => category;
            set => this.SetValue(ref category, value);
        }

        public ObservableCollection<string> CategoryCollection
        {
            get => categoryCollection;
            set => this.SetValue(ref categoryCollection, value);
        }

        public ObservableCollection<IModbusData> DataCollection
        {
            get => dataCollection;
            set => this.SetValue(ref dataCollection, value);
        }

        public int Quantity
        {
            get => quantity;
            set => this.SetValue(ref quantity, value);
        }

        public int Start
        {
            get => start;
            set => this.SetValue(ref start, value);
        }

        public ModbusCode Code
        {
            get => code;
            set => this.SetValue(ref code, value);
        }
        
        public ICommand GoBackCommand { get; }
            
        public ICommand SelectedCategoryCommand { get; }

        public ICommand EditCommand { get; }

        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        public ModbusDataListViewModel()
        {
            GoBackCommand = new DelegateCommand(GoBackCommandExecuteMethod);
            SelectedCategoryCommand = new DelegateCommand<DataGrid>(SelectedCategoryCommandExecuteMethod);
            EditCommand = new DelegateCommand<IModbusData>(EditCommandExecuteMethod);
            AddCommand = new DelegateCommand(AddCommandExecuteMethod);
            RemoveCommand = new DelegateCommand<IModbusData>(RemoveCommandExecuteMethod);
        }

        private void AddCommandExecuteMethod()
        {
            var data = default(ModbusData);
            switch (Category)
            {
                case nameof(Boolean):
                   var  boolean = new ModbusBoolean();
                    switch (Code)
                    {
                        case ModbusCode.ReadHoldingRegister:
                        case ModbusCode.ReadInputRegister:
                            boolean.CanSplit = true;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                   data = boolean;
                    break;
                case nameof(Single):
                    data = new ModbusSingle();
                    break;
                case nameof(Int32):
                    data = new ModbusInt32();
                    break;
                default:
                    throw new NotImplementedException();
            }

            data.Code = Code;
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusDataDialogViewModel), 0);
            parameters.Add(nameof(IModbusCodeSet), dictionary);
            parameters.Add(nameof(IModbusData), data);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(ModbusDataDialog), parameters, ModbusDataDialogCallback);
        }

        private void ModbusDataDialogCallback(IDialogResult dialogResult)
        {
            if (dialogResult.Result == ButtonResult.OK)
            {
                var parameters = dialogResult.Parameters;
                var data = parameters.GetValue<IModbusData>(nameof(IModbusData));
                var index = DataCollection.IndexOf(data);
                if (index == -1)
                {
                    DataCollection.Add(data);
                }
                else
                {
                    DataCollection.Remove(data);
                    DataCollection.Insert(index, data);
                }

                DataCollection = DataCollection.OrderBy(p => p.DataAddress).ToObservableCollection();
            }
        }

        private void RemoveCommandExecuteMethod(IModbusData data)
        {
            var messageBoxText = $"是否删除代码 {Code} 中数据地址为 {data.DataAddress} 的数据";
            var caption = "系统提示";
            var button = MessageBoxButton.YesNo;
            var icon = MessageBoxImage.Question;
            var result = MessageBox.Show(messageBoxText, caption, button, icon);
            if (result == MessageBoxResult.Yes)
            {
                dictionary.Remove(data);
                DataCollection.Remove(data);
            }
        }       

        private void EditCommandExecuteMethod(IModbusData data)
        {
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusDataDialogViewModel), 1);
            parameters.Add(nameof(IModbusCodeSet), dictionary);
            parameters.Add(nameof(IModbusData), data);
            var ioc = Application.Current.PrismIoc();
            ioc.DialogService.ShowDialog(nameof(ModbusDataDialog), parameters, ModbusDataDialogCallback);
        }

        private void SelectedCategoryCommandExecuteMethod(DataGrid dataGrid)
        {
            if (null == dataGrid)
            {
                throw new ArgumentNullException(nameof(dataGrid));
            }

            var column = dataGrid.FindName(nameof(DataGridTemplateColumn)) as DataGridTemplateColumn;
            dataGrid.Columns.Clear();
            switch (Category)
            {
                case nameof(Boolean):
                    BooleanHandler(dataGrid);
                    break;
                case nameof(Single):
                    SingleHandler(dataGrid);
                    break;
                case nameof(Int32):
                    Int32Handler(dataGrid);
                    break;
                default:
                    throw new NotImplementedException();
            }

            dataGrid.Columns.Add(column);
        }

        private void Int32Handler(DataGrid dataGrid)
        {
            var column = default(DataGridColumn);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据名称",
                Binding = new Binding(nameof(IModbusData.Name)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据地址",
                Binding = new Binding(nameof(IModbusData.DataAddress)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            DataCollection = dictionary
                .Keys
                .Where(p => p is ModbusInt32)
                .OrderBy(p => p.DataAddress)
                .ToObservableCollection();
        }

        private void SingleHandler(DataGrid dataGrid)
        {
            var column = default(DataGridColumn);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据名称",
                Binding = new Binding(nameof(IModbusData.Name)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据地址",
                Binding = new Binding(nameof(IModbusData.DataAddress)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据格式",
                Binding = new Binding(nameof(ModbusSingle.SingleFormat)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            DataCollection = dictionary
                .Keys
                .Where(p => p is ModbusSingle)
                .OrderBy(p => p.DataAddress)
                .ToObservableCollection();
        }

        private void BooleanHandler(DataGrid dataGrid)
        {
            var column = default(DataGridColumn);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据名称",
                Binding = new Binding(nameof(IModbusData.Name)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "数据地址",
                Binding = new Binding(nameof(IModbusData.DataAddress)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "启用拆分",
                Binding = new Binding(nameof(ModbusBoolean.IsEnabledSplit)),
                IsReadOnly = true,
            };
            dataGrid.Columns.Add(column);
            DataGridColumnHandler(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "拆分索引",
                Binding = new Binding(nameof(ModbusBoolean.Index)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            DataGridColumnHandler(column);
            //
            column = new DataGridTextColumn()
            {
                Header = "是否位移",
                Binding = new Binding(nameof(ModbusBoolean.IsOffset)),
                IsReadOnly = true
            };
            dataGrid.Columns.Add(column);
            DataGridColumnHandler(column);
            // 
            DataCollection = dictionary
                .Keys
                .Where(p => p is ModbusBoolean)
                .OrderBy(p => p.DataAddress)
                .ToObservableCollection();
        }

        private void DataGridColumnHandler(DataGridColumn column)
        {
            switch (Code)
            {
                case ModbusCode.ReadCoilStatus:
                case ModbusCode.ReadInputStatus:
                    column.Visibility = Visibility.Collapsed;
                    break;
                case ModbusCode.ReadHoldingRegister:
                case ModbusCode.ReadInputRegister:
                    column.Visibility = Visibility.Visible;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void GoBackCommandExecuteMethod()
        {
            journal.GoBack();
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            var parameters = navigationContext.Parameters;
            journal = navigationContext.NavigationService.Journal;
            dictionary = parameters.GetValue<ModbusCodeDictionary>(nameof(IModbusCodeSet));
            Code = dictionary.Code;
            Quantity = dictionary.Quantity;
            CategoryCollection = GetCategoryList(Code).ToObservableCollection();
            Category = CategoryCollection.First();
        }

        private List<string> GetCategoryList(ModbusCode code)
        {
            var list = default(List<string>);
            switch (code)
            {   
                case ModbusCode.ReadCoilStatus:
                case ModbusCode.ReadInputStatus:
                    list = new List<string>()
                    {
                        nameof(Boolean)
                    };
                    break;
                case ModbusCode.ReadHoldingRegister:
                case ModbusCode.ReadInputRegister:
                    list =new List<string>()
                    {
                        nameof(Boolean),
                        nameof(Single),
                        nameof(Int32)
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            list.Sort((p, p1) => string.Compare(p, p1, StringComparison.Ordinal));

            return list;
        }
    }
}