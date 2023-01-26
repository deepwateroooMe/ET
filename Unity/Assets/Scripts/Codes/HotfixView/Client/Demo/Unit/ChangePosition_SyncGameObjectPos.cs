using UnityEngine;

namespace ET.Client {

    // 这两个类比较好玩: 可能游戏中会涉及玩家步移与转身等,定义到了两个类里,可用于同一类玩家类型身上 ?
    [Event(SceneType.Current)]
    public class ChangePosition_SyncGameObjectPos: AEvent<EventType.ChangePosition> {

        protected override async ETTask Run(Scene scene, EventType.ChangePosition args) {
            Unit unit = args.Unit;
            GameObjectComponent gameObjectComponent = unit.GetComponent<GameObjectComponent>();
            if (gameObjectComponent == null) {
                return;
            }
            Transform transform = gameObjectComponent.GameObject.transform;
            transform.position = unit.Position;
            await ETTask.CompletedTask;
        }
    }
}