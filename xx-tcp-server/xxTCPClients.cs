using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace xx_tcp_server
{
    public class xxTCPClients
    {
        private static IDictionary<string, xxClient> clientSockets = new Dictionary<string, xxClient>();

        public static void Add(string sessionId, xxClient socket)
        {
            clientSockets.Add(sessionId,socket);
        }

        public static int ClientCount
        {
            get { return clientSockets.Count; }
        }

        public static xxClient GetClient(string sessionId)
        {
            if (clientSockets.ContainsKey(sessionId))
            {
                return clientSockets[sessionId];
            }
            return null;
        }

        public static string AssignSessionId()
        {
            return Guid.NewGuid().ToString();
        }

        public static void Close(string sessionId)
        {
            if (clientSockets.ContainsKey(sessionId))
            {
                clientSockets[sessionId].socket.Shutdown(SocketShutdown.Both);
                clientSockets[sessionId].socket.Close();
                clientSockets.Remove(sessionId);
            }

        }
    }

    public class xxClient
    {
        public string SessionId { get; set; }
        public Socket socket { get; set; }
    }
}
