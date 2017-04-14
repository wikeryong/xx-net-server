using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using xx_udp_server;

namespace xx_udp_test
{
    class Program
    {
        static void Main(string[] args)
        {
            xxUDPAsyncServer server = xxUDPServer.CreateUDPServer("wiker", 8002);
            //下面的事件都是可以用的
            //server.CompletedSend += 发送完成事件
            //server.PrepareSend += 发送数据之前的事件
            //server.NetError += 网络接收错误事件
            //server.OtherException += 其它异常事件
            server.DataReceived += (asyncServer, arg) =>
            {
                xxStrBody strBody = (xxStrBody)arg.UdpBody;
                Console.WriteLine("Server 接收："+strBody.Content);
                asyncServer.Send(arg.State.remote, "Hello client,Server time:"+DateTime.Now);
            };
            server.Start();
            Thread.Sleep(1000000);
        }
    }
}
