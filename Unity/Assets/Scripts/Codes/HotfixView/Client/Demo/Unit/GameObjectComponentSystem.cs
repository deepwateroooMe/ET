using System;

namespace ET.Client {

    public static class GameObjectComponentSystem {

// 只涉及控件的销毁 
        [ObjectSystem]
        public class DestroySystem: DestroySystem<GameObjectComponent> {

            protected override void Destroy(GameObjectComponent self) {
                UnityEngine.Object.Destroy(self.GameObject);
            }
        }
    }
}