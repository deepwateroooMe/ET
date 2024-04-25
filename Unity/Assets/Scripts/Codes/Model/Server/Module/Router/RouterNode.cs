using System.Net;
namespace ET.Server {
    public enum RouterStatus {
        Sync,
        Msg,
    }
	[ChildOf(typeof(RouterComponent))]
    public class RouterNode: Entity, IDestroy, IAwake { // 一个节点：一个路由器？
        public uint ConnectId;
        public string InnerAddress;
		// 对内、对外、 sync 端口？
        public IPEndPoint InnerIpEndPoint;
        public IPEndPoint OuterIpEndPoint;
        public IPEndPoint SyncIpEndPoint;
		// 对内、对外。。
        public uint OuterConn;
        public uint InnerConn;
		// 最后、发送、接收、时间
        public long LastRecvOuterTime;
        public long LastRecvInnerTime;
        public int RouterSyncCount; // 什么次数
        public int SyncCount; // ？
#region 限制外网消息数量，一秒最多50个包
        public long LastCheckTime;
        public int LimitCountPerSecond;
#endregion
        public RouterStatus Status;
    }
}