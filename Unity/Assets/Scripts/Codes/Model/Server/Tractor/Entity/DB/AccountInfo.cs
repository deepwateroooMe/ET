using MongoDB.Bson.Serialization.Attributes;
namespace ET.Server {
    // 账号信息: 这里把自己看得稀里糊涂，只能把它们先弄成是，小行星生成系。。。它是双端可以互相传递的登录帐户信息，感觉弄成小行星生成系，有点儿不太对。。。
    [BsonIgnoreExtraElements]
    [ChildOf(typeof(Session))]
    public class AccountInfo : Entity, IAwake<long> {
        public long id;
        // 用户名
        public string Account { get; set; }
        // 密码
        public string Password { get; set; }
    }
}
