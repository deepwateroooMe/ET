using ET;
using ET.Server;
using System.Threading.Tasks;
namespace ETHotfix {

    [ActorMessageHandler(SceneType.Gate)]
    public class Actor_MatchSucess_NttHandler : AMActorHandler<User, Actor_MatchSucess_Ntt> {

        protected override void Run(User user, Actor_MatchSucess_Ntt message) {
            user.IsMatching = false;
            user.ActorID = message.GamerID;
            Log.Info($"玩家{user.UserID}匹配成功");
        }
    }
}
