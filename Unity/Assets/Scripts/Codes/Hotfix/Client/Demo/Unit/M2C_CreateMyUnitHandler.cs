namespace ET.Client {
	
    [MessageHandler(SceneType.Client)]
    public class M2C_CreateMyUnitHandler : AMHandler<M2C_CreateMyUnit> {

        protected override async ETTask Run(Session session, M2C_CreateMyUnit message) { // 开协程锁：【客户端】切换场景，等Unit 创建，现可以继续往下执行
            // 通知场景切换协程继续往下走：就是先前M2M_UnitTransferRequestHandler 【地图服、处理逻辑】的类里，地图服命令客户端开始切场景；客户端接命令后，等地图服创建Unit
            session.DomainScene().GetComponent<ObjectWait>().Notify(new Wait_CreateMyUnit() {Message = message});
            await ETTask.CompletedTask;
        }
    }
}
