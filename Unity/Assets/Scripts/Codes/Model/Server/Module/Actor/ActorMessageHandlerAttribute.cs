using System;
namespace ET.Server {
	// 框架里封装的标签系：四大程序域启动的时候，加载程序集，直接扫描各种标签
    [AttributeUsage(AttributeTargets.Class)]
    public class ActorMessageHandlerAttribute: BaseAttribute {
        public SceneType SceneType { get; }
        public ActorMessageHandlerAttribute(SceneType sceneType) {
            this.SceneType = sceneType;
        }
    }
}