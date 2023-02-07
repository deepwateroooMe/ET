using System.Collections.Generic;
using HybridCLR;
using UnityEngine;
namespace ET
{
    public static class HybridCLRHelper
    {

        public static void Load()
        {
            // 因为游戏端存在这引动错误，所以查不到这个端8个项目的源码。。。。。            
            // 这里说的是什么意思呢？大致是说，想要加载这个热更新的程序域，但是mac平台没有这个程序域的文件 ？          
            Dictionary<string, UnityEngine.Object> dictionary = AssetsBundleHelper.LoadBundle("aotdlls.unity3d");
            foreach (var kv in dictionary)
            {
                byte[] bytes = (kv.Value as TextAsset).bytes;
                RuntimeApi.LoadMetadataForAOTAssembly(bytes, HomologousImageMode.Consistent);
            }
        }
    }
}