using System.Collections.Generic;
namespace ET {

	// 组件的主要用途：【服务端】与【客户端】的时间同步等；框架里各种【超时机制】如跨进程消息的超时；带计时器的协程锁等。属于周边辅助、服务性模块，不可或缺
    public enum TimerClass {
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }
    public class TimerAction { // 回调类
        public static TimerAction Create(long id, TimerClass timerClass, long startTime, long time, int type, object obj) {
            TimerAction timerAction = ObjectPool.Instance.Fetch<TimerAction>();
            timerAction.Id = id;
            timerAction.TimerClass = timerClass;
            timerAction.StartTime = startTime;
            timerAction.Object = obj;
            timerAction.Time = time;
            timerAction.Type = type;
            return timerAction;
        }
        public long Id;
        public TimerClass TimerClass;
        public object Object;
        public long StartTime;
        public long Time;
        public int Type;
        public void Recycle() {
            this.Id = 0;
            this.Object = null;
            this.StartTime = 0;
            this.Time = 0;
            this.TimerClass = TimerClass.None;
            this.Type = 0;
            ObjectPool.Instance.Recycle(this);
        }
    }
    public struct TimerCallback {
        public object Args;
    }
    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate { // 单例类：仅只 CodeLoader 程序域里添加过这个组件，双端都需要用到
        // key: time, value: timer id
        private readonly MultiMap<long, long> TimeId = new(); // 有序銉增排序的
        private readonly Queue<long> timeOutTime = new();
        private readonly Queue<long> timeOutTimerIds = new();
        private readonly Dictionary<long, TimerAction> timerActions = new();
        private long idGenerator;
        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue;
        private long GetId() { // 这个组件的、实例自增Id 身份证号
            return ++this.idGenerator;
        }
        private static long GetNow() {
            return TimeHelper.ClientFrameTime();
        }
        public void Update() { // 每桢更新：触发执行必要的回调
            if (this.TimeId.Count == 0) {
                return;
            }
            long timeNow = GetNow();
            if (timeNow < this.minTime) {
                return;
            }
            foreach (KeyValuePair<long, List<long>> kv in this.TimeId) {
                long k = kv.Key;
                if (k > timeNow) { // 有序键增排序：找到第一个有效最小时间，就退出循环
                    this.minTime = k;
                    break;
                }
                this.timeOutTime.Enqueue(k);
            }
			// 两次遍历， timeOutTime 和timeOutTimerIds.
			// 两次遍历， timeOutTime: 是排闹钟要闹的、目标时间，为元素；timeOutTimerIds, 是这一桢，需要执行回调的、闹钟实例身份证号
            while (this.timeOutTime.Count > 0) { 
                long time = this.timeOutTime.Dequeue();
                var list = this.TimeId[time]; // 【有序多字典的、键：闹钟目标时间】可能不止一个闹钟实例，这个目标时间要回调，所以是链条、多个
                for (int i = 0; i < list.Count; ++i) {
                    long timerId = list[i];
                    this.timeOutTimerIds.Enqueue(timerId);
                }
                this.TimeId.Remove(time); // 清除：方便下一桢再填
            }
            while (this.timeOutTimerIds.Count > 0) {
                long timerId = this.timeOutTimerIds.Dequeue();
                if (!this.timerActions.Remove(timerId, out TimerAction timerAction)) {
                    continue;
                }
                this.Run(timerAction); // 闹钟到：执行回调、触发执行的地方
            }
        }
        private void Run(TimerAction timerAction) { // 私有方法：就是闹钟到、要闹的时候，需要调用这个
            switch (timerAction.TimerClass) { // 三类计时器：
                case TimerClass.OnceTimer: {
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.OnceWaitTimer: {
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.RepeatedTimer: {                    
                    long timeNow = GetNow();
                    timerAction.StartTime = timeNow;
                    this.AddTimer(timerAction);
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    break;
                }
            }
        }
        private void AddTimer(TimerAction timer) {
            long tillTime = timer.StartTime + timer.Time;
            this.TimeId.Add(tillTime, timer.Id);    // 用【闹钟要闹的、目标时间】排序 
            this.timerActions.Add(timer.Id, timer); // 对回调进行管理
            if (tillTime < this.minTime) { // 小便利：不到 minTime 就可以什么也不用做了
                this.minTime = tillTime;
            }
        }
        public bool Remove(ref long id) {
            long i = id;
            id = 0;
            return this.Remove(i);
        }
        private bool Remove(long id) {
            if (id == 0) {
                return false;
            }
            if (!this.timerActions.Remove(id, out TimerAction timerAction)) {
                return false;
            }
            timerAction.Recycle();
            return true;
        }
        public async ETTask WaitTillAsync(long tillTime, ETCancellationToken cancellationToken = null) {
            long timeNow = GetNow();
            if (timeNow >= tillTime) {
                return;
            }
            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;
            void CancelAction() {
                if (this.Remove(timerId)) {
                    tcs.SetResult();
                }
            }
            try {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally {
                cancellationToken?.Remove(CancelAction);
            }
        }
        public async ETTask WaitFrameAsync(ETCancellationToken cancellationToken = null) {
            await this.WaitAsync(1, cancellationToken);
        }
        public async ETTask WaitAsync(long time, ETCancellationToken cancellationToken = null) {
            if (time == 0) {
                return;
            }
            long timeNow = GetNow();
            ETTask tcs = ETTask.Create(true);
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, time, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;
            void CancelAction() {
                if (this.Remove(timerId)) {
                    tcs.SetResult();
                }
            }
            try {
                cancellationToken?.Add(CancelAction);
                await tcs;
            }
            finally {
                cancellationToken?.Remove(CancelAction);
            }
        }
        // 用这个优点是可以热更，缺点是回调式的写法，逻辑不连贯。WaitTillAsync不能热更，优点是逻辑连贯。
        // wait时间短并且逻辑需要连贯的建议WaitTillAsync
        // wait时间长不需要逻辑连贯的建议用NewOnceTimer
        public long NewOnceTimer(long tillTime, int type, object args) {
            long timeNow = GetNow();
            if (tillTime < timeNow) {
                Log.Error($"new once time too small: {tillTime}");
            }
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceTimer, timeNow, tillTime - timeNow, type, args);
            this.AddTimer(timer);
            return timer.Id;
        }
        public long NewFrameTimer(int type, object args) {
#if DOTNET
            return this.NewRepeatedTimerInner(100, type, args);
#else
            return this.NewRepeatedTimerInner(0, type, args);
#endif
        }
        // 创建一个RepeatedTimer
        private long NewRepeatedTimerInner(long time, int type, object args) {
#if DOTNET
            if (time < 100) {
                throw new Exception($"repeated timer < 100, timerType: time: {time}");
            }
#endif
            long timeNow = GetNow();
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.RepeatedTimer, timeNow, time, type, args);
            // 每帧执行的不用加到timerId中，防止遍历
            this.AddTimer(timer);
            return timer.Id;
        }
        public long NewRepeatedTimer(long time, int type, object args) {
            if (time < 100) {
                Log.Error($"time too small: {time}");
                return 0;
            }
            return this.NewRepeatedTimerInner(time, type, args);
        }
    }
}