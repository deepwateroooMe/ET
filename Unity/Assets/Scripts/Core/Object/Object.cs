namespace ET {
    public abstract class Object {
        public override string ToString() {
            return JsonHelper.ToJson(this);
        }
        public string ToJson() { // 这个是？MongoDB 要用到的
            return MongoHelper.ToJson(this);
        }
        public byte[] ToBson() {
            return MongoHelper.Serialize(this);
        }
    }
}