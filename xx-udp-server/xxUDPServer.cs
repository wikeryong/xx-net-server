using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace xx_udp_server
{
    public class xxUDPServer
    {
        /// <summary>
        /// 接收消息委托
        /// </summary>
        /// <param name="body"></param>
        public delegate void EventHandler(xxUDPAsyncServer server, xxUDPEventArgs args);


        private static IDictionary<string, xxUDPAsyncServer> servers = new Dictionary<string, xxUDPAsyncServer>();

        public static xxUDPAsyncServer CreateUDPServer(string name,int port)
        {
            xxUDPAsyncServer server = new xxUDPAsyncServer(port);
            servers.Add(name,server);
            return server;
        }

        public static xxUDPAsyncServer GetServer(string name)
        {
            if (servers.ContainsKey(name))
            {
                return servers[name];
            }
            return null;
        }
    }

    public enum UDPServerType
    {
        ASCII,BYTE,
    }
}
