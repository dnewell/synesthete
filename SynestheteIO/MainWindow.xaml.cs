using System.Windows;
using MahApps.Metro.Controls;

namespace Syne.IO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static Audio.DeviceEnumerator _deviceEnumerator;

        public MainWindow()
        {
            _deviceEnumerator = new Audio.DeviceEnumerator();
            InitializeComponent();
        }

        private void Button_Click_Enumerate_Devices(object sender, RoutedEventArgs e)
        {

        }
    }
}
