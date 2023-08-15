using System.Collections.Generic;
namespace ET.Server {
    public static class RealmGateAddressHelper {
        public static StartSceneConfig GetGate(int zone) {
            List<StartSceneConfig> zoneGates = StartSceneConfigCategory.Instance.Gates[zone];
            int n = RandomGenerator.RandomNumber(0, zoneGates.Count); // 随机分配一个【网关服】
            return zoneGates[n];
        }
        // 我暂时就直接添加在这里: 下成的方法是自己整出来的. 因为自己把【匹配服】弄成了仅只一个，就不再需要下面的方法了
        public static StartSceneConfig GetMatch(int zone) {
            // List<StartSceneConfig> zoneMatchs = StartSceneConfigCategory.Instance.Match; // 匹配服并不分区
            // int n = RandomGenerator.RandomNumber(0, zoneMatchs.Count);
            // return zoneMatchs[n];
            return StartSceneConfigCategory.Instance.Match;
        }
    }
} // 【爱表哥，爱生活！！！任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】