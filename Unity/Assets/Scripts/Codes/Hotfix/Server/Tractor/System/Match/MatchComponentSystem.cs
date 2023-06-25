using ET;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
namespace ET.Server {
    [ObjectSystem]
    public class MatchComponentUpdateSystem : UpdateSystem<MatchComponent> {
        protected override void Update(MatchComponent self) {
            self.Update();
        }
    }
    [FriendOfAttribute(typeof(ET.Server.MatchComponent))]
    public static class MatchComponentSystem {
        public static void Update(this MatchComponent self) {
            while (true) {
                MatcherComponent matcherComponent = Root.Instance.Scene.GetComponent<MatcherComponent>();
                Queue<Matcher> matchers = new Queue<Matcher>(MatcherComponentSystem.GetAll(matcherComponent));
                MatchRoomComponent roomManager = Root.Instance.Scene.GetComponent<MatchRoomComponent>();
                Room room = roomManager.GetReadyRoom(); // 返回的是：人员不满 < 3 个的一个房间
                if (matchers.Count == 0)
                    // 当没有匹配玩家时直接结束
                    break;
                if (room == null && matchers.Count >= 3) // 分配一个空房间: 当还有一桌匹配玩家且没有可加入房间时使用空房间
                    room = roomManager.GetIdleRoom();
                if (room != null) { // 只要房间不为空，就被强按到这个房间里了，没有任何其它逻辑考量
                    // 当有准备状态房间且房间还有空位时匹配玩家直接加入填补空位
                    while (matchers.Count > 0 && room.Count < 3)
                        self.JoinRoom(room, MatcherComponentSystem.Remove(matcherComponent, matchers.Dequeue().UserID)).Coroutine();
                } else if (matchers.Count >= 3) {
                    // 当还有一桌匹配玩家且没有空房间时创建新房间
                    self.CreateRoomAsync().Coroutine(); // 自己加后面的
                    break;
                } else
                    break;
                // 移除匹配成功玩家
                while (self.MatchSuccessQueue.Count > 0)
                    MatcherComponentSystem.Remove(matcherComponent, matchers.Dequeue().UserID);
            }
        }
        // 创建房间
        // public static async void CreateRoom(this MatchComponent self) { // 禁止返回类型为 void 的异步方法，没弄明白呀。。。
        public static async ETTask CreateRoomAsync(this MatchComponent self) { // 禁止返回类型为 void 的异步方法，没弄明白呀。。。
            if (self.CreateRoomLock) 
                return;
            // 消息加锁，避免因为延迟重复发多次创建消息
            self.CreateRoomLock = true;
            // 发送创建房间消息
            IPEndPoint mapIPEndPoint = Root.Instance.Scene.GetComponent<AllotMapComponent>().GetAddress().GetComponent<InnerConfig>().IPEndPoint;
            // Session mapSession = Root.Instance.Scene.GetComponent<NetInnerComponent>().Get(mapIPEndPoint);
            Session mapSession = NetInnerComponentSystem.Get(Root.Instance.Scene.GetComponent<NetInnerComponent>(), mapIPEndPoint);
            MP2MH_CreateRoom_Ack createRoomRE = await mapSession.Call(new MH2MP_CreateRoom_Req()) as MP2MH_CreateRoom_Ack;  // <<<<<<<<<<<<<<<<<<<< await
            Room room = ComponentFactory.CreateWithId<Room>(createRoomRE.RoomID);
            Root.Instance.Scene.GetComponent<MatchRoomComponent>().Add(room);
            // 解锁
            self.CreateRoomLock = false;
            await ETTask.CompletedTask;
        }
        // 加入房间：逻辑极简单，就只要钱够就可以了。多出了房间服务器。。。。。这里改天再接下去往后看，几个服务器看昏了。。。。。爱表哥，爱生活！！！
        // 刚才改上一个都还没太注意：因为ET7 的重构与底层的重新封装，底层的发送返回消息等逻辑全改变了。所以这里并不期望手动去拿每个系统发送器，而是交由框架来帮处理。明天上午看过再改这类错误。
        public static async ETTask JoinRoom(this MatchComponent self, Room room, Matcher matcher) { // 这里重点看一下：匹配的逻辑是怎样的？
            // 玩家加入房间，移除匹配队列
            self.Playing[matcher.UserID] = room.Id;
            self.MatchSuccessQueue.Enqueue(matcher);
            // 向房间服务器发送玩家进入请求: ET7 重构后，不再每条消息去拿发送器，找例子，找重构后框架里，是如何发送消息的？
            // ActorMessageSender actorProxy = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>().Get(room.Id);  // 【不太明白：】room.Id 也是 actorId ？
            // IResponse response = await actorProxy.Call(new Actor_PlayerEnterRoom_Req() {  // <<<<<<<<<<<<<<<<<<<< 
            //         PlayerID = matcher.PlayerID,
            //             UserID = matcher.UserID,
            //             SessionID = matcher.GateSessionID});
            IResponse response = await ActorMessageSenderComponent.Instance.Call(room.Id, new Actor_PlayerEnterRoom_Req() { // 【不太明白：】room.Id 也是 actorId ？ 
                    PlayerID = matcher.PlayerID,
                        UserID = matcher.UserID,
                        SessionID = matcher.GateSessionID});
            Actor_PlayerEnterRoom_Ack actor_PlayerEnterRoom_Ack = response as Actor_PlayerEnterRoom_Ack;
            // 想要直接用当前的组件，来创建玩家
            // Gamer gamer = (room as Entity).Create(Gamer, actor_PlayerEnterRoom_Ack.GamerID);// Gamer is a type. 不知道说的是什么意思【这里仍然改得不对】
            // Gamer gamer = GamerFactory.Create(matcher.PlayerID, matcher.UserID, actor_PlayerEnterRoom_Ack.GamerID);
            // Gamer gamer = new Gamer(matcher.PlayerID);
            Gamer gamer = room.GetComponent<GamerComponent>().AddChild<Gamer, long>(matcher.PlayerID);
            room.Add(gamer);

            // 向玩家发送匹配成功消息:
            ActorMessageSenderComponent.Instance.Send(gamer.PlayerID, new Actor_MatchSucess_Ntt() { GamerID = gamer.Id });
            // ActorMessageSenderComponent actorProxyComponent = Root.Instance.Scene.GetComponent<ActorMessageSenderComponent>();
            // ActorMessageSender gamerActorProxy = actorProxyComponent.Get(gamer.PlayerID);
            // gamerActorProxy.Send(new Actor_MatchSucess_Ntt() { GamerID = gamer.Id });
            await ETTask.CompletedTask;
        }
    }
} 