using MongoDB.Bson.Serialization.Attributes;
namespace ET {

    // 账号信息
    [BsonIgnoreExtraElements]
    public class AccountInfo : Entity, IAwake<long> {
        public long id;
        // 用户名
        public string Account { get; set; }
        // 密码
        public string Password { get; set; }
    }
}