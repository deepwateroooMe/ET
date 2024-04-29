using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
namespace ET.Server {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 【动态路由组件】：
	[ComponentOf(typeof(Scene))]
    public class RouterComponent: Entity, IAwake<IPEndPoint, string>, IDestroy, IUpdate {
		// 对内、对外的、通信信道管道
        public Socket OuterSocket;
        public Socket InnerSocket;
        public EndPoint IPEndPoint = new IPEndPoint(IPAddress.Any, 0);
        public byte[] Cache = new byte[1500]; 

		// 【连接了，但还未验证的】：包括了，连接着的、但是还没经过验证的、路由节点
        public Dictionary<uint, RouterNode> ConnectIdNodes = new Dictionary<uint, RouterNode>();

        // 【验证】过的：已经连接成功的，虽然跟id一样，但是，没有经过验证的不会加到这里
        public Dictionary<uint, RouterNode> OuterNodes = new Dictionary<uint, RouterNode>();
		
        public long LastCheckTime = 0;
    }
}