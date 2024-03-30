using System.Collections.Generic;
using System.IO;
using HybridCLR.Editor;
using UnityEditor;

namespace ET {
    public static class HybridCLREditor
    {
        [MenuItem("HybridCLR/CopyAotDlls")]
        public static void CopyAotDll()
        {
            BuildTarget target = EditorUserBuildSettings.activeBuildTarget;
			// 因为这里有2 个会报错，暂时去掉
            // string fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
            string fromDir = "";
            string toDir = "Assets/Bundles/AotDlls";
            if (Directory.Exists(toDir))
            {
                Directory.Delete(toDir, true);
            }
            Directory.CreateDirectory(toDir);
            AssetDatabase.Refresh();
            
			// 因为这里有2 个会报错，暂时去掉
            // foreach (string aotDll in HybridCLRSettings.Instance.patchAOTAssemblies) {
            //     File.Copy(Path.Combine(fromDir, aotDll), Path.Combine(toDir, $"{aotDll}.bytes"), true);
            // }
            
            // 设置ab包
            AssetImporter assetImporter = AssetImporter.GetAtPath(toDir);
            assetImporter.assetBundleName = "AotDlls.unity3d";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}