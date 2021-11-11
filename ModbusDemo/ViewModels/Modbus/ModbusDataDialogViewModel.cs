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
        private ModbusCodeDictionary dictionary;
        private ModbusCode code;
        private int start;
        private int quantity;
        private IModbusData data;
        private int operation;
        private string name;

        public string Name
        {
            get => name;
            set => this.SetValue(ref name, value);
        }

        public IModbusData Data
        {
            get => data;
            set => this.SetValue(ref data, value);
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

        public ICommand OkCommand { get; }
        
        public ModbusDataDialogViewModel()
        {
            OkCommand = new DelegateCommand(OkCommandExecuteMethod);
        }

        private void OkCommandExecuteMethod()
        {
            if (dictionary.Any(p => p.Key.DataAddress == Data.DataAddress && p.Key != Data))
            {
                MessageBox.Show("地址已占用，请重新输入数据地址");
                return;
            }

            if (Data.DataAddress < Start)
            {
                MessageBox.Show($"数据地址不能小于起始地址 {Start}");
                return;
            }

            if (Data.DataAddress > Start + Quantity)
            {
                MessageBox.Show($"数据地址不能大于 {Start + Quantity}");
                return;
            }

            if (string.IsNullOrEmpty(Name))
            {
                MessageBox.Show($"请输入数据名称");
                return;
            }

            if (Data is ModbusData data)
            {
                data.Name = Name;
            }

            switch (operation)
            {
                case 0:
                    dictionary.Add(Data, null);
                    break;
            }
            var parameters = new DialogParameters();
            parameters.Add(nameof(IModbusCodeSet), dictionary);
            parameters.Add(nameof(IModbusData), Data);
            OnRequestClose(new DialogResult(ButtonResult.OK, parameters));
        }

        public override void OnDialogOpened(IDialogParameters parameters)
        {
            base.OnDialogOpened(parameters);
            operation = parameters.GetValue<int>(GetType().Name);
            dictionary = parameters.GetValue<ModbusCodeDictionary>(nameof(IModbusCodeSet));
            Data = parameters.GetValue<IModbusData>(nameof(IModbusData));
            Code = dictionary.Code;
            Start = dictionary.Start;
            Quantity = dictionary.Quantity;
            switch (operation)
            {
                case 1:
                    Name = Data.Name;
                    break;
            }
        }
    }
}