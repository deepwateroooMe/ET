namespace ET.Server {
	// 亲爱的表哥的活宝妹，先把它理解成：玩家，与【网关服】的【会话框】。玩家可以借助网关服中转，各种游戏玩家的索求需要等
	[ComponentOf(typeof(Player))]
    public class PlayerSessionComponent : Entity, IAwake {
        private EntityRef<Session> session;
        public Session Session { // 每个，玩家，与分配给它的【网关服】，正常都维护一个通信【会话框】
            get {
                return this.session;
            }
            set {
                this.session = value;
            }
        }
    }
}