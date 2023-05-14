namespace ET {

    namespace EventType {
        public struct SceneChangeStart {
        }
        public struct SceneChangeFinish {
        }

        public struct AfterCreateClientScene {
        }
        public struct AfterCreateCurrentScene {
        }

        public struct AppStartInitFinish {
        }
        public struct LoginFinish {
        }
        public struct ModeSelected { // 匹配玩家成功
        }
        // public struct EnterMapFinish {
        public struct EnterRoomFinish {
        }
        public struct AfterUnitCreate {
            public Unit Unit;
        }
    }
}