using System;
using System.Collections.Generic;
namespace ET {
    // 感觉对这块，进程间通信，网络交互，再细节一点儿的，脑袋仍有点儿糊糊。。。这次争取都弄懂了。
    [FriendOf(typeof(OpcodeTypeComponent))]
    public static class OpcodeTypeComponentSystem {
        [ObjectSystem]
        public class OpcodeTypeComponentAwakeSystem: AwakeSystem<OpcodeTypeComponent> {
            protected override void Awake(OpcodeTypeComponent self) {
                OpcodeTypeComponent.Instance = self;
                
                self.requestResponse.Clear();
                HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute));
                foreach (Type type in types) {
                    object[] att = type.GetCustomAttributes(typeof (MessageAttribute), false);
                    if (att.Length == 0) {
                        continue;
                    }
                    MessageAttribute messageAttribute = att[0] as MessageAttribute;
                    if (messageAttribute == null) {
                        continue;
                    }
                    ushort opcode = messageAttribute.Opcode;
                    if (OpcodeHelper.IsOuterMessage(opcode) && typeof (IActorMessage).IsAssignableFrom(type)) {
                        self.outrActorMessage.Add(opcode);
                    }
                
                    // 检查request response
                    if (typeof (IRequest).IsAssignableFrom(type)) {
                        if (typeof (IActorLocationMessage).IsAssignableFrom(type))
                        {
                            self.requestResponse.Add(type, typeof(ActorResponse));
                            continue;
                        }
                    
                        var attrs = type.GetCustomAttributes(typeof (ResponseTypeAttribute), false);
                        if (attrs.Length == 0)
                        {
                            Log.Error($"not found responseType: {type}");
                            continue;
                        }
                        ResponseTypeAttribute responseTypeAttribute = attrs[0] as ResponseTypeAttribute;
                        self.requestResponse.Add(type, EventSystem.Instance.GetType($"ET.{responseTypeAttribute.Type}"));
                    }
                }
            }
        }
        [ObjectSystem]
        public class OpcodeTypeComponentDestroySystem: DestroySystem<OpcodeTypeComponent> {
            protected override void Destroy(OpcodeTypeComponent self) {
                OpcodeTypeComponent.Instance = null;
            }
        }
        public static bool IsOutrActorMessage(this OpcodeTypeComponent self, ushort opcode) {
            return self.outrActorMessage.Contains(opcode);
        }
<<<<<<< HEAD
        public static Type GetResponseType(this OpcodeTypeComponent self, Type request) {
            if (!self.requestResponse.TryGetValue(request, out Type response)) {
                throw new Exception($"not found response type, request type: {request.GetType().Name}");
=======

        public static Type GetResponseType(this OpcodeTypeComponent self, Type request)
        {
            if (!self.requestResponse.TryGetValue(request, out Type response))
            {
                throw new Exception($"not found response type, request type: {request.GetType().FullName}");
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
            }
            return response;
        }
    }
    [ComponentOf(typeof(Scene))]
    public class OpcodeTypeComponent: Entity, IAwake, IDestroy {
        [StaticField]
        public static OpcodeTypeComponent Instance;
        
        public HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        
        public readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
    }
}