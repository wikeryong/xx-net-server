using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_udp_server
{
    public class xxStrBody:xxUDPBody
    {
        public string Content { get; set; }

        public xxStrBody(string content) : base()
        {
            Content = content;
        }

        public xxStrBody(byte[] bits)
        {
            base.BodyBytes = bits;
            Content = Encoding.ASCII.GetString(BodyBytes);
        }
    }
}
