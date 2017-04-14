﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace xx_udp_server
{
    public class xxUDPAsyncServer
    {

        private static log4net.ILog LOG = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region Fields  
        /// <summary>  
        /// 服务器程序允许的最大客户端连接数  
        /// </summary>  
        private int MaxClient;

        public UDPServerType ServerType { get; set; } = UDPServerType.ASCII;
        /// <summary>  
        /// 当前的连接的客户端数  
        /// </summary>  
        //private int _clientCount;  

        /// <summary>  
        /// 服务器使用的异步UdpClient  
        /// </summary>  
        private UdpClient _server;

        public int ReceiveBufferSize { get; set; } = 1024;

        /// <summary>  
        /// 客户端会话列表  
        /// </summary>  
        //private List<AsyncUDPState> _clients;  

        private bool disposed = false;

        /// <summary>  
        /// 数据接受缓冲区  
        /// </summary>  
        private byte[] _recvBuffer;

        #endregion

        #region Properties  

        /// <summary>  
        /// 服务器是否正在运行  
        /// </summary>  
        public bool IsRunning { get; private set; }
        /// <summary>  
        /// 监听的IP地址  
        /// </summary>  
        public IPAddress Address { get; private set; }
        /// <summary>  
        /// 监听的端口  
        /// </summary>  
        public int Port { get; private set; }
        /// <summary>  
        /// 通信使用的编码  
        /// </summary>  
        public Encoding Encoding { get; set; }

        private bool PrintHex { get; set; } = true;

        private AsyncUDPState udpReceiveState = null;

        #endregion

        #region 构造函数  

        /// <summary>  
        /// 异步UdpClient UDP服务器  
        /// </summary>  
        /// <param name="listenPort">监听的端口</param>  
        public xxUDPAsyncServer(int listenPort)
            : this(IPAddress.Any, listenPort, 1024)
        {
        }

        /// <summary>  
        /// 异步UdpClient UDP服务器  
        /// </summary>  
        /// <param name="localEP">监听的终结点</param>  
        public xxUDPAsyncServer(IPEndPoint localEP)
            : this(localEP.Address, localEP.Port, 1024)
        {
        }

        /// <summary>  
        /// 异步UdpClient UDP服务器  
        /// </summary>  
        /// <param name="localIPAddress">监听的IP地址</param>  
        /// <param name="listenPort">监听的端口</param>  
        /// <param name="maxClient">最大客户端数量</param>  
        public xxUDPAsyncServer(IPAddress localIPAddress, int listenPort, int maxClient)
        {
            this.Address = localIPAddress;
            this.Port = listenPort;
            this.Encoding = Encoding.Default;

            MaxClient = maxClient;
            //_clients = new List<AsyncUDPSocketState>();  
            _server = new UdpClient(new IPEndPoint(this.Address, this.Port));
            _server.Client.ReceiveBufferSize = ReceiveBufferSize;

            _recvBuffer = new byte[_server.Client.ReceiveBufferSize];
            udpReceiveState = new AsyncUDPState();
            udpReceiveState.udpClient = _server;
        }

        #endregion

        #region Method  
        /// <summary>  
        /// 启动服务器  
        /// </summary>  
        /// <returns>异步TCP服务器</returns>  
        public void Start()
        {
            if (!IsRunning)
            {
                LOG.InfoFormat("UDP server start on port:{0}", Port);
                IsRunning = true;
                _server.EnableBroadcast = true;
                _server.BeginReceive(ReceiveDataAsync, udpReceiveState);
            }
        }

        /// <summary>  
        /// 停止服务器  
        /// </summary>  
        public void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                _server.Close();
            }
        }

        /// <summary>  
        /// 接收数据的方法  
        /// </summary>  
        /// <param name="ar"></param>  
        private void ReceiveDataAsync(IAsyncResult ar)
        {
            AsyncUDPState udpState = ar.AsyncState as AsyncUDPState;
            IPEndPoint remote = null;
            byte[] buffer = null;
            try
            {
                buffer = _server.EndReceive(ar, ref remote);
                string receiveString = Encoding.ASCII.GetString(buffer);
                LOG.InfoFormat("Received {0}:{1},len:{2},body:{3}", remote.Address, remote.Port, buffer.Length, receiveString);
                if (PrintHex)
                {
                    PrintUtils.PrintHex(buffer);
                }
                udpState.remote = remote;
                //触发数据收到事件  
                RaiseDataReceived(udpState, buffer);
            }
            catch (Exception exception)
            {
                RaiseNetError(udpState,exception);
            }
            finally
            {
                if (IsRunning && _server != null)
                    _server.BeginReceive(ReceiveDataAsync, udpState);
            }
        }

        /// <summary>  
        /// 发送数据  
        /// </summary>  
        /// <param name="msg"></param>  
        /// <param name="remote"></param>  
        public void Send(IPEndPoint remote,byte[] bits)
        {
            try
            {
                LOG.InfoFormat("Send {0}:{1},body:{2}", remote.Address.ToString(), remote.Port, Encoding.ASCII.GetString(bits));
                if (PrintHex)
                {
                    PrintUtils.PrintHex(bits);
                }
                RaisePrepareSend(udpReceiveState);
                _server.BeginSend(bits, bits.Length, remote, new AsyncCallback(SendCallback), udpReceiveState);
            }
            catch (Exception)
            {
                RaiseOtherException(udpReceiveState);
            }
        }

        public void Send(IPEndPoint remote, xxUDPBody body)
        {
            Send(remote, body.BodyBytes);
        }

        public void Send(IPEndPoint remote, string msg)
        {
            Send(remote, Encoding.ASCII.GetBytes(msg));
        }

        public void Send(string ip, int port, string msg)
        {
            IPEndPoint remote = new IPEndPoint(IPAddress.Parse(ip), port);
            Send(remote,Encoding.ASCII.GetBytes(msg));
        }

        private void SendCallback(IAsyncResult ar)
        {
            AsyncUDPState state = ar.AsyncState as AsyncUDPState;
            if (ar.IsCompleted)
            {
                try
                {
                    _server.EndSend(ar);
                    //消息发送完毕事件  
                    RaiseCompletedSend(state);
                }
                catch (Exception)
                {
                    RaiseOtherException(state);
                }
            }

        }
        #endregion

        #region 事件  
        /// <summary>  
        /// 接收到数据事件  
        /// </summary>  
        public event xxUDPServer.EventHandler DataReceived;

        private void RaiseDataReceived(AsyncUDPState state, byte[] buffer)
        {
            DataReceived?.Invoke(this,
                ServerType == UDPServerType.ASCII
                    ? new xxUDPEventArgs(new xxStrBody(buffer), state)
                    : new xxUDPEventArgs(new xxUDPBody(buffer), state));
        }

        /// <summary>  
        /// 发送数据前的事件  
        /// </summary>  
        public event xxUDPServer.EventHandler PrepareSend;

        /// <summary>  
        /// 触发发送数据前的事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaisePrepareSend(AsyncUDPState state)
        {
            PrepareSend?.Invoke(this, new xxUDPEventArgs(state));
        }

        /// <summary>  
        /// 数据发送完毕事件  
        /// </summary>  
        public event xxUDPServer.EventHandler CompletedSend;

        /// <summary>  
        /// 触发数据发送完毕的事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseCompletedSend(AsyncUDPState state)
        {
            CompletedSend?.Invoke(this, new xxUDPEventArgs(state));
        }

        /// <summary>  
        /// 网络错误事件  
        /// </summary>  
        public event EventHandler NetError;
        /// <summary>  
        /// 触发网络错误事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseNetError(AsyncUDPState state,Exception exception)
        {
            NetError?.Invoke(this, new xxUDPEventArgs(exception,state));
        }

        /// <summary>  
        /// 异常事件  
        /// </summary>  
        public event EventHandler OtherException;
        /// <summary>  
        /// 触发异常事件  
        /// </summary>  
        /// <param name="state"></param>  
        private void RaiseOtherException(AsyncUDPState state,Exception exception)
        {
            OtherException?.Invoke(this, new xxUDPEventArgs(exception,state));
        }
        private void RaiseOtherException(AsyncUDPState state)
        {
            RaiseOtherException(state, null);
        }
        #endregion

        
        #region 释放  
        /// <summary>  
        /// Performs application-defined tasks associated with freeing,   
        /// releasing, or resetting unmanaged resources.  
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>  
        /// Releases unmanaged and - optionally - managed resources  
        /// </summary>  
        /// <param name="disposing"><c>true</c> to release   
        /// both managed and unmanaged resources; <c>false</c>   
        /// to release only unmanaged resources.</param>  
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();
                        if (_server != null)
                        {
                            _server = null;
                        }
                    }
                    catch (SocketException)
                    {
                        RaiseOtherException(null);
                    }
                }
                disposed = true;
            }
        }
        #endregion
    }
}
