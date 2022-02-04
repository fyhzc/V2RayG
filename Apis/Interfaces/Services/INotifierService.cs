﻿using System;

namespace Apis.Interfaces.Services
{
    public interface INotifierService
    {
        #region winforms
        void ShowFormJsonEditor(string config);
        void ShowFormServerSettings(ICoreServCtrl coreServ);
        void ShowFormOption();
        void ShowFormMain();
        void ShowFormLog();
        #endregion

        string RegisterHotKey(Action hotKeyHandler,
            string keyName, bool hasAlt, bool hasCtrl, bool hasShift);

        bool UnregisterHotKey(string hotKeyHandle);

        void RefreshNotifyIconLater();

        void Invoke(Action updater);

        void InvokeThen(Action updater, Action next);

    }
}
