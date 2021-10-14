using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Gdxx.Modbus;
using Prism.Commands;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class ModbusDataDialogViewModel : DialogAwareViewModel
    {
        private ObservableCollection<ModbusCode> codeCollection;
        private uint start;
        private uint quantity;
        private ObservableCollection<IndexViewModel> indexList;
        private ModbusCode selectedCode;

        public ModbusCode SelectedCode
        {
            get => selectedCode;
            set => this.SetValue(ref selectedCode, value);
        }

        public ObservableCollection<IndexViewModel> IndexList
        {
            get => indexList;
            set => this.SetValue(ref indexList, value);
        }

        public uint Quantity
        {
            get => quantity;
            set => this.SetValue(ref quantity, value);
        }
        
        public uint Start
        {
            get => start;
            set => this.SetValue(ref start, value);
        }
        
        public ICommand AddCommand { get; }

        public ICommand RemoveCommand { get; }

        public ICommand OkCommand { get; }

        public ObservableCollection<ModbusCode> CodeCollection
        {
            get => codeCollection;
            set => this.SetValue(ref codeCollection, value);
        }

        public ModbusDataDialogViewModel()
        {
            IndexList = new ObservableCollection<IndexViewModel>();
            AddCommand = new DelegateCommand<ScrollViewer>(AddCommandExecuteMethod);
            RemoveCommand = new DelegateCommand<IndexViewModel>(RemoveCommandExecuteMethod);
            OkCommand = new DelegateCommand(OkCommandExecuteMethod);
        }

        private void OkCommandExecuteMethod()
        {
            var list = new List<int>();
            foreach (var item in IndexList)
            {
                if (string.IsNullOrEmpty(item.Index))
                    continue;
                if (int.TryParse(item.Index, out var result))
                {
                    list.Add(result);
                }
            }

            var data = new ModbusData()
            {
                Code = SelectedCode,
                Start = (int) Start,
                Quantity = (int) Quantity,
                IndexList = list.OrderBy(p => p).Distinct().ToList()
            };
            var parameters = new DialogParameters();
            parameters.Add(nameof(ModbusData), data);
            OnRequestClose(new DialogResult(ButtonResult.OK, parameters));
        }

        private void RemoveCommandExecuteMethod(IndexViewModel index)
        {
            IndexList.Remove(index);
        }

        private void AddCommandExecuteMethod(ScrollViewer viewer)
        {
            if (Quantity <= 0)
            {
                MessageBox.Show("请设置数据数量（Quantity）");
                return;
            }
            IndexList.Add(new IndexViewModel()
            {
                RemoveCommand = RemoveCommand
            });
            viewer.ScrollToEnd();
        }
        
        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            CodeCollection = Enum.GetValues(typeof(ModbusCode)).OfType<ModbusCode>().ToObservableCollection();
            var index = parameters.GetValue<int>(nameof(ModbusDataDialogViewModel));
            var codes = parameters.GetValue<List<ModbusCode>>(nameof(CodeCollection));
            if (null != codes)
            {
                foreach (var code in codes)
                {
                    CodeCollection.Remove(code);
                }
            }
            switch (index)
            {
                // 新增
                case 0:
                    SelectedCode = CodeCollection.First();
                    break;
                // 编辑
                case 1:
                    var data = parameters.GetValue<ModbusData>(nameof(ModbusData));
                    SelectedCode = data.Code;
                    Start = (uint) data.Start;
                    Quantity = (uint) data.Quantity;
                    IndexList = data.IndexList.Select(p =>new IndexViewModel()
                    {
                        Index = p.ToString(),
                        RemoveCommand = RemoveCommand
                    }).ToObservableCollection();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}