﻿using ET;

namespace ET.Server {
    public static class GamerFactory {
        // 创建玩家对象
        public static Gamer Create(long playerId, long userId, long? id = null) {
            Gamer gamer = ComponentFactory.CreateWithId<Gamer, long>(id ?? IdGenerater.Instance.GenerateId(), userId);
            gamer.PlayerID = playerId;
            return gamer;
        }
    }
}