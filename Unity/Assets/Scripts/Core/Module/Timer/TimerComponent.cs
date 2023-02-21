using System.Collections.Generic;
namespace ET {
// 不太喜欢看这个类：没有仔细看。怎么想呢，就想它是闹钟吧。框架里应用运行过程中可能需要无数个闹钟，每个闹钟都有它自己的职责，到时回调
    public enum TimerClass { // 闹钟类型：一次性闹钟，反复闹的闹钟，等
        None,
        OnceTimer,
        OnceWaitTimer,
        RepeatedTimer,
    }
    public class TimerAction { // 主要负责闹钟相关的逻辑：创建生成一个闹钟；应用用完了，回收闹钟等
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
// 就像我的项目里会有分门别类的管理类，这个全局全权负责管理闹钟的：
    public class TimerComponent: Singleton<TimerComponent>, ISingletonUpdate {
        // key: time, value: timer id
        private readonly MultiMap<long, long> TimeId = new(); // 管理无数个闹钟，键为超时时间升序
        private readonly Queue<long> timeOutTime = new();
        private readonly Queue<long> timeOutTimerIds = new(); // 超时的闹钟实例 id 
        private readonly Dictionary<long, TimerAction> timerActions = new();
        private long idGenerator;
        // 记录最小时间，不用每次都去MultiMap取第一个值
        private long minTime = long.MaxValue; // 永远追踪一个－－接下来的闹钟时间
        private long GetId() {
            return ++this.idGenerator;
        }
        private static long GetNow() {
            return TimeHelper.ClientFrameTime();
        }
        public void Update() {
            if (this.TimeId.Count == 0) {
                return;
            }
            long timeNow = GetNow();
            if (timeNow < this.minTime) {
                return;
            }
            foreach (KeyValuePair<long, List<long>> kv in this.TimeId) {
                long k = kv.Key;
                if (k > timeNow) {
                    this.minTime = k;
                    break;
                }
                this.timeOutTime.Enqueue(k);
            }
            while (this.timeOutTime.Count > 0) {
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
                
                this.Run(timerAction);
            }
        }
        private void Run(TimerAction timerAction) {
            switch (timerAction.TimerClass) {
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
            ETTask tcs = ETTask.Create(true); // 它创建一个任务
            TimerAction timer = TimerAction.Create(this.GetId(), TimerClass.OnceWaitTimer, timeNow, tillTime - timeNow, 0, tcs);
            this.AddTimer(timer);
            long timerId = timer.Id;
            void CancelAction() { // 内部定义一个取消回调
                if (this.Remove(timerId)) {
                    tcs.SetResult();
                }
            }
            try {
                cancellationToken?.Add(CancelAction); // 注册回调
                await tcs; // 等待计时超时任务完成
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
        // 【自己】：上面别人的原注理解不是很深，大概就是 WaitTillAsync() 的逻辑是写死了的，就不能热更新；但是这个回调（地狱）回调到标注过的类，就动态多了，就可以热更新了？
        public long NewOnceTimer(long tillTime, int type, object args) { // 创建一个一次性闹钟，注册了回高类
            long timeNow = GetNow();
            if (tillTime < timeNow) { // 还没创建闹钟，就已经超时了，不好玩
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