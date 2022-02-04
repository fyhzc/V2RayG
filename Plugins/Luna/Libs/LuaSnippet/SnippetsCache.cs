﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Luna.Libs.LuaSnippet
{
    internal sealed class SnippetsCache :
        Apis.BaseClasses.Disposable
    {

        List<LuaKeywordSnippets> keywordCache;
        List<LuaFuncSnippets> functionCache;
        List<LuaSubFuncSnippets> subFunctionCache;
        List<LuaImportClrSnippets> importClrCache;
        List<ApiFunctionSnippets> apiFunctionCache;

        public SnippetsCache()
        {
            GenSnippetCaches();
        }

        #region public methods

        public BestMatchSnippets CreateBestMatchSnippets(ScintillaNET.Scintilla editor)
        {
            return new BestMatchSnippets(
                editor,

                apiFunctionCache,
                functionCache,
                keywordCache,
                subFunctionCache,
                importClrCache);
        }

        #endregion

        #region private methods

        List<string> GetAllNameapaces() => Apis.Misc.Utils.GetAllAssembliesType()
            .Select(t => t.Namespace)
            .Distinct()
            .Where(n => !(
                string.IsNullOrEmpty(n)
                || n.StartsWith("<")
                || n.StartsWith("AutocompleteMenuNS")
                || n.StartsWith("AutoUpdaterDotNET")
                || n.StartsWith("Internal.Cryptography")
                || n.StartsWith("Luna")
                || n.StartsWith("Pacman")
                || n.StartsWith("ProxySetter")
                || n.StartsWith("ResourceEmbedderCompilerGenerated")
                || n.StartsWith("Statistics")
                || n.StartsWith("V2RayG")
                || n.StartsWith("Apis")
            ))
            .ToList();

        List<string> GetAllAssembliesName()
        {
            var nsps = GetAllNameapaces();
            return AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.FullName)
                .Where(fn => !string.IsNullOrEmpty(fn) && nsps.Where(nsp => fn.StartsWith(nsp)).FirstOrDefault() != null)
                .Union(nsps)
                .OrderBy(n => n)
                .Select(n => $"import('{n}')")
                .ToList();
        }

        List<LuaImportClrSnippets> GenLuaImportClrSnippet() =>
            GetAllAssembliesName()
                .Select(e =>
                {
                    try
                    {
                        return new LuaImportClrSnippets(e);
                    }
                    catch { }
                    return null;
                })
                .Where(e => e != null)
                .ToList();


        List<string> GenKeywords(
            IEnumerable<string> initValues) =>
            new StringBuilder(Apis.Models.Consts.Lua.LuaModules)
            .Append(@" ")
            .Append(Apis.Models.Consts.Lua.LuaKeywords)
            .ToString()
            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .Union(initValues)
            .Union(new string[] { "setmetatable(o, {__index = mn})" })
            .OrderBy(e => e)
            .ToList();

        List<LuaFuncSnippets> GenLuaFunctionSnippet() =>
            Apis.Models.Consts.Lua.LuaFunctions
            .Replace("dofile", "")
            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .OrderBy(s => s)
            .Select(e =>
            {
                try
                {
                    return new LuaFuncSnippets($"{e}()");
                }
                catch { }
                return null;
            })
            .Where(e => e != null)
            .ToList();

        List<LuaSubFuncSnippets> GenLuaPredefinedFuncSnippets(IEnumerable<LuaSubFuncSnippets> append) =>
            Apis.Models.Consts.Lua.LuaPredefinedFunctionNames
            .Select(fn => new LuaSubFuncSnippets(fn, "."))
            .Union(append)
            .ToList();



        List<LuaSubFuncSnippets> GenLuaSubFunctionSnippet() =>
            Apis.Models.Consts.Lua.LuaSubFunctions
            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
            .OrderBy(s => s)
            .Select(e =>
            {
                try
                {
                    return new LuaSubFuncSnippets($"{e}()", ".");
                }
                catch { }
                return null;
            })
            .Where(e => e != null)
            .ToList();

        void GenSnippetCaches()
        {
            var apis = new List<Tuple<string, Type>>
            {
                new Tuple<string,Type>("mailbox", typeof(Apis.Interfaces.Lua.ILuaMailBox)),
                new Tuple<string,Type>("mail", typeof(Apis.Interfaces.Lua.ILuaMail)),
                new Tuple<string,Type>("Sys", typeof(Apis.Interfaces.Lua.ILuaSys)),
                new Tuple<string,Type>("Misc", typeof(Apis.Interfaces.Lua.ILuaMisc)),
                new Tuple<string,Type>("Server", typeof(Apis.Interfaces.Lua.ILuaServer)),
                new Tuple<string,Type>("Web", typeof(Apis.Interfaces.Lua.ILuaWeb)),
                new Tuple<string,Type>("Signal", typeof(Apis.Interfaces.Lua.ILuaSignal)),
                new Tuple<string, Type>("coreServ",typeof(Apis.Interfaces.ICoreServCtrl)),
                new Tuple<string, Type>("coreConfiger",typeof(Apis.Interfaces.CoreCtrlComponents.IConfiger)),
                new Tuple<string, Type>("coreCtrl",typeof(Apis.Interfaces.CoreCtrlComponents.ICoreCtrl)),
                new Tuple<string, Type>("coreState",typeof(Apis.Interfaces.CoreCtrlComponents.ICoreStates)),
                new Tuple<string, Type>("coreLogger",typeof(Apis.Interfaces.CoreCtrlComponents.ILogger)),
            };

            keywordCache = GenKeywordSnippetItems(GenKeywords(apis.Select(e => e.Item1)));
            functionCache = GenLuaFunctionSnippet();

            var orgLuaSubFuncSnippet = GenLuaSubFunctionSnippet();

            var apiEvSnippets = apis.SelectMany(api => GenApisEventSnippet(api.Item1, api.Item2));
            var apiPropSnippets = apis.SelectMany(api => GenApisPropSnippet(api.Item1, api.Item2));

            subFunctionCache = apiPropSnippets
                .Concat(apiEvSnippets)
                .Concat(GenLuaPredefinedFuncSnippets(orgLuaSubFuncSnippet))
                .ToList();

            importClrCache = GenLuaImportClrSnippet();

            apiFunctionCache = apis
               .SelectMany(api => GenApiFunctionSnippetItems(api.Item1, api.Item2))
               .ToList();
        }

        List<LuaKeywordSnippets> GenKeywordSnippetItems(IEnumerable<string> keywords) =>
            keywords
            .OrderBy(k => k)
            .Select(e => new LuaKeywordSnippets(e))
            .ToList();

        IEnumerable<LuaSubFuncSnippets> GenApisPropSnippet(string apiName, Type type) =>
          Apis.Misc.Utils.GetPublicPropsInfoOfType(type)
              .OrderBy(infos => infos.Item2)
              .Select(infos => new LuaSubFuncSnippets($"{apiName}.{infos.Item2}", "."));

        IEnumerable<LuaSubFuncSnippets> GenApisEventSnippet(string apiName, Type type) =>
            Apis.Misc.Utils.GetPublicEventsInfoOfType(type)
                .OrderBy(infos => infos.Item2)
                .Select(infos => new LuaSubFuncSnippets($"{apiName}.{infos.Item2}", "."));

        IEnumerable<ApiFunctionSnippets> GenApiFunctionSnippetItems(
            string apiName, Type type) =>
            Apis.Misc.Utils.GetPublicMethodNameAndParam(type)
            .OrderBy(info => info.Item2)  // item2 = method name
            .Select(info => new ApiFunctionSnippets(
                info.Item1, // return type
                apiName,
                info.Item2, // methodName,
                info.Item3, // paramStr,
                info.Item4, // paramWithType,
                @"")
            );

        #endregion

        #region protected methods
        protected override void Cleanup()
        {
            // acm will dispose it self.
            // acm?.Dispose();
        }
        #endregion
    }
}
