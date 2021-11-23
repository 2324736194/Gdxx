using System;
using System.Linq;
using ModbusDemo.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace ModbusDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation(this);
            containerRegistry.RegisterDialogWindow<DialogWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            var s = 2;
            var result =  Convert.ToString(s, 2).PadLeft(16, '0');
            var result1 = Convert.ToInt16(result, 2);
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
            e.Handled = true;
        }
    }
}
