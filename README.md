#xx-tcp-server
## 说明


1. 实现了Server监听和简单的事件处理
2. 兼容长连接和短连接
3. 支持同时启动多个Server（监听不同的端口）
4. 并没有实现解析结构体的工作（下一步可能会做）
5. 依赖于Log4Net（后面可能去掉依赖，采用封装，为了适应每个项目使用的LOG）
6. 可能不太适用于大量IO的项目，本项目为了主要为了实现针对数量不多TCP通信（后续将进行压力测试）

## 快速开始
``` C#
//创建一个Server，二个参数：
// name：用于区别多个server的标志。
// createHeader：实例化你用到的Header继承于xxHeader
AsyncServer server = xxServer.CreateServer("test1", CreateHeader);
server.printReceiveHex = true; //开启打印接收到数据的Hex
server.printSendHex = true; //开启打印发送数据的Hex
server.HeaderLength = 8; // 协议中消息头的长度。这个必须设置
server.MainNotify += MainHandler; //一个消息接收完之后的事件处理。
server.Start(8001);
```
## Demo
可以运行ServerTest中的start和ClientTest中的start
![服务端运行图](http://i.imgur.com/hvDg01R.png)

![客户端运行图](http://i.imgur.com/Ek95dHq.png)
## TODO
1. 实现UDP 的Server
2. 压力测试