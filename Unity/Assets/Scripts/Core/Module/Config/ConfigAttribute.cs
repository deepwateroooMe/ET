using System;
namespace ET {
	// 因为ConfigAttribute 继承自 BaseAttribute, 所以程序域加载时，EventSystem 事件机制，一定也会扫到这个个性化标签
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigAttribute: BaseAttribute {
    }
}