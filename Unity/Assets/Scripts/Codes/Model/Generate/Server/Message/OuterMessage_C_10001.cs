using ET;
using ProtoBuf;
using System.Collections.Generic;

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;

namespace ET
{
	[Message(OuterMessage.HttpGetRouterResponse)]
	[ProtoContract]
	public partial class HttpGetRouterResponse: ProtoObject
	{
		[ProtoMember(1)]
		public List<string> Realms { get; set; }

		[ProtoMember(2)]
		public List<string> Routers { get; set; }

		[ProtoMember(3)]
		public List<string> Matchs { get; set; }

	}

	[Message(OuterMessage.RouterSync)]
	[ProtoContract]
	public partial class RouterSync: ProtoObject
	{
		[ProtoMember(1)]
		public uint ConnectId { get; set; }

		[ProtoMember(2)]
		public string Address { get; set; }

	}

	[Message(OuterMessage.C2M_TestRequest)]
	[ProtoContract]
	public partial class C2M_TestRequest: ProtoObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string request { get; set; }

	}

	[Message(OuterMessage.M2C_TestResponse)]
	[ProtoContract]
	public partial class M2C_TestResponse: ProtoObject, IActorLocationResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public string response { get; set; }

	}

	[Message(OuterMessage.Actor_TransferRequest)]
	[ProtoContract]
	public partial class Actor_TransferRequest: ProtoObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int MapIndex { get; set; }

	}

	[Message(OuterMessage.Actor_TransferResponse)]
	[ProtoContract]
	public partial class Actor_TransferResponse: ProtoObject, IActorLocationResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2G_EnterMap)]
	[ProtoContract]
	public partial class C2G_EnterMap: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_EnterMap)]
	[ProtoContract]
	public partial class G2C_EnterMap: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

// 自己unitId
		[ProtoMember(4)]
		public long MyId { get; set; }

	}

	[Message(OuterMessage.MoveInfo)]
	[ProtoContract]
	public partial class MoveInfo: ProtoObject
	{
		[ProtoMember(1)]
		public List<Unity.Mathematics.float3> Points { get; set; }

		[ProtoMember(2)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

		[ProtoMember(3)]
		public int TurnSpeed { get; set; }

	}

	[Message(OuterMessage.UnitInfo)]
	[ProtoContract]
	public partial class UnitInfo: ProtoObject
	{
		[ProtoMember(1)]
		public long UnitId { get; set; }

		[ProtoMember(2)]
		public int ConfigId { get; set; }

		[ProtoMember(3)]
		public int Type { get; set; }

		[ProtoMember(4)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(5)]
		public Unity.Mathematics.float3 Forward { get; set; }

		[MongoDB.Bson.Serialization.Attributes.BsonDictionaryOptions(MongoDB.Bson.Serialization.Options.DictionaryRepresentation.ArrayOfArrays)]
		[ProtoMember(6)]
		public Dictionary<int, long> KV { get; set; }
		[ProtoMember(7)]
		public MoveInfo MoveInfo { get; set; }

	}

	[Message(OuterMessage.M2C_CreateUnits)]
	[ProtoContract]
	public partial class M2C_CreateUnits: ProtoObject, IActorMessage
	{
		[ProtoMember(1)]
		public List<UnitInfo> Units { get; set; }

	}

	[Message(OuterMessage.M2C_CreateMyUnit)]
	[ProtoContract]
	public partial class M2C_CreateMyUnit: ProtoObject, IActorMessage
	{
		[ProtoMember(1)]
		public UnitInfo Unit { get; set; }

	}

	[Message(OuterMessage.M2C_StartSceneChange)]
	[ProtoContract]
	public partial class M2C_StartSceneChange: ProtoObject, IActorMessage
	{
		[ProtoMember(1)]
		public long SceneInstanceId { get; set; }

		[ProtoMember(2)]
		public string SceneName { get; set; }

	}

	[Message(OuterMessage.M2C_RemoveUnits)]
	[ProtoContract]
	public partial class M2C_RemoveUnits: ProtoObject, IActorMessage
	{
		[ProtoMember(2)]
		public List<long> Units { get; set; }

	}

	[Message(OuterMessage.C2M_PathfindingResult)]
	[ProtoContract]
	public partial class C2M_PathfindingResult: ProtoObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public Unity.Mathematics.float3 Position { get; set; }

	}

	[Message(OuterMessage.C2M_Stop)]
	[ProtoContract]
	public partial class C2M_Stop: ProtoObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_PathfindingResult)]
	[ProtoContract]
	public partial class M2C_PathfindingResult: ProtoObject, IActorMessage
	{
		[ProtoMember(1)]
		public long Id { get; set; }

		[ProtoMember(2)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(3)]
		public List<Unity.Mathematics.float3> Points { get; set; }

	}

	[Message(OuterMessage.M2C_Stop)]
	[ProtoContract]
	public partial class M2C_Stop: ProtoObject, IActorMessage
	{
		[ProtoMember(1)]
		public int Error { get; set; }

		[ProtoMember(2)]
		public long Id { get; set; }

		[ProtoMember(3)]
		public Unity.Mathematics.float3 Position { get; set; }

		[ProtoMember(4)]
		public Unity.Mathematics.quaternion Rotation { get; set; }

	}

	[Message(OuterMessage.C2G_Ping)]
	[ProtoContract]
	public partial class C2G_Ping: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Ping)]
	[ProtoContract]
	public partial class G2C_Ping: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long Time { get; set; }

	}

	[Message(OuterMessage.G2C_Test)]
	[ProtoContract]
	public partial class G2C_Test: ProtoObject, IMessage
	{
	}

	[Message(OuterMessage.C2M_Reload)]
	[ProtoContract]
	public partial class C2M_Reload: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Account { get; set; }

		[ProtoMember(3)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.M2C_Reload)]
	[ProtoContract]
	public partial class M2C_Reload: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2R_Login)]
	[ProtoContract]
	public partial class C2R_Login: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Account { get; set; }

		[ProtoMember(3)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.R2C_Login)]
	[ProtoContract]
	public partial class R2C_Login: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public string Address { get; set; }

		[ProtoMember(5)]
		public long Key { get; set; }

		[ProtoMember(6)]
		public long GateId { get; set; }

	}

	[Message(OuterMessage.C2G_LoginGate)]
	[ProtoContract]
	public partial class C2G_LoginGate: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long GateId { get; set; }

	}

	[Message(OuterMessage.G2C_LoginGate)]
	[ProtoContract]
	public partial class G2C_LoginGate: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long PlayerId { get; set; }

	}

	[Message(OuterMessage.G2C_TestHotfixMessage)]
	[ProtoContract]
	public partial class G2C_TestHotfixMessage: ProtoObject, IMessage
	{
		[ProtoMember(1)]
		public string Info { get; set; }

	}

	[Message(OuterMessage.C2M_TestRobotCase)]
	[ProtoContract]
	public partial class C2M_TestRobotCase: ProtoObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int N { get; set; }

	}

	[Message(OuterMessage.M2C_TestRobotCase)]
	[ProtoContract]
	public partial class M2C_TestRobotCase: ProtoObject, IActorLocationResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public int N { get; set; }

	}

	[Message(OuterMessage.C2M_TransferMap)]
	[ProtoContract]
	public partial class C2M_TransferMap: ProtoObject, IActorLocationRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.M2C_TransferMap)]
	[ProtoContract]
	public partial class M2C_TransferMap: ProtoObject, IActorLocationResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2G_Benchmark)]
	[ProtoContract]
	public partial class C2G_Benchmark: ProtoObject, IRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_Benchmark)]
	[ProtoContract]
	public partial class G2C_Benchmark: ProtoObject, IResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2G_StartMatch_Req)]
	[ProtoContract]
	public partial class C2G_StartMatch_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

	}

	[Message(OuterMessage.G2C_StartMatch_Ack)]
	[ProtoContract]
	public partial class G2C_StartMatch_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2G_GetUserInfo_Req)]
	[ProtoContract]
	public partial class C2G_GetUserInfo_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(OuterMessage.G2C_GetUserInfo_Ack)]
	[ProtoContract]
	public partial class G2C_GetUserInfo_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public string NickName { get; set; }

		[ProtoMember(2)]
		public int Wins { get; set; }

		[ProtoMember(3)]
		public int Loses { get; set; }

		[ProtoMember(4)]
		public long Money { get; set; }

	}

	[Message(OuterMessage.C2R_Register_Req)]
	[ProtoContract]
	public partial class C2R_Register_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public string Account { get; set; }

		[ProtoMember(2)]
		public string Password { get; set; }

	}

	[Message(OuterMessage.R2C_Register_Ack)]
	[ProtoContract]
	public partial class R2C_Register_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(OuterMessage.C2G_LoginGate_Req)]
	[ProtoContract]
	public partial class C2G_LoginGate_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long Key { get; set; }

	}

	[Message(OuterMessage.G2C_LoginGate_Ack)]
	[ProtoContract]
	public partial class G2C_LoginGate_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public long PlayerID { get; set; }

		[ProtoMember(2)]
		public long UserID { get; set; }

	}

	[Message(OuterMessage.C2G_ReturnLobby_Ntt)]
	[ProtoContract]
	public partial class C2G_ReturnLobby_Ntt: ProtoObject, IMessage
	{
	}

    public enum Suits {
        // 梅花
        [pbr::OriginalName("Club")] Club = 0,
        // 方块
        [pbr::OriginalName("Diamond")] Diamond = 1,
        // 红心
        [pbr::OriginalName("Heart")] Heart = 2,
        // 黑桃
        [pbr::OriginalName("Spade")] Spade = 3,
        [pbr::OriginalName("None")] None = 4,
        }
    public enum Weight {
        // 3
        [pbr::OriginalName("Three")] Three = 0,
        // 4
        [pbr::OriginalName("Four")] Four = 1,
        // 5
        [pbr::OriginalName("Five")] Five = 2,
        // 6
        [pbr::OriginalName("Six")] Six = 3,
        // 7
        [pbr::OriginalName("Seven")] Seven = 4,
        // 8
        [pbr::OriginalName("Eight")] Eight = 5,
        // 9
        [pbr::OriginalName("Nine")] Nine = 6,
        // 10
        [pbr::OriginalName("Ten")] Ten = 7,
        // J
        [pbr::OriginalName("Jack")] Jack = 8,
        // Q
        [pbr::OriginalName("Queen")] Queen = 9,
        // K
        [pbr::OriginalName("King")] King = 10,
        // A
        [pbr::OriginalName("One")] One = 11,
        // 2
        [pbr::OriginalName("Two")] Two = 12,
        // 小王
        [pbr::OriginalName("SJoker")] Sjoker = 13,
        // 大王
        [pbr::OriginalName("LJoker")] Ljoker = 14,
        }
    public enum Identity {
        [pbr::OriginalName("IdentityNone")] None = 0,
        // 平民
        [pbr::OriginalName("Farmer")] Farmer = 1,
        // 地主
        [pbr::OriginalName("Landlord")] Landlord = 2,
        }  

//     public sealed partial class Card : pb::IMessage<Card>
// #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
//         , pb::IBufferMessage
// #endif
//     {
//         private static readonly pb::MessageParser<Card> _parser = new pb::MessageParser<Card>(() => new Card());
//         private pb::UnknownFieldSet _unknownFields;
//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public static pb::MessageParser<Card> Parser { get { return _parser; } }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public static pbr::MessageDescriptor Descriptor {
//             get { return global::ET.OuterMessageC10001Reflection.Descriptor.MessageTypes[0]; }
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         pbr::MessageDescriptor pb::IMessage.Descriptor {
//             get { return Descriptor; }
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public Card() {
//             OnConstruction();
//         }

//         partial void OnConstruction();

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public Card(Card other) : this() {
//             cardWeight_ = other.cardWeight_;
//             cardSuits_ = other.cardSuits_;
//             _unknownFields = pb::UnknownFieldSet.Clone(other._unknownFields);
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public Card Clone() {
//             return new Card(this);
//         }

//         /// <summary>Field number for the "CardWeight" field.</summary>
//         public const int CardWeightFieldNumber = 1;
//         private global::ET.Weight cardWeight_ = global::ET.Weight.Three;
//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public global::ET.Weight CardWeight {
//             get { return cardWeight_; }
//             set {
//                 cardWeight_ = value;
//             }
//         }

//         /// <summary>Field number for the "CardSuits" field.</summary>
//         public const int CardSuitsFieldNumber = 2;
//         private global::ET.Suits cardSuits_ = global::ET.Suits.Club;
//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public global::ET.Suits CardSuits {
//             get { return cardSuits_; }
//             set {
//                 cardSuits_ = value;
//             }
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public override bool Equals(object other) {
//             return Equals(other as Card);
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public bool Equals(Card other) {
//             if (ReferenceEquals(other, null)) {
//                 return false;
//             }
//             if (ReferenceEquals(other, this)) {
//                 return true;
//             }
//             if (CardWeight != other.CardWeight) return false;
//             if (CardSuits != other.CardSuits) return false;
//             return Equals(_unknownFields, other._unknownFields);
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public override int GetHashCode() {
//             int hash = 1;
//             if (CardWeight != global::ET.Weight.Three) hash ^= CardWeight.GetHashCode();
//             if (CardSuits != global::ET.Suits.Club) hash ^= CardSuits.GetHashCode();
//             if (_unknownFields != null) {
//                 hash ^= _unknownFields.GetHashCode();
//             }
//             return hash;
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public override string ToString() {
//             return pb::JsonFormatter.ToDiagnosticString(this);
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public void WriteTo(pb::CodedOutputStream output) {
//     #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
//             output.WriteRawMessage(this);
//     #else
//             if (CardWeight != global::ET.Weight.Three) {
//                 output.WriteRawTag(8);
//                 output.WriteEnum((int) CardWeight);
//             }
//             if (CardSuits != global::ET.Suits.Club) {
//                 output.WriteRawTag(16);
//                 output.WriteEnum((int) CardSuits);
//             }
//             if (_unknownFields != null) {
//                 _unknownFields.WriteTo(output);
//             }
//     #endif
//         }

//     #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         void pb::IBufferMessage.InternalWriteTo(ref pb::WriteContext output) {
//             if (CardWeight != global::ET.Weight.Three) {
//                 output.WriteRawTag(8);
//                 output.WriteEnum((int) CardWeight);
//             }
//             if (CardSuits != global::ET.Suits.Club) {
//                 output.WriteRawTag(16);
//                 output.WriteEnum((int) CardSuits);
//             }
//             if (_unknownFields != null) {
//                 _unknownFields.WriteTo(ref output);
//             }
//         }
//     #endif

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public int CalculateSize() {
//             int size = 0;
//             if (CardWeight != global::ET.Weight.Three) {
//                 size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) CardWeight);
//             }
//             if (CardSuits != global::ET.Suits.Club) {
//                 size += 1 + pb::CodedOutputStream.ComputeEnumSize((int) CardSuits);
//             }
//             if (_unknownFields != null) {
//                 size += _unknownFields.CalculateSize();
//             }
//             return size;
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public void MergeFrom(Card other) {
//             if (other == null) {
//                 return;
//             }
//             if (other.CardWeight != global::ET.Weight.Three) {
//                 CardWeight = other.CardWeight;
//             }
//             if (other.CardSuits != global::ET.Suits.Club) {
//                 CardSuits = other.CardSuits;
//             }
//             _unknownFields = pb::UnknownFieldSet.MergeFrom(_unknownFields, other._unknownFields);
//         }

//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         public void MergeFrom(pb::CodedInputStream input) {
//     #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
//             input.ReadRawMessage(this);
//     #else
//             uint tag;
//             while ((tag = input.ReadTag()) != 0) {
//                 switch(tag) {
//                 default:
//                     _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, input);
//                     break;
//                 case 8: {
//                     CardWeight = (global::ET.Weight) input.ReadEnum();
//                     break;
//                 }
//                 case 16: {
//                     CardSuits = (global::ET.Suits) input.ReadEnum();
//                     break;
//                 }
//                 }
//             }
//     #endif
//         }

//     #if !GOOGLE_PROTOBUF_REFSTRUCT_COMPATIBILITY_MODE
//         [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
//         [global::System.CodeDom.Compiler.GeneratedCode("protoc", null)]
//         void pb::IBufferMessage.InternalMergeFrom(ref pb::ParseContext input) {
//             uint tag;
//             while ((tag = input.ReadTag()) != 0) {
//                 switch(tag) {
//                 default:
//                     _unknownFields = pb::UnknownFieldSet.MergeFieldFrom(_unknownFields, ref input);
//                     break;
//                 case 8: {
//                     CardWeight = (global::ET.Weight) input.ReadEnum();
//                     break;
//                 }
//                 case 16: {
//                     CardSuits = (global::ET.Suits) input.ReadEnum();
//                     break;
//                 }
//                 }
//             }
//         }
//     #endif
//     }
    
	[Message(OuterMessage.Card)]
	[ProtoContract]
	public partial class Card: ProtoObject {
		[ProtoMember(1)]
		public Weight CardWeight { get; set; }
		[ProtoMember(2)]
		public Suits CardSuits { get; set; }
	}

	public static class OuterMessage
	{
		 public const ushort HttpGetRouterResponse = 10002;
		 public const ushort RouterSync = 10003;
		 public const ushort C2M_TestRequest = 10004;
		 public const ushort M2C_TestResponse = 10005;
		 public const ushort Actor_TransferRequest = 10006;
		 public const ushort Actor_TransferResponse = 10007;
		 public const ushort C2G_EnterMap = 10008;
		 public const ushort G2C_EnterMap = 10009;
		 public const ushort MoveInfo = 10010;
		 public const ushort UnitInfo = 10011;
		 public const ushort M2C_CreateUnits = 10012;
		 public const ushort M2C_CreateMyUnit = 10013;
		 public const ushort M2C_StartSceneChange = 10014;
		 public const ushort M2C_RemoveUnits = 10015;
		 public const ushort C2M_PathfindingResult = 10016;
		 public const ushort C2M_Stop = 10017;
		 public const ushort M2C_PathfindingResult = 10018;
		 public const ushort M2C_Stop = 10019;
		 public const ushort C2G_Ping = 10020;
		 public const ushort G2C_Ping = 10021;
		 public const ushort G2C_Test = 10022;
		 public const ushort C2M_Reload = 10023;
		 public const ushort M2C_Reload = 10024;
		 public const ushort C2R_Login = 10025;
		 public const ushort R2C_Login = 10026;
		 public const ushort C2G_LoginGate = 10027;
		 public const ushort G2C_LoginGate = 10028;
		 public const ushort G2C_TestHotfixMessage = 10029;
		 public const ushort C2M_TestRobotCase = 10030;
		 public const ushort M2C_TestRobotCase = 10031;
		 public const ushort C2M_TransferMap = 10032;
		 public const ushort M2C_TransferMap = 10033;
		 public const ushort C2G_Benchmark = 10034;
		 public const ushort G2C_Benchmark = 10035;
		 public const ushort C2G_StartMatch_Req = 10036;
		 public const ushort G2C_StartMatch_Ack = 10037;
		 public const ushort C2G_GetUserInfo_Req = 10038;
		 public const ushort G2C_GetUserInfo_Ack = 10039;
		 public const ushort C2R_Register_Req = 10040;
		 public const ushort R2C_Register_Ack = 10041;
		 public const ushort C2G_LoginGate_Req = 10042;
		 public const ushort G2C_LoginGate_Ack = 10043;
		 public const ushort C2G_ReturnLobby_Ntt = 10044;
		 public const ushort Card = 10045;
	}
}
