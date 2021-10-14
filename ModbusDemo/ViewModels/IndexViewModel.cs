using System.ComponentModel;
using System.Windows.Input;

namespace ModbusDemo.ViewModels
{
    public class IndexViewModel:ViewModel
    {
        private string index;

        public string Index
        {
            get => index;
            set => this.SetValue(ref index, value);
        }

        public ICommand RemoveCommand { get; set; }
    }
}