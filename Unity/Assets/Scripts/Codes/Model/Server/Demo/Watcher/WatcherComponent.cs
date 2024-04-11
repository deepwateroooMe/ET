using System.Collections.Generic;
using System.Diagnostics;
namespace ET.Server {
    [ComponentOf(typeof(Scene))]
    public class WatcherComponent: Entity, IAwake, IDestroy {
        public static WatcherComponent Instance { get; set; }
		// 字典：用来管理，这台物理机上的多个其它进程
        public readonly Dictionary<int, Process> Processes = new Dictionary<int, Process>();
    }
}