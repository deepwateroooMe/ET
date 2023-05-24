using System.Collections.Generic;
using System.Linq;
namespace ET.Server {
    public class UserComponent : Entity { // User对象管理组件
        private readonly Dictionary<long, User> idUsers = new Dictionary<long, User>();
        // 添加User对象
        public void Add(User user) {
            this.idUsers.Add(user.UserID, user);
        }
        // 获取User对象
        public User Get(long id) {
            this.idUsers.TryGetValue(id, out User gamer);
            return gamer;
        }
        // 移除User对象
        public void Remove(long id) {
            this.idUsers.Remove(id);
        }
        // User对象总数量
        public int Count {
            get {
                return this.idUsers.Count;
            }
        }
        // 获取所有User对象
        public User[] GetAll() {
            return this.idUsers.Values.ToArray();
        }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            foreach (User user in this.idUsers.Values) {
                user.Dispose();
            }
        }
    }
}
