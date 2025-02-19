namespace ET {
    public static partial class ErrorCode {
        public const int ERR_Success = 0;
        
        // 1-11004 是SocketError请看SocketError定义
        // -----------------------------------
        // 100000-109999是Core层的错误

        // 【下面】：不知道是否会服务器与客户端弄混了
        // 100000 以上，避免跟SocketError冲突
        public const int ERR_MyErrorCode = 100000;
        
        public const int ERR_ActorNoMailBoxComponent = 100003;
        public const int ERR_ActorRemove = 100004;
        public const int ERR_PacketParserError = 100005;
        
        public const int ERR_SessionActorError = 100103;
        public const int ERR_NotFoundUnit = 100104;
        public const int ERR_ConnectGateKeyError = 100105;
        public const int ERR_RpcFail = 102001;
        public const int ERR_SocketDisconnected = 102002;
        public const int ERR_ReloadFail = 102003;
        public const int ERR_ActorLocationNotFound = 102004;
        public const int ERR_KcpCantConnect = 102005;
        public const int ERR_KcpChannelTimeout = 102006;
        public const int ERR_KcpRemoteDisconnect = 102007;
        public const int ERR_PeerDisconnect = 102008;
        public const int ERR_SocketCantSend = 102009;
        public const int ERR_SocketError = 102010;
        public const int ERR_KcpWaitSendSizeTooLarge = 102011;
        public const int ERR_WebsocketPeerReset = 103001;
        public const int ERR_WebsocketMessageTooBig = 103002;
        public const int ERR_WebsocketError = 103003;
        public const int ERR_WebsocketConnectError = 103004;
        public const int ERR_WebsocketSendError = 103005;
        // 小于这个Rpc会抛异常，大于这个异常的error需要自己判断处理，也就是说需要处理的错误应该要大于该值
        public const int ERR_Exception = 200000;
        
        public const int ERR_NotFoundActor = 200002;
        public const int ERR_AccountOrPasswordError = 200102;
        public const int ERR_WebsocketRecvError = 203006;
        public static bool IsRpcNeedThrowException(int error) {
            if (error == 0) {
                return false;
            }
            if (error > ERR_Exception) {
                return false;
            }
            return true;
        }
        
// 照参考游戏搬过来的
        public const int ERR_SignError = 10000;
        public const int ERR_Disconnect = 270000;
        public const int ERR_AccountAlreadyRegister = 270001;
        public const int ERR_JoinRoomError = 270002;
        public const int ERR_UserMoneyLessError = 270003;
        public const int ERR_PlayCardError = 270004;
        public const int ERR_LoginError = 270005;
        
        // 110000以下的错误请看ErrorCore.cs
        
        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常
    }
}