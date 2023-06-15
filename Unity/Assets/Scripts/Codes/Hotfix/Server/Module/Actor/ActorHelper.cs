using System;
using System.IO;
namespace ET.Server {
    public static class ActorHelper {
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error) {
            Type responseType = OpcodeTypeComponent.Instance.GetResponseType(iActorRequest.GetType()); // 框架系统管理里，去拿返回消息的类型
            IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType); // 创建一个返回消息的实例 instance 
            response.Error = error; // 写实例的出错结果、类型
            response.RpcId = iActorRequest.RpcId; // 返回消息的接收者 RpcId: 实际就是，消息是谁发来的，就返回消息给谁呀
            return response; // 返回这个最小初始化过的特定类型消息实例
        }
    }
}