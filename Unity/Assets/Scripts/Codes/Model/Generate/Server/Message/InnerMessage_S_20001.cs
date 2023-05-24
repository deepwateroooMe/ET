using ET;
using ProtoBuf;
using System.Collections.Generic;

using pb = global::Google.Protobuf;
using pbc = global::Google.Protobuf.Collections;
using pbr = global::Google.Protobuf.Reflection;
using scg = global::System.Collections.Generic;

namespace ET
{
    public enum GrabLandlordState {
        /// <summary>
        ///未抢地主
        /// </summary>
        [pbr::OriginalName("Not")] Not = 0,
        /// <summary>
        ///抢地主
        /// </summary>
        [pbr::OriginalName("Grab")] Grab = 1,
        /// <summary>
        ///不抢地主
        /// </summary>
        [pbr::OriginalName("UnGrab")] UnGrab = 2,
        }
    
	[Message(InnerMessage.ObjectQueryRequest)]
	[ProtoContract]
	public partial class ObjectQueryRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.M2A_Reload)]
	[ProtoContract]
	public partial class M2A_Reload: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.A2M_Reload)]
	[ProtoContract]
	public partial class A2M_Reload: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.G2G_LockRequest)]
	[ProtoContract]
	public partial class G2G_LockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Id { get; set; }

		[ProtoMember(3)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockResponse)]
	[ProtoContract]
	public partial class G2G_LockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.G2G_LockReleaseRequest)]
	[ProtoContract]
	public partial class G2G_LockReleaseRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Id { get; set; }

		[ProtoMember(3)]
		public string Address { get; set; }

	}

	[Message(InnerMessage.G2G_LockReleaseResponse)]
	[ProtoContract]
	public partial class G2G_LockReleaseResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.ObjectAddRequest)]
	[ProtoContract]
	public partial class ObjectAddRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.ObjectAddResponse)]
	[ProtoContract]
	public partial class ObjectAddResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.ObjectLockRequest)]
	[ProtoContract]
	public partial class ObjectLockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long InstanceId { get; set; }

		[ProtoMember(4)]
		public int Time { get; set; }

	}

	[Message(InnerMessage.ObjectLockResponse)]
	[ProtoContract]
	public partial class ObjectLockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.ObjectUnLockRequest)]
	[ProtoContract]
	public partial class ObjectUnLockRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

		[ProtoMember(3)]
		public long OldInstanceId { get; set; }

		[ProtoMember(4)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.ObjectUnLockResponse)]
	[ProtoContract]
	public partial class ObjectUnLockResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.ObjectRemoveRequest)]
	[ProtoContract]
	public partial class ObjectRemoveRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectRemoveResponse)]
	[ProtoContract]
	public partial class ObjectRemoveResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.ObjectGetRequest)]
	[ProtoContract]
	public partial class ObjectGetRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.ObjectGetResponse)]
	[ProtoContract]
	public partial class ObjectGetResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long InstanceId { get; set; }

	}

	[Message(InnerMessage.R2G_GetLoginKey)]
	[ProtoContract]
	public partial class R2G_GetLoginKey: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public string Account { get; set; }

	}

	[Message(InnerMessage.G2R_GetLoginKey)]
	[ProtoContract]
	public partial class G2R_GetLoginKey: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public long Key { get; set; }

		[ProtoMember(5)]
		public long GateId { get; set; }

	}

	[Message(InnerMessage.G2M_SessionDisconnect)]
	[ProtoContract]
	public partial class G2M_SessionDisconnect: ProtoObject, IActorLocationMessage
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.ObjectQueryResponse)]
	[ProtoContract]
	public partial class ObjectQueryResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

		[ProtoMember(4)]
		public byte[] Entity { get; set; }

	}

	[Message(InnerMessage.M2M_UnitTransferRequest)]
	[ProtoContract]
	public partial class M2M_UnitTransferRequest: ProtoObject, IActorRequest
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public long OldInstanceId { get; set; }

		[ProtoMember(3)]
		public byte[] Unit { get; set; }

		[ProtoMember(4)]
		public List<byte[]> Entitys { get; set; }

	}

	[Message(InnerMessage.M2M_UnitTransferResponse)]
	[ProtoContract]
	public partial class M2M_UnitTransferResponse: ProtoObject, IActorResponse
	{
		[ProtoMember(1)]
		public int RpcId { get; set; }

		[ProtoMember(2)]
		public int Error { get; set; }

		[ProtoMember(3)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.Actor_PlayerEnterRoom_Req)]
	[ProtoContract]
	public partial class Actor_PlayerEnterRoom_Req: ProtoObject, IActorRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long PlayerID { get; set; }

		[ProtoMember(2)]
		public long UserID { get; set; }

		[ProtoMember(3)]
		public long SessionID { get; set; }

	}

	[Message(InnerMessage.Actor_PlayerEnterRoom_Ack)]
	[ProtoContract]
	public partial class Actor_PlayerEnterRoom_Ack: ProtoObject, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public long GamerID { get; set; }

	}

	[Message(InnerMessage.Actor_SetMultiples_Ntt)]
	[ProtoContract]
	public partial class Actor_SetMultiples_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public int Multiples { get; set; }

	}

	[Message(InnerMessage.Actor_SetLandlord_Ntt)]
	[ProtoContract]
	public partial class Actor_SetLandlord_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_Gameover_Ntt)]
	[ProtoContract]
	public partial class Actor_Gameover_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(2)]
		public long BasePointPerMatch { get; set; }

		[ProtoMember(3)]
		public int Multiples { get; set; }

		[ProtoMember(4)]
		public List<GamerScore> GamersScore { get; set; }

	}

	[Message(InnerMessage.Actor_GamerMoneyLess_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerMoneyLess_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.G2R_PlayerOnline_Req)]
	[ProtoContract]
	public partial class G2R_PlayerOnline_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public int GateAppID { get; set; }

	}

	[Message(InnerMessage.R2G_PlayerOnline_Ack)]
	[ProtoContract]
	public partial class R2G_PlayerOnline_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.G2R_PlayerOffline_Req)]
	[ProtoContract]
	public partial class G2R_PlayerOffline_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.R2G_PlayerOffline_Ack)]
	[ProtoContract]
	public partial class R2G_PlayerOffline_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.R2G_GetLoginKey_Req)]
	[ProtoContract]
	public partial class R2G_GetLoginKey_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.G2R_GetLoginKey_Ack)]
	[ProtoContract]
	public partial class G2R_GetLoginKey_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public long Key { get; set; }

	}

	[Message(InnerMessage.R2G_PlayerKickOut_Req)]
	[ProtoContract]
	public partial class R2G_PlayerKickOut_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.G2R_PlayerKickOut_Ack)]
	[ProtoContract]
	public partial class G2R_PlayerKickOut_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.G2M_PlayerExitMatch_Req)]
	[ProtoContract]
	public partial class G2M_PlayerExitMatch_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.M2G_PlayerExitMatch_Ack)]
	[ProtoContract]
	public partial class M2G_PlayerExitMatch_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.Actor_PlayerExitRoom_Req)]
	[ProtoContract]
	public partial class Actor_PlayerExitRoom_Req: ProtoObject, IActorRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_PlayerExitRoom_Ack)]
	[ProtoContract]
	public partial class Actor_PlayerExitRoom_Ack: ProtoObject, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.Actor_MatchSucess_Ntt)]
	[ProtoContract]
	public partial class Actor_MatchSucess_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long GamerID { get; set; }

	}

	[Message(InnerMessage.MH2MP_CreateRoom_Req)]
	[ProtoContract]
	public partial class MH2MP_CreateRoom_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.MP2MH_CreateRoom_Ack)]
	[ProtoContract]
	public partial class MP2MH_CreateRoom_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

		[ProtoMember(1)]
		public long RoomID { get; set; }

	}

	[Message(InnerMessage.MP2MH_PlayerExitRoom_Req)]
	[ProtoContract]
	public partial class MP2MH_PlayerExitRoom_Req: ProtoObject, IRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(1)]
		public long RoomID { get; set; }

		[ProtoMember(2)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.MH2MP_PlayerExitRoom_Ack)]
	[ProtoContract]
	public partial class MH2MP_PlayerExitRoom_Ack: ProtoObject, IResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.MP2MH_SyncRoomState_Ntt)]
	[ProtoContract]
	public partial class MP2MH_SyncRoomState_Ntt: ProtoObject, IMessage
	{
		[ProtoMember(1)]
		public long RoomID { get; set; }

		[ProtoMember(2)]
		public RoomState State { get; set; }

	}

	[Message(InnerMessage.PlayerInfo)]
	[ProtoContract]
	public partial class PlayerInfo: ProtoObject, IMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

	}

	[Message(InnerMessage.Actor_GamerReady_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerReady_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_GamerGrabLandlordSelect_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerGrabLandlordSelect_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public bool IsGrab { get; set; }

	}

	[Message(InnerMessage.Actor_GamerPlayCard_Req)]
	[ProtoContract]
	public partial class Actor_GamerPlayCard_Req: ProtoObject, IActorRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public long ActorId { get; set; }

	}

	[Message(InnerMessage.Actor_GamerPlayCard_Ack)]
	[ProtoContract]
	public partial class Actor_GamerPlayCard_Ack: ProtoObject, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.Actor_GamerPlayCard_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerPlayCard_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_GamerPrompt_Req)]
	[ProtoContract]
	public partial class Actor_GamerPrompt_Req: ProtoObject, IActorRequest
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public long ActorId { get; set; }

	}

	[Message(InnerMessage.Actor_GamerPrompt_Ack)]
	[ProtoContract]
	public partial class Actor_GamerPrompt_Ack: ProtoObject, IActorResponse
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(91)]
		public int Error { get; set; }

		[ProtoMember(92)]
		public string Message { get; set; }

	}

	[Message(InnerMessage.Actor_GamerDontPlay_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerDontPlay_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_Trusteeship_Ntt)]
	[ProtoContract]
	public partial class Actor_Trusteeship_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public bool isTrusteeship { get; set; }

	}

	[Message(InnerMessage.GamerInfo)]
	[ProtoContract]
	public partial class GamerInfo: ProtoObject
	{
		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public bool IsReady { get; set; }

	}

	[Message(InnerMessage.Actor_GamerEnterRoom_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerEnterRoom_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public List<GamerInfo> Gamers { get; set; }

	}

	[Message(InnerMessage.Actor_GamerExitRoom_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerExitRoom_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.GamerState)]
	[ProtoContract]
	public partial class GamerState: ProtoObject
	{
		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(3)]
		public GrabLandlordState State { get; set; }

	}

	[Message(InnerMessage.Actor_GamerReconnect_Ntt)]
	[ProtoContract]
	public partial class Actor_GamerReconnect_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserId { get; set; }

		[ProtoMember(2)]
		public int Multiples { get; set; }

		[ProtoMember(4)]
		public List<GamerState> GamersState { get; set; }

	}

	[Message(InnerMessage.GamerCardNum)]
	[ProtoContract]
	public partial class GamerCardNum: ProtoObject, IMessage
	{
		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public int Num { get; set; }

	}

	[Message(InnerMessage.Actor_GameStart_Ntt)]
	[ProtoContract]
	public partial class Actor_GameStart_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(2)]
		public List<GamerCardNum> GamersCardNum { get; set; }

	}

	[Message(InnerMessage.Actor_AuthorityGrabLandlord_Ntt)]
	[ProtoContract]
	public partial class Actor_AuthorityGrabLandlord_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

	}

	[Message(InnerMessage.Actor_AuthorityPlayCard_Ntt)]
	[ProtoContract]
	public partial class Actor_AuthorityPlayCard_Ntt: ProtoObject, IActorMessage
	{
		[ProtoMember(90)]
		public int RpcId { get; set; }

		[ProtoMember(94)]
		public long ActorId { get; set; }

		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public bool IsFirst { get; set; }

	}

	[Message(InnerMessage.GamerScore)]
	[ProtoContract]
	public partial class GamerScore: ProtoObject
	{
		[ProtoMember(1)]
		public long UserID { get; set; }

		[ProtoMember(2)]
		public long Score { get; set; }

	}

	public static class InnerMessage
	{
		 public const ushort ObjectQueryRequest = 20002;
		 public const ushort M2A_Reload = 20003;
		 public const ushort A2M_Reload = 20004;
		 public const ushort G2G_LockRequest = 20005;
		 public const ushort G2G_LockResponse = 20006;
		 public const ushort G2G_LockReleaseRequest = 20007;
		 public const ushort G2G_LockReleaseResponse = 20008;
		 public const ushort ObjectAddRequest = 20009;
		 public const ushort ObjectAddResponse = 20010;
		 public const ushort ObjectLockRequest = 20011;
		 public const ushort ObjectLockResponse = 20012;
		 public const ushort ObjectUnLockRequest = 20013;
		 public const ushort ObjectUnLockResponse = 20014;
		 public const ushort ObjectRemoveRequest = 20015;
		 public const ushort ObjectRemoveResponse = 20016;
		 public const ushort ObjectGetRequest = 20017;
		 public const ushort ObjectGetResponse = 20018;
		 public const ushort R2G_GetLoginKey = 20019;
		 public const ushort G2R_GetLoginKey = 20020;
		 public const ushort G2M_SessionDisconnect = 20021;
		 public const ushort ObjectQueryResponse = 20022;
		 public const ushort M2M_UnitTransferRequest = 20023;
		 public const ushort M2M_UnitTransferResponse = 20024;
		 public const ushort Actor_PlayerEnterRoom_Req = 20025;
		 public const ushort Actor_PlayerEnterRoom_Ack = 20026;
		 public const ushort Actor_SetMultiples_Ntt = 20027;
		 public const ushort Actor_SetLandlord_Ntt = 20028;
		 public const ushort Actor_Gameover_Ntt = 20029;
		 public const ushort Actor_GamerMoneyLess_Ntt = 20030;
		 public const ushort G2R_PlayerOnline_Req = 20031;
		 public const ushort R2G_PlayerOnline_Ack = 20032;
		 public const ushort G2R_PlayerOffline_Req = 20033;
		 public const ushort R2G_PlayerOffline_Ack = 20034;
		 public const ushort R2G_GetLoginKey_Req = 20035;
		 public const ushort G2R_GetLoginKey_Ack = 20036;
		 public const ushort R2G_PlayerKickOut_Req = 20037;
		 public const ushort G2R_PlayerKickOut_Ack = 20038;
		 public const ushort G2M_PlayerExitMatch_Req = 20039;
		 public const ushort M2G_PlayerExitMatch_Ack = 20040;
		 public const ushort Actor_PlayerExitRoom_Req = 20041;
		 public const ushort Actor_PlayerExitRoom_Ack = 20042;
		 public const ushort Actor_MatchSucess_Ntt = 20043;
		 public const ushort MH2MP_CreateRoom_Req = 20044;
		 public const ushort MP2MH_CreateRoom_Ack = 20045;
		 public const ushort MP2MH_PlayerExitRoom_Req = 20046;
		 public const ushort MH2MP_PlayerExitRoom_Ack = 20047;
		 public const ushort MP2MH_SyncRoomState_Ntt = 20048;
		 public const ushort PlayerInfo = 20049;
		 public const ushort Actor_GamerReady_Ntt = 20050;
		 public const ushort Actor_GamerGrabLandlordSelect_Ntt = 20051;
		 public const ushort Actor_GamerPlayCard_Req = 20052;
		 public const ushort Actor_GamerPlayCard_Ack = 20053;
		 public const ushort Actor_GamerPlayCard_Ntt = 20054;
		 public const ushort Actor_GamerPrompt_Req = 20055;
		 public const ushort Actor_GamerPrompt_Ack = 20056;
		 public const ushort Actor_GamerDontPlay_Ntt = 20057;
		 public const ushort Actor_Trusteeship_Ntt = 20058;
		 public const ushort GamerInfo = 20059;
		 public const ushort Actor_GamerEnterRoom_Ntt = 20060;
		 public const ushort Actor_GamerExitRoom_Ntt = 20061;
		 public const ushort GamerState = 20062;
		 public const ushort Actor_GamerReconnect_Ntt = 20063;
		 public const ushort GamerCardNum = 20064;
		 public const ushort Actor_GameStart_Ntt = 20065;
		 public const ushort Actor_AuthorityGrabLandlord_Ntt = 20066;
		 public const ushort Actor_AuthorityPlayCard_Ntt = 20067;
		 public const ushort GamerScore = 20068;
	}
}
