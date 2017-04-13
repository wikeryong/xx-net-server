using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using xx_tcp;

namespace cs_tcp_test
{
    class Program
    {
        private static log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        static void Main(string[] args)
        {

            AsyncServer server = xxServer.CreateServer("test1", CreateHeader);
            server.printReceiveHex = true;
            server.printSendHex = true;
            server.HeaderSize = 8;  // 这个必须设置
            server.MainNotify += MainHandler;  //这个也必须设置
            server.Start(8001);
        }

        private static void MainHandler(xxHeader header, xxBody body)
        {
            TestBody test = (TestBody)body;
            TestHeader testHeader = (TestHeader)header;
            LOG.Info("我去，这里竟然回调了" + test.testVal1 + "," + test.testVal2);
            LOG.InfoFormat("回调的MsgId:{0},bodyLen:{1},from:{2}", testHeader.msgId, testHeader.bodyLength + ",", testHeader.RemoteSocket.RemoteEndPoint);
        }

        private static xxHeader CreateHeader()
        {
            return new TestHeader();
        }
    }

    public class TestBody : xxBody
    {
        private static log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int testVal1;
        public string testVal2;

        public override void Debug()
        {
            LOG.Debug("BODY 打印了DEBUG");
        }

        public override void Info()
        {
            LOG.Info("BODy 打印了INFO");
        }

        public override void Decode()
        {
            testVal1 = BitConverter.ToInt32(BodyBytes, 4);
            testVal2 = Encoding.ASCII.GetString(BodyBytes, 4, 32);
        }

        public override xxMsg GetSendMsg()
        {
            TestHeader header = new TestHeader();
            header.msgId = 2;
            TestBody body = new TestBody();
            body.testVal1 = 110;
            body.testVal2 = "Wiker Yong ,Hello world";
            xxMsg msg = new xxMsg(header, body);
            msg.CloseClient = false;
            return msg;
        }

        public override void Encode()
        {
            WriteBytes(BitConverter.GetBytes(testVal1));
            WriteBytes(Encoding.ASCII.GetBytes(testVal2));
        }
    }

    public class TestHeader : xxHeader
    {
        private static log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int msgId { get; set; }
        public override void Encode()
        {
            WriteBytes(BitConverter.GetBytes(msgId));
            WriteBytes(BitConverter.GetBytes(bodyLength));
        }


        public override void Decode()
        {
            msgId = BitConverter.ToInt32(bytes, 0);
            bodyLength = BitConverter.ToInt32(bytes, 4);
        }

        public override void Debug()
        {
            LOG.Debug("Header 打印了DEBUG");
        }

        public override void Info()
        {
            LOG.Info("Header 打印了INFO");
        }

        public override xxBody InstanceBody()
        {
            return new TestBody();
        }
    }
}
