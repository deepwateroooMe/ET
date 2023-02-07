using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using Unity.Mathematics;

namespace ET {

    public static class MongoHelper {

        private class StructBsonSerialize<TValue>: StructSerializerBase<TValue> where TValue : struct {
// 对于Struct结构体(抽象提精定义): 序列化 与 反序列化的 特殊定义
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValue value) {
                Type nominalType = args.NominalType;
                IBsonWriter bsonWriter = context.Writer;
// 什么意思呢: 对于数据库中的每一条纪录(Document), 都进行序列化                
                bsonWriter.WriteStartDocument(); // Document: 一个文件 是说 数据库中的一条纪录,但是可以嵌套 !!!
                FieldInfo[] fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic); // 列的集合吗?
                foreach (FieldInfo field in fields) {
                    bsonWriter.WriteName(field.Name);
                    BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value)); 
                } // 每条纪录,都一列一列地对每一列进行序列化 ? 为什么会感觉狠慢呢?因为第一条,条与条之间,列狠可能是不同的,不同长度的,可以嵌套的,就不得不这么做
                bsonWriter.WriteEndDocument();
            }

            public override TValue Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args) { // 把一条纪录反序列化为一个 TValue对象
                // boxing is required for SetValue to work
                object obj = new TValue();
                Type actualType = args.NominalType;
                IBsonReader bsonReader = context.Reader;
                bsonReader.ReadStartDocument();
                while (bsonReader.State != BsonReaderState.EndOfDocument) { // 遍历到当前纪录的最后一列
                    switch (bsonReader.State) {
                    case BsonReaderState.Name: {
                        string name = bsonReader.ReadName(Utf8NameDecoder.Instance);
                        FieldInfo field = actualType.GetField(name);
                        if (field != null) {
                            object value = BsonSerializer.Deserialize(bsonReader, field.FieldType); // 反序列化出这一列的值
                            field.SetValue(obj, value);
                        }
                        break;
                    }
                    case BsonReaderState.Type: {
                        bsonReader.ReadBsonType();
                        break;
                    }
                    case BsonReaderState.Value: {
                        bsonReader.SkipValue();
                        break;
                    }
                    }
                }
                bsonReader.ReadEndDocument();
                return (TValue)obj;
            }
        }

        [StaticField]
        private static readonly JsonWriterSettings defaultSettings = new() { OutputMode = JsonOutputMode.RelaxedExtendedJson };
// 静态构造函数：  所以，这里第一次实例化，就完成了MongaoDB Bson序列化反序列化所必需的所有的必要工作        
        static MongoHelper() { // <<<<<<<<<< 静态构造函数: 将在创建第一个实例或引用任何静态成员之前自动调用静态构造函数。
            // 自动注册IgnoreExtraElements: 支持忽略某些字段序列化 ？ 就是说,某些字段,只要被标注过,不序列化不保存数据库,这里主序列化时就跳过这个字段(列), 这对多版本协议非常有用
            ConventionPack conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
            ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);
            RegisterStruct<float2>(); // <<<<<<<<<< 这里自动注册的意思: 就是方便Bson序列化操作,它定义了序列化Struct 的方法在一个类里,注册是就是,请你认得,这些注册的都按那个定义执行
            RegisterStruct<float3>();
            RegisterStruct<float4>();
            RegisterStruct<quaternion>();
// Bson: 支持类的序列化? 支持类的继承关系 ? protobuf不支持复杂的对象结构（无法使用继承）; 这里是, 支持复杂的继承结构 ?
            Dictionary<string, Type> types = EventSystem.Instance.GetTypes(); // 类继承在反序列化时需要知道所有的父类。这里关心（事件系统，还是说所有的呢？程序集里的所有的类型Type）类的继承关系，与父类的注册
            foreach (Type type in types.Values) {
                if (!type.IsSubclassOf(typeof (Object))) { // 非 Object 子类,跳过
                    continue;
                }
                if (type.IsGenericType) { // 泛型类,跳过
                    continue;
                }
// Bson有个类管理字典：Dictionary<type, BsonClassMap> __classMaps, 确保当前type字典中有过注册（有个键值对）
                BsonClassMap.LookupClassMap(type); // 这里是:工程中 自定义类的类型的注册,以便支持类的序列反序列化 ?
            }
        }

        public static void Init() { // <<<<<<<<<<<<<<<<<<<< 这个方法感觉空空如也,是如何初始化的呢>? 它在创建第一个实例前一定会调用上面的静态构造函数,从而完成初始化
        }

        public static void RegisterStruct<T>() where T : struct {
            BsonSerializer.RegisterSerializer(typeof (T), new StructBsonSerialize<T>()); // <<<<<<<<<< 结构体,序列化的方法定义类 在 文件最开始的地方
        }

        public static string ToJson(object obj) {
            return obj.ToJson(defaultSettings);
        }
        public static string ToJson(object obj, JsonWriterSettings settings) {
            return obj.ToJson(settings);
        }

        public static T FromJson<T>(string str) {
            try {
                return BsonSerializer.Deserialize<T>(str);
            }
            catch (Exception e) {
                throw new Exception($"{str}\n{e}");
            }
        }
        public static object FromJson(Type type, string str) {
            return BsonSerializer.Deserialize(str, type);
        }

        public static byte[] Serialize(object obj) {
            return obj.ToBson();
        }
        public static void Serialize(object message, MemoryStream stream) {
            using (BsonBinaryWriter bsonWriter = new BsonBinaryWriter(stream, BsonBinaryWriterSettings.Defaults)) {
                BsonSerializationContext context = BsonSerializationContext.CreateRoot(bsonWriter);
                BsonSerializationArgs args = default;
                args.NominalType = typeof (object);
                IBsonSerializer serializer = BsonSerializer.LookupSerializer(args.NominalType);
                serializer.Serialize(context, args, message);
            }
        }

        public static object Deserialize(Type type, byte[] bytes) {
            try {
                return BsonSerializer.Deserialize(bytes, type);
            }
            catch (Exception e) {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }
        public static object Deserialize(Type type, byte[] bytes, int index, int count) {
            try {
                using (MemoryStream memoryStream = new MemoryStream(bytes, index, count)) {
                    return BsonSerializer.Deserialize(memoryStream, type);
                }
            }
            catch (Exception e) {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }
        public static object Deserialize(Type type, Stream stream) {
            try {
                return BsonSerializer.Deserialize(stream, type);
            }
            catch (Exception e) {
                throw new Exception($"from bson error: {type.Name}", e);
            }
        }
        public static T Deserialize<T>(byte[] bytes) {
            try {
                using (MemoryStream memoryStream = new MemoryStream(bytes)) {
                    return (T)BsonSerializer.Deserialize(memoryStream, typeof (T));
                }
            }
            catch (Exception e) {
                throw new Exception($"from bson error: {typeof (T).Name}", e);
            }
        }
        public static T Deserialize<T>(byte[] bytes, int index, int count) {
            return (T)Deserialize(typeof (T), bytes, index, count);
        }

        public static T Clone<T>(T t) {
            return Deserialize<T>(Serialize(t));
        }
    }
}