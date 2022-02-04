﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using GlobalApis = global::Apis;

namespace Luna.Models.Apis.Components
{
    internal sealed class Web :
        GlobalApis.BaseClasses.ComponentOf<LuaApis>,
        GlobalApis.Interfaces.Lua.ILuaWeb
    {
        GlobalApis.Interfaces.Services.IWebService vgcWeb;
        GlobalApis.Interfaces.Services.IServersService vgcServers;
        GlobalApis.Interfaces.Services.IShareLinkMgrService vgcSlinkMgr;

        public Web(GlobalApis.Interfaces.Services.IApiService api)
        {
            vgcWeb = api.GetWebService();
            vgcServers = api.GetServersService();
            vgcSlinkMgr = api.GetShareLinkMgrService();
        }

        #region ILuaWeb thinggy
        public string Post(string url, string text) => Post(url, text, 20000);

        public string Post(string url, string text, int timeout)
        {
            timeout = Math.Max(1, timeout);

            try
            {
                var t = Task.Run(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        var token = new CancellationTokenSource(timeout).Token;
                        var content = new StringContent(text);
                        var resp = await client.PostAsync(url, content, token);
                        return await resp.Content.ReadAsStringAsync();
                    }
                });

                return t.GetAwaiter().GetResult();
            }
            catch { }
            return null;
        }

        public bool Tcping(string url, int milSec) =>
            Tcping(url, milSec, -1);

        public bool Tcping(string url, int milSec, int proxyPort)
        {
            var timeout = TimedDownloadTesting(url, milSec, 0, proxyPort);
            return timeout > 0 && timeout <= milSec;
        }

        public long TimedDownloadTesting(string url, int timeout, int kib) =>
            TimedDownloadTesting(url, timeout, kib, -1);

        public long TimedDownloadTesting(string url, int timeout, int kib, int proxyPort)
        {
            try
            {
                var r = GlobalApis.Misc.Utils.TimedDownloadTest(url, proxyPort, kib, timeout);
                return r.Item1;
            }
            catch { }
            return -1;
        }

        public List<string> ExtractBase64String(string text, int minLen) =>
            GlobalApis.Misc.Utils.ExtractBase64Strings(text, minLen);

        public List<string> ExtractBase64String(string text) =>
            ExtractBase64String(text, 1);

        public int GetProxyPort() =>
            vgcServers.GetAvailableHttpProxyPort();

        public bool Download(string url, string filename) =>
            vgcWeb.Download(url, filename, -1, -1);

        public bool Download(string url, string filename, int millSecond) =>
            vgcWeb.Download(url, filename, -1, millSecond);

        public bool Download(string url, string filename, int proxyPort, int millSecond) =>
            vgcWeb.Download(url, filename, proxyPort, millSecond);

        public string Fetch(string url) => vgcWeb.Fetch(url, -1, -1);

        public string Fetch(string url, int milliSeconds) =>
            vgcWeb.Fetch(url, -1, milliSeconds);

        public string Fetch(string url, int proxyPort, int milliSeconds) =>
            vgcWeb.Fetch(url, proxyPort, milliSeconds);

        public int UpdateSubscriptions() =>
            vgcSlinkMgr.UpdateSubscriptions(-1);

        public int UpdateSubscriptions(int proxyPort) =>
            vgcSlinkMgr.UpdateSubscriptions(proxyPort);

        public List<string> ExtractV2cfgLinks(string text) =>
            vgcWeb.ExtractLinks(
                text, GlobalApis.Models.Datas.Enums.LinkTypes.v2cfg);

        public List<string> ExtractVmessLinks(string text) =>
            vgcWeb.ExtractLinks(
                text, GlobalApis.Models.Datas.Enums.LinkTypes.vmess);

        public List<string> ExtractSsLinks(string text) =>
            vgcWeb.ExtractLinks(
                text, GlobalApis.Models.Datas.Enums.LinkTypes.ss);

        public string Search(string keywords, int first, int proxyPort) =>
            vgcWeb.Search(keywords, first, proxyPort, 20 * 1000);

        public string PatchHref(string url, string href) =>
            vgcWeb.PatchHref(url, href);



        #endregion
    }
}
