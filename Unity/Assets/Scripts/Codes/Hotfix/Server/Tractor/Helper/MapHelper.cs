using ET;
using System.Net;
namespace ET.Server {
    public static class MapHelper {

        // 发送消息给匹配服务器
        public static void SendMessage(IMessage message) {
            GetMapSession().Send(message);
        }
        // 获取匹配服务器连接【源】: 先前不知道自己写的是什么乱注解。如果自己的【匹配服】按小区管理，那么任何场景拿到的都是先前随机分配，或是按小区分配来的匹配服。唯一确定(的前提是，四大单例管理类只在一个物理机上，可是感觉不对，应该属于整个服务端 )，哪里要记吗，也可以不用
        public static Session GetMapSession() { // 感觉这里是，方法的名字起得奇怪，为什么叫去拿【地图服】会话框呢？
            // IPEndPoint matchIPEndPoint = Root.Instance.Scene.GetComponent<StartConfigComponent>().MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
            Session matchSession = NetInnerComponent.Instance.Get(StartSceneConfigCategory.Instance.Match.StartProcessConfig.SceneId);
            // 上面两行的过程，重构成为：从本台机制的配置里，去拿单例管理类里的【匹配服】的地址，再作其它
// StartSceneConfigCategory.Instance.Matchs // 我这里可以拿到一个链表，我还是需要先去把 protobuf-partial 这个东西给弄明白。地【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】 
            return matchSession;
            // return null;
        }
    }
}
