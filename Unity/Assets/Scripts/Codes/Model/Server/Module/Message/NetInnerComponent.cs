using System.Net;
namespace ET.Server {
    public struct ProcessActorId {
        public int Process;
        public long ActorId; // 这时指的是，当前，服务器或是客户端，它自己的 ActorId
        public ProcessActorId(long actorId) {
            InstanceIdStruct instanceIdStruct = new InstanceIdStruct(actorId); //actorId 带着结构体三个变量的信息，自动拆解成结构体
            this.Process = instanceIdStruct.Process;
            instanceIdStruct.Process = Options.Instance.Process; // 更新，当前进程的类型
            this.ActorId = instanceIdStruct.ToLong(); // 成员变量更新过，同步成最新
        }
    }
    
    public struct NetInnerComponentOnRead {
        public long ActorId;
        public object Message;
    }
    
    [ComponentOf(typeof(Scene))]
    public class NetInnerComponent: Entity, IAwake<IPEndPoint>, IAwake, IDestroy {
        public int ServiceId;
        
        public NetworkProtocol InnerProtocol = NetworkProtocol.KCP;
        [StaticField]
        public static NetInnerComponent Instance;
    }
}