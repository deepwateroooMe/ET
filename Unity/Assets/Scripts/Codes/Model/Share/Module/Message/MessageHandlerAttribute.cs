<<<<<<< HEAD
﻿namespace ET {
    public class MessageHandlerAttribute: BaseAttribute {
=======
﻿using System;

namespace ET
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlerAttribute: BaseAttribute
    {
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
        public SceneType SceneType { get; }

        public MessageHandlerAttribute(SceneType sceneType) {
            this.SceneType = sceneType;
        }
    }
}