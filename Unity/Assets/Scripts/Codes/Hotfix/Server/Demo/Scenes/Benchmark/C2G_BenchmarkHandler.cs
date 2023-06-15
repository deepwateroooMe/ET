using System;
using ET;

namespace ET.Server {
    [MessageHandler(SceneType.BenchmarkServer)]
    public class C2G_BenchmarkHandler: AMRpcHandler<C2G_Benchmark, G2C_Benchmark> {
        // 这里是它：原本的框架里的方法，怎么过程中，会被我改坏掉的？以后改的时候要长下脑子
        protected override async ETTask Run(Session session, C2G_Benchmark request, G2C_Benchmark response) {
            BenchmarkServerComponent benchmarkServerComponent = session.DomainScene().GetComponent<BenchmarkServerComponent>();
            if (benchmarkServerComponent.Count++ % 1000000 == 0) {
                Log.Debug($"benchmark count: {benchmarkServerComponent.Count} {TimeHelper.ClientNow()}");
            }
            await ETTask.CompletedTask;
        }
	}   
}