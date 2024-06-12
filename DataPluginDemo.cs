using GameReaderCommon;
using SimHub.Plugins;
using SimhubHIDTest;
using System;
using System.Windows.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace User.PluginSdkDemo
{
    [PluginDescription("Controls LEDs through hidapi")]
    [PluginAuthor("Will Jacks")]
    [PluginName("HID LED Control")]
    public class DataPluginDemo : IPlugin, IDataPlugin, IWPFSettingsV2, INotifyPropertyChanged
    {

        

        public event PropertyChangedEventHandler PropertyChanged;

        private bool? _ledEnabled;

        public bool LEDEnabled
        {
            get
            {
                return (_ledEnabled ?? false);
            }
            set
            {
                _ledEnabled = value;
                OnPropertyChanged(nameof(LEDEnabled));
                Settings.LEDEnabled = value;
            }
        }

        private bool? _ledControlEnabled;

        private bool _canConnect;
        public bool CanConnect
        {
            get
            {
                return _canConnect;
            }
            set
            {
                _canConnect = value;
                OnPropertyChanged(nameof(CanConnect));
            }
        }

        public bool LEDControlEnabled
        {
            get
            {
                return (_ledControlEnabled ?? false);
            }
            set
            {
                _ledControlEnabled = value;
                OnPropertyChanged(nameof(LEDControlEnabled));
            }
        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            SimHub.Logging.Current.Info($"Invoked {name}");

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

           

        }






        private byte[] _data;

        private IntPtr _hidHandle;

        public bool _ledState;



        public DataPluginDemoSettings Settings;

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        /// <summary>
        /// Gets the left menu icon. Icon must be 24x24 and compatible with black and white display.
        /// </summary>
        public ImageSource PictureIcon => this.ToIcon(Properties.Resources.sdkmenuicon);

        /// <summary>
        /// Gets a short plugin title to show in left menu. Return null if you want to use the title as defined in PluginName attribute.
        /// </summary>
        public string LeftMenuTitle => "HID LED Control";

        /// <summary>
        /// Called one time per game data update, contains all normalized game data,
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        ///
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        ///
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data">Current game data, including current and previous data frame.</param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            // Define the value of our property (declared in init)
            if (!data.GameRunning)
            {
                return;
            }
            if (data.NewData is null)
            {
                return;
            }
            if (_hidHandle == IntPtr.Zero)
            {
                return;
            }

            if (data.NewData.CarSettings_RPMRedLineReached > 0 && LEDEnabled)
            {
                TurnLedOn();
                _ledState = true;
            }
            else
            {
                TurnLedOff();
                _ledState = false;
            }
        }

        private void TurnLedOn()
        {
            if (_ledState == false)
            {
                byte[] data = { 0x00, 0x02 };
                int ret = HidApi.hid_write(_hidHandle, data, (uint)data.Length);
                if(ret < 0)
                {
                    Disconnect();
                }
                SimHub.Logging.Current.Info($"response {ret}");

            }
        }
        private void TurnLedOff()
        {
            if (_ledState == true)
            {
                byte[] data = { 0x00, 0x00 };
                int ret = HidApi.hid_write(_hidHandle, data, (uint)data.Length);
                if (ret < 0)
                {
                    Disconnect();
                }
            }
        }

        public void Connect()
        {
            _hidHandle = HidApi.hid_open(Settings.VID, Settings.PID, null);
            if (_hidHandle == IntPtr.Zero)
            {
                LEDEnabled = false;
                LEDControlEnabled = false;
                CanConnect = true;
            }
            else
            {
                LEDControlEnabled = true;
                CanConnect = false;
            }
            SimHub.Logging.Current.Info($"HID Opened {_hidHandle}");
        }

        public void Disconnect()
        {
            _hidHandle = IntPtr.Zero;
            LEDControlEnabled = false;
            CanConnect = true;
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here !
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
            // Save settings
            this.SaveCommonSettings("GeneralSettings", Settings);
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return new SettingsControlDemo(this);
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {
            SimHub.Logging.Current.Info("Starting plugin");

            // Load settings
            Settings = this.ReadCommonSettings<DataPluginDemoSettings>("GeneralSettings", () => new DataPluginDemoSettings());
            LEDEnabled = Settings.LEDEnabled;

            // Declare a property available in the property list, this gets evaluated "on demand" (when shown or used in formulas)
            this.AttachDelegate("CurrentDateTime", () => DateTime.Now);
            this.AttachDelegate("WheelConnected", () => LEDControlEnabled);

            _ = HidApi.hid_init();
            Connect();

            SimHub.Logging.Current.Info($"HID Opened {_hidHandle}");

            

        }
    }
}