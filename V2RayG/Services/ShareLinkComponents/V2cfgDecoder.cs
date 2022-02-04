﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace V2RayG.Services.ShareLinkComponents
{
    internal sealed class V2cfgDecoder :
        Apis.BaseClasses.ComponentOf<Codecs>,
        Apis.Interfaces.IShareLinkDecoder
    {
        public V2cfgDecoder() { }

        #region properties

        #endregion

        #region public methods
        public Tuple<JObject, JToken> Decode(string shareLink)
        {
            try
            {
                var linkBody = Misc.Utils.GetLinkBody(shareLink);
                var plainText = Misc.Utils.Base64Decode(linkBody);
                var config = JObject.Parse(plainText);
                if (config != null)
                {
                    return new Tuple<JObject, JToken>(config, null);
                }
            }
            catch { }
            return null;
        }

        public string Encode(string config)
        {
            if (string.IsNullOrEmpty(config))
            {
                return null;
            }

            var body = Misc.Utils.Base64Encode(config);

            return Misc.Utils.AddLinkPrefix(
                body,
                Apis.Models.Datas.Enums.LinkTypes.v2cfg);
        }


        public List<string> ExtractLinksFromText(string text) =>
            Misc.Utils.ExtractLinks(text, Apis.Models.Datas.Enums.LinkTypes.v2cfg);
        #endregion

        #region private methods

        #endregion

        #region protected methods

        #endregion
    }
}
