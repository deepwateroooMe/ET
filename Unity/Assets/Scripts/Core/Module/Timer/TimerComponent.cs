using System.Collections.Generic;
namespace ET {
    // 计时器：所涉及的方方面面
    public enum TimerClass { // 类型：
        None,      // 无
        OnceTimer, // 一次性
        OnceWaitTimer,  // 一次性要等待的计时器
        RepeatedTimer,  // 重复性、周期性计时器
    }
    public class TimerAction {
        public static TimerAction Create(long id, TimerClass timerClass, long startTime, long time, int type, object obj) {
            TimerAction timerAction = ObjectPool.Instance.Fetch<TimerAction>();
            timerAction.Id = id;
            timerAction.TimerClass = timerClass;
            timerAction.StartTime = startTime;
            timerAction.Object = obj;
            timerAction.Time = time;
            timerAction.Type = type; // 回调到，框架里标签系统，标记申明过的类的类型， eg ActorLocationSenderChecker 类
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
    public struct TimerCallback { // 在标签系中会用到计时器的回调
        public object Args;
    }
    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate { // 单例计时组件，使用时，拿单例的 Instance: TimerComponent.Instance.方法名
        // key: time, value: timer id
        private readonly MultiMap<long, long> TimeId = new();
        private readonly Queue<long> timeOutTime = new();
        private readonly Queue<long> timeOutTimerIds = new();
        private readonly Dictionary<long, TimerAction> timerActions = new();
        private long idGenerator;
        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue;
        private long GetId() {
            return ++this.idGenerator;
        }
        private static long GetNow() {
            return TimeHelper.ClientFrameTime();
        }
        public void Update() { // 每桢调用
            if (this.TimeId.Count == 0) {
                return;
            }
            long timeNow = GetNow();
            if (timeNow < this.minTime) { // 还不到最近一次需要闹的时间，直接返回
                return;
            }
            foreach (KeyValuePair<long, List<long>> kv in this.TimeId) {
                long k = kv.Key;
                if (k > timeNow) { // 因为它是排序字典，所以找到第一个不超时的，组件记下下次的闹钟时间，等时间到再闹，就可以退出了
                    this.minTime = k;
                    break;
                }
                this.timeOutTime.Enqueue(k); // 其它超时的，入队列处理回调逻辑
            }
            while (this.timeOutTime.Count > 0) { // 遍历：一一处理，这一桢里，超时闹钟的回调
                long time = this.timeOutTime.Dequeue();
                var list = this.TimeId[time];
                for (int i = 0; i < list.Count; ++i) {
                    long timerId = list[i];
                    this.timeOutTimerIds.Enqueue(timerId);
                }
                this.TimeId.Remove(time);
            }
            while (this.timeOutTimerIds.Count > 0) {
                long timerId = this.timeOutTimerIds.Dequeue();
                if (!this.timerActions.Remove(timerId, out TimerAction timerAction)) {
                    continue;
                }
                this.Run(timerAction); // 真正调用回调逻辑：比如ActorMessageSenderComponentSystem 里的 Check() 方法
            }
        }
        private void Run(TimerAction timerAction) {
            // 区分重复闹钟：与否，的逻辑，在这个方法里处理
            switch (timerAction.TimerClass) {
                case TimerClass.OnceTimer: { // 一次性闹钟
                    EventSystem.Instance.Invoke(timerAction.Type, new TimerCallback() { Args = timerAction.Object });
                    timerAction.Recycle(); // 一次闹钟闹过了，就回收
                    break;
                }
                case TimerClass.OnceWaitTimer: { // 需要包装一个异步任务
                    ETTask tcs = timerAction.Object as ETTask;
                    tcs.SetResult();
                    timerAction.Recycle();
                    break;
                }
                case TimerClass.RepeatedTimer: { // 重复闹钟：每桢（每一次的闹钟到闹钟响），都会重新调用下一次闹钟到的回调。所以它就成重复闹钟了。。                    
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
            this.TimeId.Add(tillTime, timer.Id);
            this.timerActions.Add(timer.Id, timer);
            if (tillTime < this.minTime) {
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
            if (tillTime < timeNow) 
                Log.Error($"new once time too small: {tillTime}");
            // 重点下面：时间到的回调包装：
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