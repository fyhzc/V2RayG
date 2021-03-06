using Newtonsoft.Json;

namespace V2RayG.Models.Datas
{
    public class Vmess
    {
        public string ps, add, port, id, aid, net, type, host, tls, v, path, sni;

        public Vmess()
        {
            v = string.Empty;       // v1:"" v2:"2"
            ps = string.Empty;      // alias
            add = string.Empty;     // ip,hostname
            port = string.Empty;    // port
            id = string.Empty;      // user id
            aid = string.Empty;
            net = string.Empty;     // ws,tcp,kcp
            type = string.Empty;    // kcp->header
            host = string.Empty;    // v1: ws->path v2: ws->host h2->["host1","host2"]
            path = string.Empty;    // v1: "" v2: ws->path h2->path
            tls = string.Empty;     // streamSettings->security
            sni = string.Empty;     // tlsSettings.serverName
        }

        public bool Equals(Vmess t)
        {
            if (t == null
                || !t.sni.Equals(this.sni)
                || !t.v.Equals(this.v)
                || !t.ps.Equals(this.ps)
                || !t.add.Equals(this.add)
                || !t.port.Equals(this.port)
                || !t.id.Equals(this.id)
                || !t.aid.Equals(this.aid)
                || !t.net.Equals(this.net)
                || !t.type.Equals(this.type)
                || !t.host.Equals(this.host)
                || !t.path.Equals(this.path)
                || !t.tls.Equals(this.tls))
            {
                return false;
            }
            return true;
        }

        public string ToShareLink()
        {
            var vmess = this;
            if (vmess == null)
            {
                return null;
            }

            string content = JsonConvert.SerializeObject(vmess);
            return Misc.Utils.AddLinkPrefix(
                Misc.Utils.Base64Encode(content),
                Apis.Models.Datas.Enums.LinkTypes.vmess);
        }
    }
}
