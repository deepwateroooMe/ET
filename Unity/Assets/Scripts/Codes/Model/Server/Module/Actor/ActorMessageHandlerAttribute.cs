<<<<<<< HEAD
﻿namespace ET.Server {
    public class ActorMessageHandlerAttribute: BaseAttribute {
=======
﻿using System;

namespace ET.Server
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ActorMessageHandlerAttribute: BaseAttribute
    {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
        public SceneType SceneType { get; }
        public ActorMessageHandlerAttribute(SceneType sceneType) {
            this.SceneType = sceneType;
        }
    }
}