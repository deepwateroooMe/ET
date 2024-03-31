using System.Runtime.InteropServices;
namespace ET {
    public static class WinPeriod {
        // 一般默认的精度不止1毫秒（不同操作系统有所不同），需要调用timeBeginPeriod与timeEndPeriod来设置精度
        [DllImport("winmm")]
        private static extern void timeBeginPeriod(int t);
        // [DllImport("winmm")]
        // static extern void timeEndPeriod(int t);
		// 上面：C++ NDK? 底层所实现的方法？安卓中是C++ NDK, 谁说C# 这里也，一定是C++ NDK 来着？【TODO】：
		
        public static void Init() {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
                timeBeginPeriod(1);
            }
        }
    }
}