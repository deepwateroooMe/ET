using System.Collections.Generic;
namespace ET.Client {
	// 
    // 管理Scene上的UI
    [ComponentOf(typeof(Scene))]
    public class UIComponent: Entity, IAwake {
        public Dictionary<string, UI> UIs = new Dictionary<string, UI>();
    }
}