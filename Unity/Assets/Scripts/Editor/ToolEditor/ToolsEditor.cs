using UnityEditor;

namespace ET
{
    public static class ToolsEditor {
		//./Tool 工具：应该是，服务端构建的一个可执行文件。暂时不去找了，改天再找【TODO】：
        public static void ExcelExporter()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool"; // 现在，亲爱的表哥的活宝妹，是不知道 ./Tool 这个可执行文件，是怎么构建出来的？可以找到和理解。【TODO】：
#else
            const string tools = ".\\Tool.exe";
#endif
            ShellHelper.Run($"{tools} --AppType=ExcelExporter --Console=1", "../Bin/");
        }
        
        public static void Proto2CS()
        {
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            const string tools = "./Tool";
#else
            const string tools = ".\\Tool.exe";
#endif
            ShellHelper.Run($"{tools} --AppType=Proto2CS --Console=1", "../Bin/");
        }
    }
}