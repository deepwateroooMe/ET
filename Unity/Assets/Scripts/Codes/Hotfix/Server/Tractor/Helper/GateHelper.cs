using ET;
namespace ET.Server {
    public static class GateHelper {

        // 验证 Session 是否合法：感觉这里的验证，不太有效。因为客户端能够用有效会话框必消息过来，除非关卡点儿上全话框过时【这里好像没有超时机制】，一般用户都是有效的
        public static bool SignSession(Session session) {

            SessionUserComponent sessionUser = session.GetComponent<SessionUserComponent>();
            // UserComponent: 是从  Program.cs 里添加的服务器端的用户？服务器端用户远不止一个
            if (sessionUser == null || Game.Scene.GetComponent<UserComponent>().Get(sessionUser.User.UserID) == null) {
                return false;
            }
            return true;
        }
    }
}

