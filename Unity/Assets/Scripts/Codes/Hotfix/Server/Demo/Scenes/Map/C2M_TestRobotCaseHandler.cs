using System;
using ET;

namespace ET.Server {
    [ActorMessageHandler(SceneType.Map)]
    public class C2M_TestRobotCaseHandler : AMActorLocationRpcHandler<Unit, C2M_TestRobotCase, M2C_TestRobotCase> {

        // protected override async ETTask Run(Unit unit, C2M_TestRobotCase request, M2C_TestRobotCase response) {
        protected override void Run(Unit unit, C2M_TestRobotCase request, M2C_TestRobotCase response) { // 【报错：】禁止返回类型为void 的异步方法 
            response.N = request.N;
            //await ETTask.CompletedTask; // 暂时改成这样，就真不知道改对了没有？
        }
    }
}
