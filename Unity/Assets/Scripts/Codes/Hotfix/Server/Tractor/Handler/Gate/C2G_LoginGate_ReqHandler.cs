using System;
using ET;
using System.Net;
namespace ET.Server {
    // 这个类，不想要了：因为它跟框架原本的是重复的。去查，目前也没什么真正用到它的地方
    // [MessageHandler(SceneType.Gate)]
    // public class C2G_LoginGate_ReqHandler : AMRpcHandler<C2G_LoginGate_Req, G2C_LoginGate_Ack> {
    //     protected override async ETTask Run(Session session, C2G_LoginGate_Req message, G2C_LoginGate_Ack response) {
    //         LandlordsGateSessionKeyComponent landlordsGateSessionKeyComponent = session.DomainScene().GetComponent<LandlordsGateSessionKeyComponent>();
    //         long userId = LandlordsGateSessionKeyComponentSystem.Get(landlordsGateSessionKeyComponent, message.Key); // 出错：这里像是，我的组件，与它热更新层的生成系没能联起来，那就是Awake() 系统的原因了？先放一下
    //         // 验证登录Key是否正确
    //         if (userId == 0) {
    //             response.Error = ErrorCode.ERR_ConnectGateKeyError;
    //             // reply(response);
    //             return;
    //         }
    //         // Key过期
    //         LandlordsGateSessionKeyComponentSystem.Remove(landlordsGateSessionKeyComponent, message.Key);
    //         // 创建User对象
    //         UserComponent userComponent = Root.Instance.Scene.GetComponent<UserComponent>(); // 不知道这么拿，对不对，运行时测试
    //         User user = userComponent.AddChild<User, long>(userId);
    //             // UserFactory.Create(userId, session.InstanceId);// 用 UserComponent: 对，用这个，就是去拿这个管理组件的 reference
    //         await user.AddComponent<MailBoxComponent>().AddLocation(); // 为【用户】添加了邮箱和位置信息。那么这个用户就可以收发消息了
    //         // 添加User对象关联到Session上
    //         session.AddComponent<SessionUserComponent>().User = user;
    //         await session.AddComponent<MailBoxComponent>().AddLocation(); // 添加消息转发组件

    //         // 下面：拿【匹配服】的地址。我记得这个哪里有个例子来着，大概登录的时候，原框架自带的例子
    //         // 【ET7 框架自带的】：LoginHelper, 登录时，会随机分配一个Realm 服。那么，为什么还要保留当前这个方法呢？先按照原例子来写
    //         // 【注意：】用户第一次登录的时候，【随机分配一个网关服，随机分配一个注册登录服】
    //         StartConfigComponent config = Root.Instance.Scene.GetComponent<StartConfigComponent>(); // 这个组件被重构了，需要去查
    //         // 这里涉及到另一块：如何发请求消息，新封装是怎么的，今天暂时不看这一块儿k
    //         IPEndPoint realmIPEndPoint = config.RealmConfig.GetComponent<InnerConfig>().IPEndPoint;// 这里拿这个地址的方法不对
    //         Session realmSession = NetInnerComponentSystem.Get(session.DomainScene().GetComponent<NetInnerComponent>(), realmIPEndPoint);
    //         await realmSession.Call(new G2R_PlayerOnline_Req() { UserID = userId, GateAppID = config.StartConfig.AppId });

    //         response.PlayerID = user.InstanceId;
    //         response.UserID = user.UserID;
    //     }
    // }
}