using System;
using UnityEngine;

namespace ET.Client {

    [FriendOf(typeof(OperaComponent))] // OperaComponent: 读到过网上介绍说,它是全权负责监控用户的鼠标操作,以便控制玩家的位移步移等
    public static class OperaComponentSystem {

        [ObjectSystem]
        public class OperaComponentAwakeSystem : AwakeSystem<OperaComponent> {

            protected override void Awake(OperaComponent self) {
                self.mapMask = LayerMask.GetMask("Map");
            }
        }

        [ObjectSystem]
        public class OperaComponentUpdateSystem : UpdateSystem<OperaComponent> {

            protected override void Update(OperaComponent self) {
                if (Input.GetMouseButtonDown(1)) { // 左键 右键 ?
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, 1000, self.mapMask)) {
                        C2M_PathfindingResult c2MPathfindingResult = new C2M_PathfindingResult(); // new 个新的
                        c2MPathfindingResult.Position = hit.point; // 赋值, 玩家的目标位置, 这里就涉及到一些寻址之类的东西 
// 这里就是要把当前玩家的目标地址发给服务器不:  它是通过Actor系统背着instanceID发消息（当然就可以跨进程了）
                        self.ClientScene().GetComponent<SessionComponent>().Session.Send(c2MPathfindingResult); 
                    }
                }
// 这里是能够,实时加载热更新程序包吗? 那么现在的问题就变成是,每当有玩家的位置要改变,就会直接构建生成热更新包吗?谁在什么时候构建或提供了这橷包?
                if (Input.GetKeyDown(KeyCode.R)) { 
                    CodeLoader.Instance.LoadHotfix();
                    EventSystem.Instance.Load();
                    Log.Debug("hot reload success!");
                }
            
                if (Input.GetKeyDown(KeyCode.T)) {
                    C2M_TransferMap c2MTransferMap = new C2M_TransferMap();
// 这里是大速度切换地图, 还是随玩家位移小速度慢移地图背景?
// 这里更像是：场景里的背景地图要大切换了，先弄个空窗口，然后申请服务器说，你给我下载到的新地图就放这个容器里，好了通知我好来读，跟我被录取来读WSU的计算机博士学位是一样一样滴￣￣！！！                    
                    self.ClientScene().GetComponent<SessionComponent>().Session.Call(c2MTransferMap).Coroutine(); 
                }
            }
        }
    }
}
