using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace ET.Server {
    [ComponentOf(typeof(Scene))]
    public class RouterComponent: Entity, IAwake<IPEndPoint, string>, IDestroy, IUpdate {
        public Socket OuterSocket;// 对外业务端口
        public Socket InnerSocket;// 对内业务端口
        public EndPoint IPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public byte[] Cache = new byte[1500]; // 路由器缓冲区？
        // 下面的注释是【框架开发者注的】：但仍没看明白，两个字典有什么不同？
        public Dictionary<uint, RouterNode> ConnectIdNodes = new Dictionary<uint, RouterNode>();
        // 已经连接成功的，虽然跟id一样，但是没有经过验证的不会加到这里
        public Dictionary<uint, RouterNode> OuterNodes = new Dictionary<uint, RouterNode>();
        public long LastCheckTime = 0;
    }
}
