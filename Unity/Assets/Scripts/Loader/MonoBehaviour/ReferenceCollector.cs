using System;
using System.Collections.Generic;
using UnityEngine;
// Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;

// Unity 中用户自定义一个【组件】的案例：这个组件ReferenceCollector 的功能是：为【TODO 哪个脚本？】添加必要的索引
// 需要可以【序列化、与反序列化】：因为ET 框架里广泛使用字典，而Unity 序列化不能用字典，就必须用ISerializationCallbackReceiver 官方接口，过程中借助List<>, 来实现对字典【序列化与反。。。】的支持
// 狠简单，一看就懂了。再去看Editor 相关的、一点儿必要Unity-Editor 逻辑

// 使其能在Inspector面板显示，并且可以被赋予相应值
[Serializable]
public class ReferenceCollectorData {
    public string key;
    // Object并非C#基础中的Object，而是 UnityEngine.Object
    public Object gameObject;
}

// 继承IComparer对比器，Ordinal会使用序号排序规则比较字符串，因为是byte级别的比较，所以准确性和性能都不错
public class ReferenceCollectorDataComparer: IComparer<ReferenceCollectorData> {
    public int Compare(ReferenceCollectorData x, ReferenceCollectorData y) { // 排序逻辑：按元素名字的 StringComparison.Ordinal 排序，什么 byte 比较？
        return string.Compare(x.key, y.key, StringComparison.Ordinal);
    }
}

// 继承ISerializationCallbackReceiver后会增加OnAfterDeserialize和OnBeforeSerialize两个回调函数，如果有需要可以在对需要序列化的东西进行操作
// ET在这里主要是在OnAfterDeserialize回调函数中将data中存储的ReferenceCollectorData转换为dict中的Object，方便之后的使用
// 注意UNITY_EDITOR宏定义，在编译以后，部分编辑器相关函数并不存在
public class ReferenceCollector: MonoBehaviour, ISerializationCallbackReceiver { // 注释比较多，参考网络上的解释，还是极容易懂的。用户自定义工具的一种 
    // 用于序列化的List 【源】：方便实现序列化字典
    public List<ReferenceCollectorData> data = new List<ReferenceCollectorData>();
    // Object并非C#基础中的Object，而是 UnityEngine.Object
    private readonly Dictionary<string, Object> dict = new Dictionary<string, Object>(); // 反序列化后，方便自己框架使用的字典
#if UNITY_EDITOR
    // 添加新的元素
    public void Add(string key, Object obj) {
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        // 根据PropertyPath读取数据
        // 如果不知道具体的格式，可以右键用文本编辑器打开一个prefab文件（如Bundles/UI目录中的几个）
        // 因为这几个prefab挂载了ReferenceCollector，所以搜索data就能找到存储的数据
        UnityEditor.SerializedProperty dataProperty = serializedObject.FindProperty("data"); // 此“data”，为这个类的成员变量 data 的名字
        int i; // 这里的问题是：它也不知道，要添加的 key 是否包含在 data 里，是UnityEditor.SerializedObject(this) 的神逻辑，惹得祸？！！
        // 遍历data，看添加的数据是否存在相同key
        for (i = 0; i < data.Count; i++) {
            if (data[i].key == key) {
                break;
            }
        }
        // 不等于data.Count意为已经存在于data List中，直接赋值即可
        if (i != data.Count) { // data 里，已经有 key 这个条目
            // 根据i的值获取dataProperty，也就是data中的对应ReferenceCollectorData，不过在这里，是对Property进行的读取，有点类似json或者xml的节点
            UnityEditor.SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
            // 对对应节点进行赋值，值为gameobject相对应的fileID
            // fileID独一无二，单对单关系，其他挂载在这个gameobject上的script或组件会保存相对应的fileID
            element.FindPropertyRelative("gameObject").objectReferenceValue = obj; // 赋值
        }
        else { // data 里，没有 key 这个条目，补充到最后一个条目 i=data.Count
            // 等于则说明key在data中无对应元素，所以得向其插入新的元素
            dataProperty.InsertArrayElementAtIndex(i); // 链条，最后添加一个节点
            UnityEditor.SerializedProperty element = dataProperty.GetArrayElementAtIndex(i);
			//me: 分别对 Data 的两个属性赋值 
            element.FindPropertyRelative("key").stringValue = key;
            element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
        }
        // 应用与更新
        UnityEditor.EditorUtility.SetDirty(this);   // 标记：有更新
        serializedObject.ApplyModifiedProperties(); // 使用上当前更新
		// UpdateIfRequiredOrScript(): Unity 一个更新API.表示更新序列化后的对象，仅只在，自上次Update() 之后对象被修改过，或者它是脚本时，才更新。最小必要更新
        serializedObject.UpdateIfRequiredOrScript(); //  
    }
    // 删除元素，知识点与上面的添加相似
    public void Remove(string key) {
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        UnityEditor.SerializedProperty dataProperty = serializedObject.FindProperty("data");
        int i;
        for (i = 0; i < data.Count; i++) {
            if (data[i].key == key) {
                break;
            }
        }
        if (i != data.Count) { // 一定在【0,size-1】下标范围内
            dataProperty.DeleteArrayElementAtIndex(i); // 直接删除
        }
// 通知更新
        UnityEditor.EditorUtility.SetDirty(this);
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }
	// 自定义组件，面板上的几个功能 
    public void Clear() { // 清空
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        // 根据PropertyPath读取prefab文件中的数据
        // 如果不知道具体的格式，可以直接右键用文本编辑器打开，搜索data就能找到
        var dataProperty = serializedObject.FindProperty("data");
        dataProperty.ClearArray(); // 清空；然后通知Unity 更新
        UnityEditor.EditorUtility.SetDirty(this);
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }
    public void Sort() { // 排序
        UnityEditor.SerializedObject serializedObject = new UnityEditor.SerializedObject(this);
        data.Sort(new ReferenceCollectorDataComparer()); // 同亲爱的表哥的活宝妹，无数个Java 算法题目里的、字典等的自定义排序一样，自定义排序函数的逻辑，就可以了
        UnityEditor.EditorUtility.SetDirty(this);
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }
#endif
    // 使用泛型返回对应key的gameobject
    public T Get<T>(string key) where T : class {
        Object dictGo;
        if (!dict.TryGetValue(key, out dictGo)) {
            return null;
        }
        return dictGo as T;
    }
    public Object GetObject(string key) {
        Object dictGo;
        if (!dict.TryGetValue(key, out dictGo)) {
            return null;
        }
        return dictGo;
    }

    public void OnBeforeSerialize() {
    }
    // 在反序列化后运行【源】：反序列化后，就把桥接List 转成，框架好用的字典
    public void OnAfterDeserialize() {
        dict.Clear();
        foreach (ReferenceCollectorData referenceCollectorData in data) {
            if (!dict.ContainsKey(referenceCollectorData.key)) {
                dict.Add(referenceCollectorData.key, referenceCollectorData.gameObject);
            }
        }
    }
}
