using System.Linq;
using System.Collections.Generic;
namespace ET.Server {
    // 匹配对象管理组件
    [FriendOf(typeof(MatcherComponent))]
    public static class MatcherComponentSystem {
        // [ObjectSystem]
        // public class MatcherComponentSystem : UpdateSystem<MatchComponent> {
        //     protected override void Update(MatchComponent self) {
        //         self.Update();
        //     }
        // }
        // 匹配对象数量
        // public static int Count { get { return matchers.Count; } } // 它不允许这么写，就换个写法来写呀
        public static int Count(MatcherComponent self) {
            return self.matchers.Count;
        }
        // 添加匹配对象
        public static void Add(MatcherComponent self, Matcher matcher) {
            self.matchers.Add(matcher.UserID, matcher);
        }
        // 获取匹配对象
        public static Matcher Get(MatcherComponent self, long id) {
            self.matchers.TryGetValue(id, out Matcher matcher);
            return matcher;
        }
        // 获取所有匹配对象
        public static Matcher[] GetAll(MatcherComponent self) {
            return self.matchers.Values.ToArray();
        }
        // 移除匹配对象并返回
        public static Matcher Remove(MatcherComponent self, long id) {
            Matcher matcher = Get(self, id);
            self.matchers.Remove(id);
            return matcher;
        }
    }
}