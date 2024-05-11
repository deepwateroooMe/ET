using System;
namespace ET {
    // 添加该标记的类或结构体禁止使用new关键字构造对象【源】
	// 【TODO】：知道，它说的大致意思，可是是怎么实现的呢？怎么自动禁止的？
    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct,Inherited = true)]
    public class DisableNewAttribute : Attribute {
    }
}
