using MongoDB.Driver;
namespace ET.Server {
    // 用来缓存数据
    [ChildOf(typeof(DBManagerComponent))]
    public class DBComponent: Entity, IAwake<string, string, int>, IDestroy {
        public const int TaskCount = 32;
        public MongoClient mongoClient;
        public IMongoDatabase database;
    }
}