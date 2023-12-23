using UnityEngine;
namespace ET {
    public enum CodeMode {
        Client = 1,
        Server = 2,
        ClientServer = 3,
    }
	// 菜单下的一个可点击配置 
[CreateAssetMenu(menuName = "ET/CreateGlobalConfig", fileName = "GlobalConfig", order = 0)]
    public class GlobalConfig: ScriptableObject {
        public CodeMode CodeMode;
    }
}