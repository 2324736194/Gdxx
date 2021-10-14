using System.Windows;

namespace ModbusDemo.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public const string ContentRegion = nameof(MainWindow) + "_" + nameof(ContentRegion);

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
