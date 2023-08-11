using System.Collections.Generic;
namespace ET.Server {

    public static class RealmGateAddressHelper {
        public static StartSceneConfig GetGate(int zone) {
            List<StartSceneConfig> zoneGates = StartSceneConfigCategory.Instance.Gates[zone];
            int n = RandomGenerator.RandomNumber(0, zoneGates.Count);
            return zoneGates[n];
        }
        // 我暂时就直接添加在这里: 下成的方法是自己整出来的
        public static StartSceneConfig GetMatch(int zone) {
            // List<StartSceneConfig> zoneMatchs = StartSceneConfigCategory.Instance.Match; // 匹配服并不分区
            // int n = RandomGenerator.RandomNumber(0, zoneMatchs.Count);
            // return zoneMatchs[n];
            return StartSceneConfigCategory.Instance.Match;
        }
    }
}