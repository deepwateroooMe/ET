using System;
using System.IO;

namespace ET.Server
{
    public static class ActorHelper
    { 
        public static IActorResponse CreateResponse(IActorRequest iActorRequest, int error)
        {
            Type responseType = OpcodeTypeComponent.Instance.GetResponseType(iActorRequest.GetType());
            IActorResponse response = (IActorResponse)Activator.CreateInstance(responseType); // 创建实例
            response.Error = error; // 两条内容封装
            response.RpcId = iActorRequest.RpcId;
            return response;
        }
    }
}