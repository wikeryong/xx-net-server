using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_udp_server
{
    public class xxUDPMsg
    {

        public xxUDPBody MsgBody { get;private set;}
        public byte[] MsgBytes { get; private set; }

        public xxUDPMsg(byte[] bodyBytes)
        {
            this.MsgBytes = bodyBytes;
        }

        public xxUDPMsg(xxUDPBody msgBody)
        {
            MsgBody = msgBody;
        }
    }
}
