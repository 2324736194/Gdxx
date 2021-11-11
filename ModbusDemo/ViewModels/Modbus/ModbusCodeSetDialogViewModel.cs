using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Gdxx.Modbus;
using Prism.Commands;
using Prism.Services.Dialogs;
using Prism.ViewModels;

namespace ModbusDemo.ViewModels
{
    public class ModbusCodeSetDialogViewModel : DialogAwareViewModel
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

        public ObservableCollection<ModbusCode> CodeCollection
        {
            get => codeCollection;
            set => this.SetValue(ref codeCollection, value);
        }

        public ICommand OkCommand { get; }

        public ModbusCodeSetDialogViewModel()
        {
            OkCommand = new DelegateCommand(OkCommandExecuteMethod);
        }

        private void OkCommandExecuteMethod()
        {
            if (!Check())
            {
                return;
            }

            var codeSet = new ModbusCodeSet()
            {
                Code = SelectedCode,
                Start = Start,
                Quantity = Quantity,
            };
            var parameters = new DialogParameters();
            parameters.Add(nameof(IModbusCodeSet), codeSet);
            OnRequestClose(new DialogResult(ButtonResult.OK, parameters));
        }

        private bool Check()
        {
            var result = true;
            var error = string.Empty;
            if (Quantity <= 0)
            {
                error = "数据量必须大于 0";
                this.RaiseErrorsChanged(p => p.Quantity, error);
                result = false;
            }
            if (Start < 0)
            {
                error = "起始地址必须大于或等于 0";
                this.RaiseErrorsChanged(p => p.Start, error);
                result = false;
            }
            return result;
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            CodeCollection = Enum.GetValues(typeof(ModbusCode)).OfType<ModbusCode>().ToObservableCollection();
            var index = parameters.GetValue<int>(GetType().Name);
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
                    var codeSet = parameters.GetValue<IModbusCodeSet>(nameof(IModbusCodeSet));
                    SelectedCode = codeSet.Code;
                    Start = codeSet.Start;
                    Quantity = codeSet.Quantity;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}