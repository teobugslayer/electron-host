using System.Windows;

namespace electron_host
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.Content = new ElectronWindow(this);
        }
    }
}
