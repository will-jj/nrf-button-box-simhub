using SimHub.Plugins.Styles;
using System.Windows.Controls;

namespace User.PluginSdkDemo
{
    /// <summary>
    /// Logique d'interaction pour SettingsControlDemo.xaml
    /// </summary>
    public partial class SettingsControlDemo : UserControl
    {
        public DataPluginDemo Plugin { get; }

        public SettingsControlDemo()
        {
            InitializeComponent();

            
        }

        public SettingsControlDemo(DataPluginDemo plugin) : this()
        {
            Plugin = plugin;
            DataContext = plugin;
        }

        private void SHButtonPrimary_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Plugin.Connect();
        }
    }
}
