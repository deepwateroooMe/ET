using System.Collections.Generic;
namespace ET.Client {
    // 管理Scene上的UI
    [ComponentOf(typeof(Scene))]
    public class UIComponent: Entity, IAwake { // 这里，可能缺少了生成系文件
        public Dictionary<string, UI> UIs = new Dictionary<string, UI>();
    }
}