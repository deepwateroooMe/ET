using System;
using System.Collections.Generic;
namespace ET {
    [FriendOf(typeof(OpcodeTypeComponent))]
    public static class OpcodeTypeComponentSystem {
        [ObjectSystem]
        public class OpcodeTypeComponentAwakeSystem: AwakeSystem<OpcodeTypeComponent> {

            protected override void Awake(OpcodeTypeComponent self) {
                OpcodeTypeComponent.Instance = self;
                self.requestResponse.Clear(); // 清空重扫
// 事件系统，是【热更新】后第1 个反应、扫描热更域的监控，保障实时更新的
                HashSet<Type> types = EventSystem.Instance.GetTypes(typeof (MessageAttribute)); 
                foreach (Type type in types) {
                    object[] att = type.GetCustomAttributes(typeof (MessageAttribute), false);
                    if (att.Length == 0) 
                        continue;
                    MessageAttribute messageAttribute = att[0] as MessageAttribute;
                    if (messageAttribute == null) {
                        continue;
                    }
                    ushort opcode = messageAttribute.Opcode; // 这类消息的唯一标记字段，网络操作码
                    if (OpcodeHelper.IsOuterMessage(opcode) && typeof (IActorMessage).IsAssignableFrom(type)) {
                        self.outrActorMessage.Add(opcode);
                    }
                    // 检查request response
                    if (typeof (IRequest).IsAssignableFrom(type)) {
                        if (typeof (IActorLocationMessage).IsAssignableFrom(type)) {
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
        public static Type GetResponseType(this OpcodeTypeComponent self, Type request) {
            if (!self.requestResponse.TryGetValue(request, out Type response)) {
                throw new Exception($"not found response type, request type: {request.GetType().FullName}");
            }
            return response;
        }
    }
	// 【网络操作码、类型组件】：把这一个类型，提取出来，双端通用——最大限度地双端公用、每端都添加吗？
	// 双端公用组件：是否如事件系统，程序域加载，或是热更域更新时，扫程序集添加就可以了？去看热更域
    [ComponentOf(typeof(Scene))]
    public class OpcodeTypeComponent: Entity, IAwake, IDestroy { // 组件：怎么用的？
        [StaticField]
        public static OpcodeTypeComponent Instance;
        public HashSet<ushort> outrActorMessage = new HashSet<ushort>();
        public readonly Dictionary<Type, Type> requestResponse = new Dictionary<Type, Type>();
    }
}