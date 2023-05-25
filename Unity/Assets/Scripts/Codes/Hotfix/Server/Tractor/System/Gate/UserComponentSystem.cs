using System.Collections.Generic;
using System.Linq;
namespace ET.Server {
    public static class UserComponentSystem { // User对象管理组件生成系
        [ObjectSystem]
        public class UserComponentAwakeSystem : AwakeSystem<UserComponent> { // 可以找个例子出来看一下
            protected override void Awake(UserComponent self, long id) {
            }
        }
        // 添加User对象
        public static void Add(UserComponent self, User user) {
            self.idUsers.Add(user.UserID, user);
        }
        // 获取User对象
        public static User Get(UserComponent self, long id) {
            self.idUsers.TryGetValue(id, out User gamer);
            return gamer;
        }
        // 移除User对象
        public static void Remove(UserComponent self, long id) {
            self.idUsers.Remove(id);
        }
        // User对象总数量
        public static int Count {
            get {
                return self.idUsers.Count;
            }
        }
        // 获取所有User对象
        public static User[] GetAll() {
            return self.idUsers.Values.ToArray();
        }
    }
}
