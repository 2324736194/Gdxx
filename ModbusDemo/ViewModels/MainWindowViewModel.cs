using System.Windows;
using System.Windows.Input;
using ModbusDemo.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;

namespace ModbusDemo.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism Application";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public ICommand MainCommand
        {
            get;
        }

        public MainWindowViewModel()
        {
            MainCommand = new DelegateCommand(MainCommandExecuteMethod);
        }

        private void MainCommandExecuteMethod()
        {
            var ioc = Application.Current.PrismIoc();
            var manager = ioc.RegionManager;
            manager.RequestNavigate(MainWindow.ContentRegion, nameof(MainView));
        }
    }
}
