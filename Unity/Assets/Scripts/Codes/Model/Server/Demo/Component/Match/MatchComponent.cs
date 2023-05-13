using System.Collections.Generic;
namespace ET {

    // 匹配组件，匹配逻辑在MatchComponentSystem扩展. 这里是处理匹配的组件，与 Matcher 被匹配者相区分开来
    public class MatchComponent : Component {

        // 游戏中匹配对象列表：值是 roomId
        public readonly Dictionary<long, long> Playing = new Dictionary<long, long>();

        // 匹配成功队列
        public readonly Queue<Matcher> MatchSuccessQueue = new Queue<Matcher>();

        // 创建房间消息加锁，避免因为延迟重复发多次创建房间消息
        public bool CreateRoomLock { get; set; }
    }
}
