using Luna.Services;
using System;
using GlobalApis = global::Apis;

namespace Luna.Models.Apis
{
    internal class LuaApis :
        GlobalApis.BaseClasses.ComponentOf<LuaApis>
    {
        // this must be static!
        static SysCmpos.PostOffice postOffice = new SysCmpos.PostOffice();

        Settings settings;
        GlobalApis.Interfaces.Services.IApiService vgcApi;
        private readonly FormMgrSvc formMgr;
        Action<string> redirectLogWorker;

        public LuaApis(
            GlobalApis.Interfaces.Services.IApiService api,
            Settings settings,
            FormMgrSvc formMgr)
        {
            this.settings = settings;
            redirectLogWorker = settings.SendLog;

            vgcApi = api;
            this.formMgr = formMgr;
        }

        #region public methods
        public GlobalApis.Interfaces.Services.IServersService GetVgcServerService() =>
            vgcApi.GetServersService();

        public SysCmpos.PostOffice GetPostOffice() => postOffice;

        public string RegisterHotKey(Action hotKeyHandler,
              string keyName, bool hasAlt, bool hasCtrl, bool hasShift)
        {
            var vgcNotifier = vgcApi.GetNotifierService();
            return vgcNotifier.RegisterHotKey(hotKeyHandler, keyName, hasAlt, hasCtrl, hasShift);
        }

        public bool UnregisterHotKey(string hotKeyHandle)
        {
            var vgcNotifier = vgcApi.GetNotifierService();
            return vgcNotifier.UnregisterHotKey(hotKeyHandle);
        }

        public override void Prepare()
        {
            var misc = new Components.Misc(vgcApi, settings, formMgr);
            var web = new Components.Web(vgcApi);
            var server = new Components.Server(vgcApi);

            AddChild(misc);
            AddChild(web);
            AddChild(server);
        }

        #endregion

        #region public methods
        public void SendLog(string message) =>
            redirectLogWorker?.Invoke(message);

        public void SetRedirectLogWorker(Action<string> worker)
        {
            if (worker != null)
            {
                redirectLogWorker = worker;
            }
        }
        #endregion

        #region private methods
        #endregion
    }
}
