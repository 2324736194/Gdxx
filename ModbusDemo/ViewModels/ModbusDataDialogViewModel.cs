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
        private int start;
        private int quantity;
        private ModbusCode selectedCode;

        public ModbusCode SelectedCode
        {
            get => selectedCode;
            set => this.SetValue(ref selectedCode, value);
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
        
        public ICommand OkCommand { get; }

        public ObservableCollection<ModbusCode> CodeCollection
        {
            get => codeCollection;
            set => this.SetValue(ref codeCollection, value);
        }

        public ModbusDataDialogViewModel()
        {
            OkCommand = new DelegateCommand(OkCommandExecuteMethod);
        }

        private void OkCommandExecuteMethod()
        {
            var list = new List<int>();
            for (int i = Start; i < Quantity; i++)
            {
                list.Add(i);
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
                    Start =  data.Start;
                    Quantity =  data.Quantity;                  
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}