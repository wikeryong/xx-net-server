using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace xx_tcp
{
    public abstract class xxTCPHeader
    {

        public string SessionId { get; set; }

        private Socket remoteSocket;

        public Socket RemoteSocket
        {
            get { return remoteSocket; }
            set { remoteSocket = value; }
        }

        private byte[] _bytes;

        public byte[] bytes
        {
            get { return _bytes;}
            set { _bytes = value; }
        }


        public int bodyLength { get; set; }

        public virtual void Encode()
        {
        }

        public virtual void Decode() { }

        public virtual void Debug()
        {
            
        }

        public virtual void Info()
        {
        }

        public abstract xxTCPBody InstanceBody();


        private MemoryStream stream = new MemoryStream();
        public byte[] GetBytes() => stream.ToArray();

        public void WriteBytes(byte[] bts)
        {
            stream.Write(bts, 0, bts.Length);
        }
    }
}
