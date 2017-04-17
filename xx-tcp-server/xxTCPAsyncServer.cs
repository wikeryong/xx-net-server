using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace xx_tcp
{
    public class ReadState
    {
        public string sessionId { get; set; }
        // Client  socket.
        public Socket workSocket = null;
        public xxTCPHeader header { get; set; }
        // Size of receive buffer.
        // Receive buffer.
        public byte[] HeaderBytes { get; set; }
        public byte[] BodyBytes { get; set; }
    }

    public class SendState
    {
        public Socket RemoteSocket { get; set; }
        public xxTCPMsg Msg { get; set; }

        public xxClient Client { get; set; }

        public bool CloseClient { get; set; }
    }

    public class xxTCPAsyncServer
    {
        private static xxLogManager LOG = xxLogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        

        public xxTCPServer.InstanceHeaderNeed InstanceHeader;
        public event xxTCPServer.MainNotifyHandler MainNotify;
        public event xxTCPServer.ReadErrorHandler ReadSocketError;
        public event xxTCPServer.SendErrorHandler SendSocketError;
        public event xxTCPServer.SendFinishHandler SendFinish;
        /// <summary>
        /// 获取和设置Header的长度
        /// </summary>
        public int HeaderLength { get; set; }
        public string ServerName { get; set; }

        public static ManualResetEvent allDone = new ManualResetEvent(false);

        public bool PrintReceiveHex = false;
        public bool PrintSendHex = false;

        public void Start(int port)
        {
            //创建套接字  
            IPEndPoint ipe = new IPEndPoint(IPAddress.Any, port);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            //绑定端口和IP  
            socket.Bind(ipe);
            //设置监听  
            socket.Listen(10);
            LOG.InfoFormat("Server start on port:{0}", port);
            LOG.InfoFormat("Waiting for a connection");
            //连接客户端  
            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                socket.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    socket);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Create the state object.
            ReadState readState = new ReadState();
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket) ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                string sessionId = xxTCPClients.AssignSessionId();
                LOG.InfoFormat("New client:{0},sessionId:{1}", handler.RemoteEndPoint, sessionId);

                xxClient client = new xxClient();
                client.socket = handler;
                client.SessionId = sessionId;
                xxTCPClients.Add(client.SessionId, client);
                LOG.InfoFormat("Clients count:{0}",xxTCPClients.ClientCount);

                readState.workSocket = handler;
                readState.sessionId = client.SessionId;

                readState.HeaderBytes = new byte[HeaderLength];
                handler.BeginReceive(readState.HeaderBytes, 0, HeaderLength, SocketFlags.None, new AsyncCallback(ReadHeadCallback), readState);
            }
            catch (Exception e)
            {
                ReadException(e,readState);
            }
        }

        public void ReadHeadCallback(IAsyncResult ar)
        {
            ReadState readState = (ReadState)ar.AsyncState;
            try
            {
                Socket handler = readState.workSocket;
                if (!handler.Connected)
                {
                    LOG.InfoFormat("Connection closed!");
                    return;
                }
                int bytesRead = handler.EndReceive(ar);
                if (bytesRead > 0)
                {
                    xxTCPHeader header = InstanceHeader?.Invoke();
                    if (header == null)
                    {
                        LOG.Error("Not found instance xxHeader class");
                        return;
                    }
                    header.bytes = readState.HeaderBytes;
                    header.RemoteSocket = handler;
                    header.Decode();
                    header.SessionId = readState.sessionId;
                    readState.header = header;
                    header.Debug();
                    header.Info();
                    if (PrintReceiveHex)
                    {
                        PrintUtils.PrintHex(header.bytes);
                    }

                    

                    LOG.InfoFormat("Read header from:{0},body len:{1}",handler.RemoteEndPoint,header.bodyLength);
                    readState.BodyBytes = new byte[header.bodyLength];
                    handler.BeginReceive(readState.BodyBytes, 0, header.bodyLength, SocketFlags.None, new AsyncCallback(ReadCallback), readState);
                }
            }
            catch (Exception e)
            {
                ReadException(e,readState);
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            ReadState readState = (ReadState)ar.AsyncState;
            try
            {
                Socket handler = readState.workSocket;

                // Read data from the client socket. 
                int bytesRead = handler.EndReceive(ar);
                xxTCPHeader header = readState.header;

                if (bytesRead > 0)
                {
                
                
                    if (PrintReceiveHex)
                    {
                        PrintUtils.PrintHex(readState.BodyBytes);
                    }

                    xxTCPBody body = header.InstanceBody();
                    body.BodyBytes = readState.BodyBytes;
                    body.Decode();
                    body.Debug();
                    body.Info();

                    MainNotify?.Invoke(header, body);
                    xxTCPMsg sendMsg = body.GetSendMsg();
                    if (sendMsg != null)
                    {
                        Send(header.SessionId, sendMsg, sendMsg.CloseClient);
                    }

                    //将客户端状态重置为接收状态
                    ReadState readStateNew = new ReadState();
                    readStateNew.workSocket = handler;
                    readStateNew.sessionId = header.SessionId;
                    readStateNew.HeaderBytes = new byte[HeaderLength];
                    handler.BeginReceive(readStateNew.HeaderBytes, 0, HeaderLength, SocketFlags.None, new AsyncCallback(ReadHeadCallback), readStateNew);
                }
            }
            catch (Exception e)
            {
                ReadException(e,readState);
            }
        }

        public void Send(string sessionId, xxTCPMsg msg,bool closeClient)
        {

            try
            {
                // Convert the string data to byte data using ASCII encoding.
                //            byte[] byteData = Encoding.ASCII.GetBytes(data);
                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new Exception("Session id is null!");
                }
                xxClient client = xxTCPClients.GetClient(sessionId);
                if (client == null)
                {
                    throw new Exception("Not found client by session:"+ sessionId);
                }
                LOG.DebugFormat("Send {0} bytes to {1}", msg.MsgBytes.Length, client.socket.RemoteEndPoint);
                if (PrintSendHex)
                {
                    PrintUtils.PrintHex(msg.MsgBytes);
                }
                SendState state = new SendState();
                state.CloseClient = closeClient;
                state.RemoteSocket = client.socket;
                state.Client = client;
                state.Msg = msg;
                // Begin sending the data to the remote device.
                client.socket.BeginSend(msg.MsgBytes, 0, msg.MsgBytes.Length, 0,
                    new AsyncCallback(SendCallback), state);
            }
            catch (Exception e)
            {
                ReadException(e,null);
            }
        }


        private void SendCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            SendState handler = (SendState)ar.AsyncState;
            try
            {
                Socket socket = handler.RemoteSocket;
                // Complete sending the data to the remote device.
                int bytesSent = socket.EndSend(ar);
                LOG.InfoFormat("Sent {0} bytes to {1} finish!", bytesSent, socket.RemoteEndPoint);
                SendFinish?.Invoke(handler);
                if (handler.CloseClient)
                {
                    xxTCPClients.Close(handler.Client.SessionId);
                    LOG.InfoFormat("Client :{0} closed!", handler.Client.SessionId);
                }
            }
            catch (Exception e)
            {
                WriteException(e, handler);
            }
        }

        private void ReadException(Exception e, ReadState state)
        {
            LOG.Error("xxServer read error",e);
            ReadSocketError?.Invoke(e,state);
        }

        private void WriteException(Exception e, SendState state)
        {
            LOG.Error("xxServer write error", e);
            SendSocketError?.Invoke(e, state);
        }
    }
}
