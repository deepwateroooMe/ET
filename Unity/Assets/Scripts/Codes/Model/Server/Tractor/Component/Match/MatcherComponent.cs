using System.Linq;
using System.Collections.Generic;
namespace ET.Server {

    // 匹配对象管理组件
    public class MatcherComponent : Component {

        private readonly Dictionary<long, Matcher> matchers = new Dictionary<long, Matcher>();
        // 匹配对象数量
        public int Count { get { return matchers.Count; } }

        // 添加匹配对象
        public void Add(Matcher matcher) {
            this.matchers.Add(matcher.UserID, matcher);
        }
        // 获取匹配对象
        public Matcher Get(long id) {
            this.matchers.TryGetValue(id, out Matcher matcher);
            return matcher;
        }
        // 获取所有匹配对象
        public Matcher[] GetAll() {
            return this.matchers.Values.ToArray();
        }
        // 移除匹配对象并返回
        public Matcher Remove(long id) {
            Matcher matcher = Get(id);
            this.matchers.Remove(id);
            return matcher;
        }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            foreach (var matcher in this.matchers.Values) {
                matcher.Dispose();
            }
        }
    }
}
