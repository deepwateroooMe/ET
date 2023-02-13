using System;
namespace ET {

    // RPC异常,带ErrorCode
    public class RpcException: Exception {
        public int Error {
            get;
        }
        public RpcException(int error, string message): base($"Error: {error} Message: {message}") {
            this.Error = error;
        }
        public RpcException(int error, string message, Exception e): base($"Error: {error} Message: {message}", e) {
            this.Error = error;
        }
    }
}