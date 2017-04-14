using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace xx_udp_server
{
    public class xxUDPBody
    {
        public byte[] BodyBytes { get; set; }
        public string IpAddr => Remote.Address.ToString();

        public int Port => Remote.Port;

        public IPEndPoint Remote { get; set; }

        public xxUDPBody(byte[] bodyBytes)
        {
            BodyBytes = bodyBytes;
        }

        public xxUDPBody()
        {
        }
    }
}
