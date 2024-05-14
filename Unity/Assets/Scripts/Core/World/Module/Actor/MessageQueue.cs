using System.Collections.Concurrent;
using System.Collections.Generic;
namespace ET {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
	// 【纤程】：【TODO】：亲爱的表哥的活宝妹，认为，ET 新版本，说是【多线程多进程】，但主要逻辑应该还是【单线程多进程】,除了某些模块，如【网络相关】模块【多线程】之外
	// 【纤程】：用一个线程模拟一个纤程，每个进程可以有多个线程纤程【多线程多进程】架构。
	// 纤程后，消息发送等相关模块还是作了必要适配。主要是一些公用逻辑的提取与封装：三种消息分发回调类、 actorId 自带进程纤程标记等
	// 纤程，这里，感觉难度不大，可是还没能系统地、去把它看得狠透。将来有机会，或是某天想要弄透某个模块、某个知识点儿的时候再回来看
	// 亲爱的表哥的活宝妹，狠想要去折腾自己的重构游戏项目了，里面的因为View 相关无数编译错误，现在再看都小蛋糕狠好解决。。
    public struct MessageInfo {
        public ActorId ActorId;
        public MessageObject MessageObject;
    }
	// 【消息队列】：同安卓【跨进程消息队列】像是一样的。这里管理多纤程多进程的消息，使用多进程安全同步字典
    public class MessageQueue: Singleton<MessageQueue>, ISingletonAwake {
        private readonly ConcurrentDictionary<int, ConcurrentQueue<MessageInfo>> messages = new();
        public void Awake() {
        }
        public bool Send(ActorId actorId, MessageObject messageObject) {
            return this.Send(actorId.Address, actorId, messageObject);
        }
        public void Reply(ActorId actorId, MessageObject messageObject) {
            this.Send(actorId.Address, actorId, messageObject);
        }
        public bool Send(Address fromAddress, ActorId actorId, MessageObject messageObject) {
            if (!this.messages.TryGetValue(actorId.Address.Fiber, out var queue)) {
                return false;
            }
            queue.Enqueue(new MessageInfo() {ActorId = new ActorId(fromAddress, actorId.InstanceId), MessageObject = messageObject});
            return true;
        }
        public void Fetch(int fiberId, int count, List<MessageInfo> list) { // 抓出：发给？这个【纤程】的所有消息
            if (!this.messages.TryGetValue(fiberId, out var queue)) {
                return;
            }
            for (int i = 0; i < count; ++i) {
                if (!queue.TryDequeue(out MessageInfo message)) {
                    break;
                }
                list.Add(message);
            }
        }
        public void AddQueue(int fiberId) {
            var queue = new ConcurrentQueue<MessageInfo>();
            this.messages[fiberId] = queue;
        }
        public void RemoveQueue(int fiberId) {
            this.messages.TryRemove(fiberId, out _);
        }
    }
}