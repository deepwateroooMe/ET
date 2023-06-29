using ET;
using System.Net;
namespace ET.Server {
    public static class MapHelper {
        // 发送消息给匹配服务器
        public static void SendMessage(IMessage message) {
            GetMapSession().Send(message);
        }
        // 获取匹配服务器连接: 游戏中匹配的逻辑是放在Map 地图服的
        public static Session GetMapSession() {
            IPEndPoint matchIPEndPoint = Root.Instance.Scene.GetComponent<StartConfigComponent>().MatchConfig.GetComponent<InnerConfig>().IPEndPoint;
            Session matchSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), matchIPEndPoint);
            return matchSession;
        }
    }
}
