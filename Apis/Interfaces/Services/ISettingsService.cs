namespace Apis.Interfaces.Services
{
    public interface ISettingsService
    {
        int GetSpeedtestQueueLength();

        bool IsScreenLocked();

        bool IsClosing();

        void SendLog(string log);
        void SavePluginsSetting(string pluginName, string value);
        string GetPluginsSetting(string pluginName);

        // for luna plug-in
        string GetSubscriptionConfig();
        void SetSubscriptionConfig(string cfgStr);
    }
}
