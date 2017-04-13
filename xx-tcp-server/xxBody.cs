using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace xx_tcp
{
    public class xxBody
    {
        private byte[] bodyBytes;

        private MemoryStream stream = new MemoryStream();

        public byte[] BodyBytes
        {
            get { return bodyBytes; }
            set { bodyBytes = value; }
        }



        public virtual void Debug()
        {

        }

        public virtual void Info()
        {
        }

        public virtual void Decode()
        {
        }

        public virtual xxMsg GetSendMsg()
        {
            return null;
        }

        public virtual void Encode()
        {
        }

        public byte[] GetBytes() => stream.ToArray();

        public void WriteBytes(byte[] bts)
        {
            stream.Write(bts,0,bts.Length);
        }

    }
}
