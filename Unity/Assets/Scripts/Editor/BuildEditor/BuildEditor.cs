﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;
namespace ET {

    public enum PlatformType {
        None,
        Android,
        IOS,
        Windows,
        MacOS,
        Linux
    }
    public enum BuildType {
        Development,
        Release,
    }

    public class BuildEditor : EditorWindow {
        private PlatformType activePlatform;
        private PlatformType platformType;
        private bool clearFolder;
        private bool isBuildExe;
        private bool isContainAB;
        private CodeOptimization codeOptimization = CodeOptimization.Debug;
        private BuildOptions buildOptions;
        private BuildAssetBundleOptions buildAssetBundleOptions = BuildAssetBundleOptions.None;
        private GlobalConfig globalConfig;
        [MenuItem("ET/Build Tool")]
        public static void ShowWindow() {
            GetWindow<BuildEditor>(DockDefine.Types);
        }
        private void OnEnable() {
            globalConfig = Resources.Load<GlobalConfig>("GlobalConfig");
#if UNITY_ANDROID
            activePlatform = PlatformType.Android;
#elif UNITY_IOS
            activePlatform = PlatformType.IOS;
#elif UNITY_STANDALONE_WIN
            activePlatform = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
            activePlatform = PlatformType.MacOS;
#elif UNITY_STANDALONE_LINUX
            activePlatform = PlatformType.Linux;
#else
            activePlatform = PlatformType.None;
#endif
            platformType = activePlatform;
        }
        private void OnGUI()  {
            this.platformType = (PlatformType)EditorGUILayout.EnumPopup(platformType);
            this.clearFolder = EditorGUILayout.Toggle("clean folder? ", clearFolder);
            this.isBuildExe = EditorGUILayout.Toggle("build exe?", this.isBuildExe);
            this.isContainAB = EditorGUILayout.Toggle("contain assetsbundle?", this.isContainAB);
            this.codeOptimization = (CodeOptimization)EditorGUILayout.EnumPopup("CodeOptimization ", this.codeOptimization);
            EditorGUILayout.LabelField("BuildAssetBundleOptions ");
            this.buildAssetBundleOptions = (BuildAssetBundleOptions)EditorGUILayout.EnumFlagsField(this.buildAssetBundleOptions);
            switch (this.codeOptimization) {
                case CodeOptimization.None:
                case CodeOptimization.Debug:
                    this.buildOptions = BuildOptions.Development | BuildOptions.ConnectWithProfiler;
                    break;
                case CodeOptimization.Release:
                    this.buildOptions = BuildOptions.None;
                    break;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("BuildPackage")) {
                if (this.platformType == PlatformType.None) {
                    ShowNotification(new GUIContent("please select platform!"));
                    return;
                }
                if (platformType != activePlatform) {
                    switch (EditorUtility.DisplayDialogComplex("Warning!", $"current platform is {activePlatform}, if change to {platformType}, may be take a long time", "change", "cancel", "no change"))
                    {
                        case 0:
                            activePlatform = platformType;
                            break;
                        case 1:
                            return;
                        case 2:
                            platformType = activePlatform;
                            break;
                    }
                }
                BuildHelper.Build(this.platformType, this.buildAssetBundleOptions, this.buildOptions, this.isBuildExe, this.isContainAB, this.clearFolder);
            }
            GUILayout.Label("");
            GUILayout.Label("Code Compile：");
            var codeMode = (CodeMode)EditorGUILayout.EnumPopup("CodeMode: ", this.globalConfig.CodeMode);
            if (codeMode != this.globalConfig.CodeMode) {
                this.globalConfig.CodeMode = codeMode;
                EditorUtility.SetDirty(this.globalConfig);
                AssetDatabase.SaveAssets();
            }
            if (GUILayout.Button("BuildModelAndHotfix")) {
                if (Define.EnableCodes) {
                    throw new Exception("now in ENABLE_CODES mode, do not need Build!");
                }
                BuildAssembliesHelper.BuildModel(this.codeOptimization, globalConfig);
                BuildAssembliesHelper.BuildHotfix(this.codeOptimization, globalConfig);
                AfterCompiling();
                
                ShowNotification("Build Model And Hotfix Success!");
            }
            if (GUILayout.Button("BuildModel")) {
                if (Define.EnableCodes) {
                    throw new Exception("now in ENABLE_CODES mode, do not need Build!");
                }
                BuildAssembliesHelper.BuildModel(this.codeOptimization, globalConfig);
                AfterCompiling();
                
                ShowNotification("Build Model Success!");
            }
            if (GUILayout.Button("BuildHotfix")) {
                if (Define.EnableCodes) 
                    throw new Exception("now in ENABLE_CODES mode, do not need Build!");
                BuildAssembliesHelper.BuildHotfix(this.codeOptimization, globalConfig);
                AfterCompiling();
                
                ShowNotification("Build Hotfix Success!");
            }
            if (GUILayout.Button("ExcelExporter")) { // 这里，明明是Unity IDE 双端开发工具：一键打包、更新热更新服务器的过程？
                // Directory.Delete("Assets/Bundles/Config", true);
                ToolsEditor.ExcelExporter(); // 去把，这个上午看过的过程，再快速地扫一遍
				// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】

                // 设置ab包【源】：下面，说是从这个路径来读取资源包；但是客户端的、这个路径的资源包，应该是从网络上下载来，实时更新来的
                AssetImporter assetImporter = AssetImporter.GetAtPath($"Assets/Bundles/Config"); // 去框架里找：这个方法的实现细节【TODO】：改天再看这个
                assetImporter.assetBundleName = "Config.unity3d";
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            if (GUILayout.Button("Proto2CS")) {
                ToolsEditor.Proto2CS();
            }
            GUILayout.Space(5);
        }
        private static void AfterCompiling() {
            Directory.CreateDirectory(BuildAssembliesHelper.CodeDir);
            // 设置ab包
            AssetImporter assetImporter = AssetImporter.GetAtPath("Assets/Bundles/Code");
            assetImporter.assetBundleName = "Code.unity3d";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("build success!");
        }
        public static void ShowNotification(string tips) {
            EditorWindow game = EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
            game?.ShowNotification(new GUIContent($"{tips}"));
        }
    }
}
