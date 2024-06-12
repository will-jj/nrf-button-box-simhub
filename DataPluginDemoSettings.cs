namespace User.PluginSdkDemo
{
    /// <summary>
    /// Settings class, make sure it can be correctly serialized using JSON.net
    /// </summary>
    public class DataPluginDemoSettings
    {
        public ushort VID { get; set; } = 0x1915;
        public ushort PID { get; set; } = 0xEEEF;
        public bool LEDEnabled { get; set; } = true;
    }
}