using MongoDB.Driver;

namespace ET.Server {

    // 用来缓存数据: 它是这么明文标注的，不是说代码具有侵入性，不太好吗？要如何改写呢  ？
    [ChildOf(typeof(DBManagerComponent))]
    public class DBComponent: Entity, IAwake<string, string, int>, IDestroy {

        public const int TaskCount = 32;
        public MongoClient mongoClient;
        public IMongoDatabase database;
    }
}