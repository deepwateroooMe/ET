using ET;
namespace ET.Client {

    [MessageHandler]
    public class Actor_GamerMoneyLess_NttHandler : AMHandler<Actor_GamerMoneyLess_Ntt> {

        protected override async ETTask Run(Session session, Actor_GamerMoneyLess_Ntt message) {
            long userId = ClientComponent.Instance.LocalPlayer.UserID;
            if (message.UserID == userId) {
                // 余额不足时退出房间
                UI room = session.DomainScene().GetComponent<UIComponent>().Get(UIType.TractorRoom);
                await room.GetComponent<TractorRoomComponent>().OnQuit();
            }
            await ETTask.CompletedTask;
        }
    }
}
