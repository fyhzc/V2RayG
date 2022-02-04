﻿using ProxySetter.Resources.Langs;
using System.Drawing;
using System.Windows.Forms;

namespace ProxySetter
{
    public class ProxySetter : Apis.BaseClasses.Plugin
    {
        Services.PsLuncher luncher;
        public ProxySetter() { }

        #region private methods

        #endregion

        #region properties
        ToolStripMenuItem menuItemCache = null;
        public override ToolStripMenuItem GetMenu()
        {
            if (menuItemCache != null)
            {
                return menuItemCache;
            }

            Apis.Misc.UI.Invoke(() =>
            {
                menuItemCache = new ToolStripMenuItem(Name, Icon);
                menuItemCache.ToolTipText = Description;

                var children = menuItemCache.DropDownItems;
                children.Add(new ToolStripMenuItem(
                    I18N.Options,
                    Properties.Resources.WebConfiguration_16x,
                    (s, a) => Show()));
                children.Add(new ToolStripSeparator());
                children.AddRange(luncher?.GetSubMenu());
            });

            return menuItemCache;
        }

        public override string Name => Properties.Resources.Name;
        public override string Version => Properties.Resources.Version;
        public override string Description => I18N.Description;

        public override Image Icon => Properties.Resources.VBDynamicWeb_16x;
        #endregion

        #region protected overrides
        protected override void Start(Apis.Interfaces.Services.IApiService api)
        {
            luncher = new Services.PsLuncher();
            luncher.Run(api);
            menuItemCache = null;
        }

        protected override void Popup()
        {
            Apis.Misc.UI.Invoke(() => luncher?.Show());
        }

        protected override void Stop()
        {
            luncher?.Cleanup();
        }
        #endregion
    }
}
