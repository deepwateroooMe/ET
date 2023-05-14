namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        
        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误

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