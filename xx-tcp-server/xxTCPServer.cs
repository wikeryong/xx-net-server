using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace xx_tcp_server
{
    public class xxTCPServer
    {

        private static IDictionary<string,xxTCPAsyncServer> servers = new Dictionary<string, xxTCPAsyncServer>();
        /// <summary>
        /// 创建Header的委托
        /// </summary>
        /// <returns></returns>
        public delegate xxTCPHeader InstanceHeaderNeed();

        /// <summary>
        /// 消息处理的委托
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public delegate void MainNotifyHandler(xxTCPHeader header, xxTCPBody body);

        public delegate void SendFinishHandler(SendState state);

        public delegate void ReadErrorHandler(Exception e, ReadState clientState);
        public delegate void SendErrorHandler(Exception e, SendState clientState);
        

        /// <summary>
        /// 创建一个Server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instanceHeader">实例化Header的方法</param>
        /// <returns></returns>
        public static xxTCPAsyncServer CreateServer(string name, InstanceHeaderNeed instanceHeader)
        {
            xxTCPAsyncServer asyncServer = new xxTCPAsyncServer();
            asyncServer.ServerName = name;
            servers.Add(name,asyncServer);
            asyncServer.InstanceHeader = instanceHeader;
            return asyncServer;
        }

        public static xxTCPAsyncServer GetServer(string name)
        {
            if (servers.ContainsKey(name))
            {
                return servers[name];
            }
            return null;
        }
        
    }
}
