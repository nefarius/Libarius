using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

/*
 * Project origin:  http://www.codeproject.com/Articles/27992/NAT-Traversal-with-UPnP-in-C
 * Original author: http://www.codeproject.com/Members/Harold-Aptroot
 * Modified by:     Benjamin "Nefarius" Höglinger <nefarius@dhmx.at>
 *                  http://nefarius.at/
 * */
namespace Libarius.Network.UPnP
{
    public class NAT
    {
        static TimeSpan _timeout = new TimeSpan(0, 0, 0, 3);
        public static TimeSpan TimeOut
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        static string _descUrl, _serviceUrl, _eventUrl;

        /// <summary>
        /// Sends out a NOTIFY request and waits for an answer.
        /// </summary>
        /// <returns>Returns true on success and false if no answer was received.</returns>
        public static bool Discover()
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 3000);
            string req = "M-SEARCH * HTTP/1.1\r\n" +
            "HOST: 239.255.255.250:1900\r\n" +
            "ST:upnp:rootdevice\r\n" +
            "MAN:\"ssdp:discover\"\r\n" +
            "MX:3\r\n\r\n";
            byte[] data = Encoding.ASCII.GetBytes(req);
            IPEndPoint ipe = new IPEndPoint(IPAddress.Broadcast, 1900);
            byte[] buffer = new byte[0x1000];

            DateTime start = DateTime.Now;

            do
            {
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);
                s.SendTo(data, ipe);

                int length = 0;
                do
                {
                    length = s.Receive(buffer);

                    string resp = Encoding.ASCII.GetString(buffer, 0, length).ToLower();
                    if (resp.Contains("upnp:rootdevice"))
                    {
                        resp = resp.Substring(resp.ToLower().IndexOf("location:") + 9);
                        resp = resp.Substring(0, resp.IndexOf("\r")).Trim();
                        if (!string.IsNullOrEmpty(_serviceUrl = GetServiceUrl(resp)))
                        {
                            _descUrl = resp;
                            return true;
                        }
                    }
                } while (length > 0);
            } while (start.Subtract(DateTime.Now) < _timeout);
            return false;
        }

        private static string GetServiceUrl(string resp)
        {
#if !DEBUG
            try
            {
#endif
                XmlDocument desc = new XmlDocument();
                desc.Load(WebRequest.Create(resp).GetResponse().GetResponseStream());
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(desc.NameTable);
                nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                XmlNode typen = desc.SelectSingleNode("//tns:device/tns:deviceType/text()", nsMgr);
                if (!typen.Value.Contains("InternetGatewayDevice"))
                    return null;
                XmlNode node = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:controlURL/text()", nsMgr);
                if (node == null)
                    return null;
                XmlNode eventnode = desc.SelectSingleNode("//tns:service[tns:serviceType=\"urn:schemas-upnp-org:service:WANIPConnection:1\"]/tns:eventSubURL/text()", nsMgr);
                _eventUrl = CombineUrls(resp, eventnode.Value);
                return CombineUrls(resp, node.Value);
#if !DEBUG
            }
            catch { return null; }
#endif
        }

        private static string CombineUrls(string resp, string p)
        {
            int n = resp.IndexOf("://");
            n = resp.IndexOf('/', n + 3);
            return resp.Substring(0, n) + p;
        }

        /// <summary>
        /// Requests the UPnP device to forward a specific port to the current hosts local IP address.
        /// </summary>
        /// <param name="port">Number of external (e.g. WAN) port to forward.</param>
        /// <param name="protocol">Desired protocol.</param>
        /// <param name="description">Description the device should name this new forwarding rule.</param>
        public static void ForwardPort(int port, ProtocolType protocol, string description)
        {
            ForwardPort(port, port, IPHelper.PrivateIpAddress, protocol, description);
        }

        /// <summary>
        /// Requests the UPnP device to forward a specific external port to a defined internal port at the current hosts local IP address.
        /// </summary>
        /// <param name="externalPort">Number of external (e.g. WAN) port to forward.</param>
        /// <param name="localPort">Number of local port to get forwarded to.</param>
        /// <param name="protocol">Desired protocol.</param>
        /// <param name="description">Description the device should name this new forwarding rule.</param>
        public static void ForwardPort(int externalPort, int localPort, ProtocolType protocol, string description)
        {
            ForwardPort(externalPort, localPort, IPHelper.PrivateIpAddress, protocol, description);
        }

        /// <summary>
        /// Requests the UPnP device to forward a specific port to a defined internal IP address and port number.
        /// </summary>
        /// <param name="externalPort">Number of external (e.g. WAN) port to forward.</param>
        /// <param name="localPort">Number of local port to get forwarded to.</param>
        /// <param name="localIp">Desired target hosts local IP address.</param>
        /// <param name="protocol">Desired protocol.</param>
        /// <param name="description">Description the device should name this new forwarding rule.</param>
        public static void ForwardPort(int externalPort, int localPort, IPAddress localIp, ProtocolType protocol, string description)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:AddPortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "<NewRemoteHost></NewRemoteHost><NewExternalPort>" + externalPort + "</NewExternalPort><NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
                "<NewInternalPort>" + localPort + "</NewInternalPort><NewInternalClient>" + localIp +
                "</NewInternalClient><NewEnabled>1</NewEnabled><NewPortMappingDescription>" + description +
            "</NewPortMappingDescription><NewLeaseDuration>0</NewLeaseDuration></u:AddPortMapping>", "AddPortMapping");
        }

        /// <summary>
        /// Requests the UPnP device to delete a specified forwarded external port.
        /// </summary>
        /// <param name="port">Number of external (e.g. WAN) port which is currently forwarded.</param>
        /// <param name="protocol">The protocol the target forwarding is registered under.</param>
        public static void DeleteForwardingRule(int port, ProtocolType protocol)
        {
            if (string.IsNullOrEmpty(_serviceUrl))
                throw new Exception("No UPnP service available or Discover() has not been called");
            XmlDocument xdoc = SOAPRequest(_serviceUrl,
            "<u:DeletePortMapping xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
            "<NewRemoteHost>" +
            "</NewRemoteHost>" +
            "<NewExternalPort>"+ port+"</NewExternalPort>" +
            "<NewProtocol>" + protocol.ToString().ToUpper() + "</NewProtocol>" +
            "</u:DeletePortMapping>", "DeletePortMapping");
        }

        /// <summary>
        /// Returns the WAN IP address the UPnP device discovered. This doesn't have to be the public IP address.
        /// </summary>
        public static IPAddress ExternalIP
        {
            get
            {
                if (string.IsNullOrEmpty(_serviceUrl))
                    throw new Exception("No UPnP service available or Discover() has not been called");
                XmlDocument xdoc = SOAPRequest(_serviceUrl, "<u:GetExternalIPAddress xmlns:u=\"urn:schemas-upnp-org:service:WANIPConnection:1\">" +
                "</u:GetExternalIPAddress>", "GetExternalIPAddress");
                XmlNamespaceManager nsMgr = new XmlNamespaceManager(xdoc.NameTable);
                nsMgr.AddNamespace("tns", "urn:schemas-upnp-org:device-1-0");
                string IP = xdoc.SelectSingleNode("//NewExternalIPAddress/text()", nsMgr).Value;
                return IPAddress.Parse(IP);
            }
        }

        private static XmlDocument SOAPRequest(string url, string soap, string function)
        {
            string req = "<?xml version=\"1.0\"?>" +
            "<s:Envelope xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/\" s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/\">" +
            "<s:Body>" +
            soap +
            "</s:Body>" +
            "</s:Envelope>";
            WebRequest r = HttpWebRequest.Create(url);
            r.Method = "POST";
            byte[] b = Encoding.UTF8.GetBytes(req);
            r.Headers.Add("SOAPACTION", "\"urn:schemas-upnp-org:service:WANIPConnection:1#" + function + "\"");
            r.ContentType = "text/xml; charset=\"utf-8\"";
            r.ContentLength = b.Length;
            r.GetRequestStream().Write(b, 0, b.Length);
            XmlDocument resp = new XmlDocument();
            WebResponse wres = r.GetResponse();
            Stream ress = wres.GetResponseStream();
            resp.Load(ress);
            return resp;
        }
    }
}
