using MongoDB.Bson.Serialization.Attributes;
using ET;

namespace ET {
    // 用户信息
    [BsonIgnoreExtraElements]
    [ChildOf(typeof(Session))]
    public class UserInfo : Entity, IAwake<long> {
        public long id;
        // 昵称
        public string NickName { get; set; }
        // 胜场
        public int Wins { get; set; }
        // 负场
        public int Loses { get; set; }
        // 余额
        public long Money { get; set; }
    }
}