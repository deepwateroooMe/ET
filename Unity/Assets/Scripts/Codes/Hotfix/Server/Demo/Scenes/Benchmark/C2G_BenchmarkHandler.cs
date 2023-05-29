using System;
using ET;

namespace ET.Server {
    [MessageHandler(SceneType.BenchmarkServer)]
    public class C2G_BenchmarkHandler: AMRpcHandler<C2G_Benchmark, G2C_Benchmark> {
        // protected override async ETTask Run(Session session, C2G_Benchmark request, G2C_Benchmark response) {            
		protected override void Run(Session session, C2G_Benchmark message, Action<G2C_Benchmark> reply) { // 暂时不用管它这里的逻辑。就是把方法定义参数改一下            
        // protected override void Run(Session session, C2G_Benchmark request, G2C_Benchmark response) { // 暂时不用管它这里的逻辑。就是把方法定义参数改一下            
            // BenchmarkServerComponent benchmarkServerComponent = session.DomainScene().GetComponent<BenchmarkServerComponent>();
            // if (benchmarkServerComponent.Count++ % 1000000 == 0) {
            //     Log.Debug($"benchmark count: {benchmarkServerComponent.Count} {TimeHelper.ClientNow()}");
            // }
            // // await ETTask.CompletedTask;
        }
	}   
}

