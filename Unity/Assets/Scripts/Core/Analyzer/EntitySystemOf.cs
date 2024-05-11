using System;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
    // 标记Entity的System静态类 用于自动生成System函数【源】
	// 原来它是可以自动、如一键生成 System 类的配置
    [AttributeUsage(AttributeTargets.Class)]
    public class EntitySystemOfAttribute: BaseAttribute {
        public Type type;
        // 标记Entity的System静态类 用于自动生成System函数
        // <param name="type">Entity类型</param>
        // <param name="ignoreAwake">是否忽略生成AwakeSystem</param>
        public EntitySystemOfAttribute(Type type, bool ignoreAwake = false) {
            this.type = type;
        }
    }
    // 标记LSEntity的System静态类 用于自动生成System函数【源】：没想明白，LS 这里是什么意思【TODO】：
    [AttributeUsage(AttributeTargets.Class)]
    public class LSEntitySystemOfAttribute: BaseAttribute {
        public Type type;
        // 标记LSEntity的System静态类 用于自动生成System函数
        // <param name="type">LSEntity类型</param>
        // <param name="ignoreAwake">是否忽略生成AwakeSystem</param>
        public LSEntitySystemOfAttribute(Type type, bool ignoreAwake = false) {
            this.type = type;
        }
    }
}