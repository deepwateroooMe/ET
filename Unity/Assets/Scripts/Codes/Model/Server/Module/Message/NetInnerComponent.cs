using System.Net;

namespace ET.Server {

    public struct ProcessActorId {
        public int Process;
        public long ActorId;

        public ProcessActorId(long actorId) {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId);
            this.Process = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process;
            this.ActorId = instanceIdStruct.ToLong();
        }
    }
    
    public struct NetInnerComponentOnRead {
        public long ActorId;
        public object Message;
    }

    // 内网通信组件 NetInnerComponent
    // 顾名思义 这个是内网通信用的 添加组件时候传入了内网的地址
    [ComponentOf(typeof(Scene))]
    public class NetInnerComponent: Entity, IAwake<IPEndPoint>, IAwake, IDestroy {
        public int ServiceId;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
        [StaticField]
        public static NetInnerComponent Instance;
    }
}