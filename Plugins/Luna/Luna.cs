﻿using Luna.Resources.Langs;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Luna
{
    // Using lunar not lua to void naming conflicts.
    public class Luna : Apis.BaseClasses.Plugin
    {
        Services.Settings settings;
        Services.LuaServer luaServer;
        Services.FormMgrSvc formMgr;
        Services.MenuUpdater menuUpdater;

        readonly ToolStripMenuItem miRoot, miShowMgr, miShowEditor;
        public Luna()
        {
            ToolStripMenuItem mr = null, msw = null, mse = null;
            Apis.Misc.UI.Invoke(() =>
            {
                mr = new ToolStripMenuItem(this.Name, this.Icon);

                msw = new ToolStripMenuItem(
                    I18N.OpenScriptManger,
                    Properties.Resources.StoredProcedureScript_16x,
                    (s, a) => Show());

                mse = new ToolStripMenuItem(
                    I18N.OpenScriptEditor,
                    Properties.Resources.EditWindow_16x,
                    (s, a) => formMgr?.ShowOrCreateFirstEditor());

            });

            miRoot = mr;
            miShowMgr = msw;
            miShowEditor = mse;
        }

        #region properties
        public override string Name => Properties.Resources.Name;
        public override string Version => Properties.Resources.Version;
        public override string Description => I18N.Description;

        // icon from http://lua-users.org/wiki/LuaLogo
        public override Image Icon => Properties.Resources.Lua_Logo_32x32;
        #endregion

        #region public overrides
        public override ToolStripMenuItem GetMenu() => miRoot;

        #endregion

        #region protected overrides
        protected override void Popup()
        {
            // formMgr.ShowOrCreateFirstEditor();
            formMgr.ShowFormMain();
        }

        protected override void Start(Apis.Interfaces.Services.IApiService api)
        {
            var vgcSettings = api.GetSettingService();

            settings = new Services.Settings();
            luaServer = new Services.LuaServer();
            formMgr = new Services.FormMgrSvc();
            menuUpdater = new Services.MenuUpdater(settings);

            settings.Run(vgcSettings);
            luaServer.Run(api, settings, formMgr);
            formMgr.Run(settings, luaServer, api);
            menuUpdater.Run(luaServer, miRoot, miShowMgr, miShowEditor);

            luaServer.WakeUpAutoRunScripts(TimeSpan.FromSeconds(2));
        }

        protected override void Stop()
        {
            Apis.Libs.Sys.FileLogger.Info("Luna.Cleanup() begin");
            settings?.SetIsDisposing(true);
            menuUpdater?.Dispose();
            formMgr?.Dispose();
            luaServer?.Dispose();
            settings?.Dispose();
            Apis.Libs.Sys.FileLogger.Info("Luna.Cleanup() end");
        }
        #endregion
    }
}
