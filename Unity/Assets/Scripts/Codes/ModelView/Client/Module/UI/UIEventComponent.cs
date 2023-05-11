using System;
using System.Collections.Generic;
using UnityEngine;
namespace ET.Client {

    // 管理所有UI GameObject
    [ComponentOf(typeof(Scene))]
    public class UIEventComponent: Entity, IAwake {
        [StaticField]
        public static UIEventComponent Instance;
        
        public Dictionary<string, AUIEvent> UIEvents = new Dictionary<string, AUIEvent>();
        public Dictionary<int, Transform> UILayers = new Dictionary<int, Transform>();
    }
}