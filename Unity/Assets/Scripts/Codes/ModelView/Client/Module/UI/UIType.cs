using System;
using System.Collections.Generic;
namespace ET.Client {
    public static class UIType {

	    public const string Root = "Root";
	    public const string UILoading = "UILoading";
	    public const string UILogin = "UILogin";
	    public const string UILobby = "UILobby";
	    public const string UIHelp = "UIHelp";

        // 因为命名空间的合并，客户端只有 ET.Client, 我应该就能再在热更新域里定义拖拉机游戏专用类型了
        // public const string TractorLogin = "TractorLogin";
        // public const string TractorLobby = "TractorLobby";
        public const string TractorRoom = "TractorRoom";
        public const string TractorInteraction = "TractorInteraction";
        public const string TractorEnd = "TractorEnd";

    }
}