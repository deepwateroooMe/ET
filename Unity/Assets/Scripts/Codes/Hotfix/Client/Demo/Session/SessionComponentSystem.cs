﻿namespace ET.Client {

    // 【销毁系】：只负责用户掉线，或是下线后的自动移除会话框 
    public class SessionComponentDestroySystem: DestroySystem<SessionComponent> {
        protected override void Destroy(SessionComponent self) {
            self.Session?.Dispose();
        }
    }
}
