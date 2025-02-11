using System;
using System.Collections.Generic;
using System.Web;
using Redbox.Core;

namespace Redbox.IPC.Framework
{
    public class IPCProtocol
    {
        private List<string> m_properties;
        private Dictionary<string, string> m_query;
        private string m_rawUri;

        public List<string> Properties
        {
            get
            {
                if (m_properties == null)
                    m_properties = new List<string>();
                return m_properties;
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
                if (m_query == null)
                    m_query = new Dictionary<string, string>();
                return m_query;
            }
            set
            {
                if (value == null)
                {
                    m_query = null;
                }
                else
                {
                    Query.Clear();
                    value.ForEach(pair => Query.Add(pair.Key, pair.Value));
                }
            }
        }

        public static bool Validate(string uri)
        {
            try
            {
                Parse(uri);
                return true;
            }
            catch (UriFormatException ex)
            {
                Console.WriteLine("Caught URIformatException: {0}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Caught Exception: {0}", ex.Message);
                return false;
            }
        }

        public static IPCProtocol Parse(string protocolURI)
        {
            protocolURI = protocolURI.Trim();
            var ipcProtocol = new IPCProtocol
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
            if ((ipcProtocol.Channel == ChannelType.Socket || ipcProtocol.Channel == ChannelType.Remoting) &&
                !string.IsNullOrEmpty(ipcProtocol.Port) && !int.TryParse(ipcProtocol.Port, out var _))
                throw new UriFormatException(string.Format("Protocol is set up for {0}, but port isn't a valid number.",
                    ipcProtocol.Channel));
            return ipcProtocol;
        }

        public override string ToString()
        {
            return m_rawUri;
        }

        private void Read(string protocolURI)
        {
            protocolURI = protocolURI.Replace(" ", "");
            var uri = new Uri(protocolURI);
            m_rawUri = protocolURI;
            Host = uri.Host;
            if (uri.Port != -1)
                Port = uri.Port.ToString();
            if (!string.IsNullOrEmpty(uri.Query))
            {
                var queries = HttpUtility.ParseQueryString(uri.Query);
                queries.AllKeys.ForEach(key => Query.Add(key, queries[key]));
            }

            SetChannel(uri);
            Scheme = uri.Scheme;
            if (Query.ContainsKey("USERNAME"))
                UserName = Query["USERNAME"];
            if (Query.ContainsKey("PASSWORD"))
                Password = Query["PASSWORD"];
            var count = 0;
            if (uri.Segments.Length == 0)
                return;
            uri.Segments.ForEach(s =>
            {
                s = s.Replace("/", "").Replace(" ", "");
                if (count == 0)
                {
                    if (!string.IsNullOrEmpty(s))
                        Properties.Add(s);
                }
                else
                {
                    Properties.Add(s);
                }

                ++count;
            });
        }

        private void SetChannel(Uri uri)
        {
            Channel = ChannelType.Unknown;
            IsSecure = false;
            if (uri.Scheme.Equals("rcp", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.Socket;
            }
            else if (uri.Scheme.Equals("srcp", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.Socket;
                IsSecure = true;
            }
            else if (uri.Scheme.Equals("rcp-p", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.NamedPipe;
            }
            else if (uri.Scheme.Equals("srcp-p", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.NamedPipe;
                IsSecure = true;
            }
            else if (uri.Scheme.Equals("rcp-amq", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.ActiveMQ;
            }
            else if (uri.Scheme.Equals("rcp-msmq", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.MSMQ;
            }
            else if (uri.Scheme.Equals("rcp-tems", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.TibcoEMS;
            }
            else if (uri.Scheme.Equals("rcp-oaq", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.OracleMQ;
            }
            else if (uri.Scheme.Equals("rcp-wmq", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.WebSphereMQ;
            }
            else if (uri.Scheme.Equals("rcp-rem", StringComparison.CurrentCultureIgnoreCase))
            {
                Channel = ChannelType.Remoting;
            }
            else
            {
                if (!uri.Scheme.Equals("srcp-rem", StringComparison.CurrentCultureIgnoreCase))
                    return;
                Channel = ChannelType.Remoting;
                IsSecure = true;
            }
        }
    }
}