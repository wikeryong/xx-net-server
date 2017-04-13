using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_tcp
{
    public class xxMsg
    {
        private xxHeader header;
        private xxBody body;

        public xxHeader Header
        {
            get { return header; }
            set { header = value; }
        }

        public xxBody Body
        {
            get { return body; }
            set { body = value; }
        }

        private byte[] headerBytes;
        private byte[] bodyBytes;

        public xxMsg(xxHeader header, xxBody body)
        {
            this.header = header;
            this.body = body;
            this.header.Encode();
            this.body.Encode();
            headerBytes = this.header.GetBytes();
            bodyBytes = this.body.GetBytes();
            MsgBytes = new byte[headerBytes.Length+bodyBytes.Length];
            headerBytes.CopyTo(MsgBytes,0);
            header.bodyLength = bodyBytes.Length;
            bodyBytes.CopyTo(MsgBytes,headerBytes.Length);
        }
        

        public byte[] MsgBytes { get; }

        public bool CloseClient { get; set; }
    }
}
