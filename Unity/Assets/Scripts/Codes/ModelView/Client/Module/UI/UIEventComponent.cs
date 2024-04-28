using System;
using System.Collections.Generic;
using UnityEngine;
namespace ET.Client {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	
    // 管理所有UI GameObject【源】： me- 以及UI 组件的添加或删除后的回调管理，借助字典的值 AUIEvent 抽象类的封装
    [ComponentOf(typeof(Scene))]
    public class UIEventComponent: Entity, IAwake {
        [StaticField]
        public static UIEventComponent Instance;
        public Dictionary<string, AUIEvent> UIEvents = new Dictionary<string, AUIEvent>();
        public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
    }
}