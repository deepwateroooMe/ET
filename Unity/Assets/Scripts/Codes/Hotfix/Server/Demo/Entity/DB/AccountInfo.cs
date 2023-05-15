using MongoDB.Bson.Serialization.Attributes;
namespace ET.Server {

    // 账号信息
    [BsonIgnoreExtraElements]
    public class AccountInfo : Entity {
        // 用户名
        public string Account { get; set; }
        // 密码
        public string Password { get; set; }
    }
}
