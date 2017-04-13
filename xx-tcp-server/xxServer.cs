using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace xx_tcp
{
    public class xxServer
    {

        private static IDictionary<string,AsyncServer> servers = new Dictionary<string, AsyncServer>();
        /// <summary>
        /// 创建Header的委托
        /// </summary>
        /// <returns></returns>
        public delegate xxHeader InstanceHeaderNeed();

        /// <summary>
        /// 消息处理的委托
        /// </summary>
        /// <param name="header"></param>
        /// <param name="body"></param>
        public delegate void MainNotifyHandler(xxHeader header, xxBody body);

        public delegate void SendFinishHandler(SendState state);

        public delegate void ReadErrorHandler(Exception e, ReadState clientState);
        public delegate void SendErrorHandler(Exception e, SendState clientState);
        

        /// <summary>
        /// 创建一个Server
        /// </summary>
        /// <param name="name"></param>
        /// <param name="instanceHeader">实例化Header的方法</param>
        /// <returns></returns>
        public static AsyncServer CreateServer(string name, InstanceHeaderNeed instanceHeader)
        {
            AsyncServer asyncServer = new AsyncServer();
            asyncServer.ServerName = name;
            servers.Add(name,asyncServer);
            asyncServer.InstanceHeader = instanceHeader;
            return asyncServer;
        }

        public static AsyncServer GetServer(string name)
        {
            if (servers.ContainsKey(name))
            {
                return servers[name];
            }
            return null;
        }
        
    }
}
