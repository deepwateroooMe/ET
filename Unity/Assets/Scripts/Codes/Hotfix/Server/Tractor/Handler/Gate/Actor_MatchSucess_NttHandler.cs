using System.Threading.Tasks;
using ET;
namespace ET.Server {

    [ActorMessageHandler(SceneType.Gate)]
    public class Actor_MatchSucess_NttHandler : AMActorHandler<User, Actor_MatchSucess_Ntt> {
        // 感觉下面的这个方法没有写完整：不知道什么时候源码变成这样的。就需要自己再去对比一遍，校正一下，再去改可能存在的错误
        protected override void Run(User user, Actor_MatchSucess_Ntt message) {
            user.IsMatching = false;
            user.ActorID = message.GamerID;
            Log.Info($"玩家{user.UserID}匹配成功");
        }
    }
}
