using Redbox.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web;

namespace Redbox.IPC.Framework
{
    internal class IPCProtocol
    {
        private string m_rawUri;
        private List<string> m_properties;
        private Dictionary<string, string> m_query;

        public static bool Validate(string uri)
        {
            try
            {
                IPCProtocol.Parse(uri);
                return true;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("Caught URIformatException: {0}", (object)ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception: {0}", (object)ex.Message);
                return false;
            }
        }

        public static IPCProtocol Parse(string protocolURI)
        {
            protocolURI = protocolURI.Trim();
            IPCProtocol ipcProtocol = new IPCProtocol()
            {
                m_rawUri = protocolURI
            };
            try
            {
                ipcProtocol.Read(protocolURI);
            }
            catch (Exception ex)
            {
                throw new UriFormatException(ex.Message);
            }
            if (string.IsNullOrEmpty(ipcProtocol.Host))
                throw new UriFormatException("Host isn't set.");
            if (ipcProtocol.Channel == ChannelType.Unknown)
                throw new UriFormatException("The channel type is unknown; please correct your URI.");
            if ((ipcProtocol.Channel == ChannelType.Socket || ipcProtocol.Channel == ChannelType.Remoting) && !string.IsNullOrEmpty(ipcProtocol.Port) && !int.TryParse(ipcProtocol.Port, out int _))
                throw new UriFormatException(string.Format("Protocol is set up for {0}, but port isn't a valid number.", (object)ipcProtocol.Channel));
            return ipcProtocol;
        }

        public override string ToString() => this.m_rawUri;

        public List<string> Properties
        {
            get
            {
                if (this.m_properties == null)
                    this.m_properties = new List<string>();
                return this.m_properties;
            }
        }

        public bool IsSecure { get; internal set; }

        public ChannelType Channel { get; internal set; }

        public string Host { get; private set; }

        public string Port { get; private set; }

        public string Scheme { get; private set; }

        public string UserName { get; private set; }

        public string Password { get; private set; }

        public Dictionary<string, string> Query
        {
            get
            {
                if (this.m_query == null)
                    this.m_query = new Dictionary<string, string>();
                return this.m_query;
            }
            set
            {
                if (value == null)
                {
                    this.m_query = (Dictionary<string, string>)null;
                }
                else
                {
                    this.Query.Clear();
                    value.ForEach<KeyValuePair<string, string>>((Action<KeyValuePair<string, string>>)(pair => this.Query.Add(pair.Key, pair.Value)));
                }
            }
        }

        private void Read(string protocolURI)
        {
            protocolURI = protocolURI.Replace(" ", "");
            Uri uri = new Uri(protocolURI);
            this.m_rawUri = protocolURI;
            this.Host = uri.Host;
            if (uri.Port != -1)
                this.Port = uri.Port.ToString();
            if (!string.IsNullOrEmpty(uri.Query))
            {
                NameValueCollection queries = HttpUtility.ParseQueryString(uri.Query);
                ((IEnumerable<string>)queries.AllKeys).ForEach<string>((Action<string>)(key => this.Query.Add(key, queries[key])));
            }
            this.SetChannel(uri);
            this.Scheme = uri.Scheme;
            if (this.Query.ContainsKey("USERNAME"))
                this.UserName = this.Query["USERNAME"];
            if (this.Query.ContainsKey("PASSWORD"))
                this.Password = this.Query["PASSWORD"];
            int count = 0;
            if (uri.Segments.Length == 0)
                return;
            ((IEnumerable<string>)uri.Segments).ForEach<string>((Action<string>)(s =>
            {
                s = s.Replace("/", "").Replace(" ", "");
                if (count == 0)
                {
                    if (!string.IsNullOrEmpty(s))
                        this.Properties.Add(s);
                }
                else
                    this.Properties.Add(s);
                ++count;
            }));
        }

        private void SetChannel(Uri uri)
        {
            this.Channel = ChannelType.Unknown;
            this.IsSecure = false;
            if (uri.Scheme.Equals("rcp", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.Socket;
            else if (uri.Scheme.Equals("srcp", StringComparison.CurrentCultureIgnoreCase))
            {
                this.Channel = ChannelType.Socket;
                this.IsSecure = true;
            }
            else if (uri.Scheme.Equals("rcp-p", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.NamedPipe;
            else if (uri.Scheme.Equals("srcp-p", StringComparison.CurrentCultureIgnoreCase))
            {
                this.Channel = ChannelType.NamedPipe;
                this.IsSecure = true;
            }
            else if (uri.Scheme.Equals("rcp-amq", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.ActiveMQ;
            else if (uri.Scheme.Equals("rcp-msmq", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.MSMQ;
            else if (uri.Scheme.Equals("rcp-tems", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.TibcoEMS;
            else if (uri.Scheme.Equals("rcp-oaq", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.OracleMQ;
            else if (uri.Scheme.Equals("rcp-wmq", StringComparison.CurrentCultureIgnoreCase))
                this.Channel = ChannelType.WebSphereMQ;
            else if (uri.Scheme.Equals("rcp-rem", StringComparison.CurrentCultureIgnoreCase))
            {
                this.Channel = ChannelType.Remoting;
            }
            else
            {
                if (!uri.Scheme.Equals("srcp-rem", StringComparison.CurrentCultureIgnoreCase))
                    return;
                this.Channel = ChannelType.Remoting;
                this.IsSecure = true;
            }
        }
    }
}
