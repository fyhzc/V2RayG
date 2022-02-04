﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using GlobalApis = global::Apis;

namespace V2RayG.Libs.Lua.ApiComponents
{
    public class UtilsApi :
        GlobalApis.BaseClasses.Disposable,
        GlobalApis.Interfaces.Services.IUtilsService
    {
        #region json
        public bool TrySetValue<T>(JToken json, string path, T value) =>
            Misc.Utils.TrySetValue<T>(json, path, value);

        public T GetValue<T>(JToken json, string path) =>
            Misc.Utils.GetValue<T>(json, path);

        public JToken GetKey(JToken json, string path) =>
            Misc.Utils.GetKey(json, path);

        public string GetProtocol(JObject config) =>
            Misc.Utils.GetProtocolFromConfig(config);

        public string JTokenToString(JToken jtoken) =>
            jtoken.ToString();

        public void Replace(JToken node, JToken value) =>
            node.Replace(value);

        public void Union(JObject body, JObject mixin) =>
            Misc.Utils.UnionJson(body, mixin);

        public void Merge(JObject body, JObject mixin) =>
            Misc.Utils.MergeJson(body, mixin);

        public void CombineWithRoutingInTheEnd(JObject body, JObject mixin) =>
            Misc.Utils.CombineConfigWithRoutingInTheEnd(ref body, mixin);

        public void CombineWithRoutingInFront(JObject body, JObject mixin) =>
           Misc.Utils.CombineConfigWithRoutingInFront(ref body, mixin);

        public JObject ToJObject(JToken jtoken) =>
            jtoken as JObject;

        public JArray ToJArray(JToken jtoken) =>
            jtoken as JArray;

        public JToken ParseJToken(string json)
        {
            try
            {
                return JToken.Parse(json);
            }
            catch { }
            return null;
        }

        public JArray ParseJArray(string json) =>
            ParseJToken(json) as JArray;

        public JObject ParseJObject(string json) =>
            ParseJToken(json) as JObject;

        #endregion

        #region misc
        public string AddLinkPrefix(string linkBody, GlobalApis.Models.Datas.Enums.LinkTypes type) =>
            Misc.Utils.AddLinkPrefix(linkBody, type);

        public string Base64Encode(string plainText)
        {
            try
            {
                return Misc.Utils.Base64Encode(plainText);
            }
            catch { }
            return null;
        }

        public string Base64Decode(string b64String)
        {
            try
            {
                return Misc.Utils.Base64Decode(b64String);
            }
            catch { }
            return null;
        }

        public string GetLinkBody(string link) =>
            Misc.Utils.GetLinkBody(link);

        public string ScanQrcode()
        {
            var shareLink = @"";
            AutoResetEvent are = new AutoResetEvent(false);

            void Success(string result)
            {
                shareLink = result;
                are.Set();
            }

            void Fail()
            {
                are.Set();
            }

            Libs.QRCode.QRCode.ScanQRCode(Success, Fail);
            are.WaitOne(10000);
            return shareLink;
        }

        public void ExecuteInParallel<TParam>(
            IEnumerable<TParam> source, Action<TParam> worker) =>
            Misc.Utils.ExecuteInParallel(source, worker);

        public void ExecuteInParallel<TParam, TResult>(
            IEnumerable<TParam> source, Func<TParam, TResult> worker) =>
            Misc.Utils.ExecuteInParallel(source, worker);
        #endregion

    }
}
