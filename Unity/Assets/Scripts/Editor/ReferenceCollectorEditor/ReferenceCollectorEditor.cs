using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
// Object并非C#基础中的Object，而是 UnityEngine.Object
using Object = UnityEngine.Object;
// 亲爱的表哥的活宝妹，现在再读这些Unity-Editor 相关的自定义功能，感觉好简单！一口气，把这个类、功能都读完、理解透彻！！

// 自定义ReferenceCollector类在界面中的显示与功能【源】：也还是狠简单。【TODO】：面板上的【拖拽复制功能】没看，改天再看那个
[CustomEditor(typeof (ReferenceCollector))]
public class ReferenceCollectorEditor: Editor {
    // 输入在textfield中的字符串
    private string searchKey {
        get {
            return _searchKey;
        }
        set {
            if (_searchKey != value) {
                _searchKey = value;
                heroPrefab = referenceCollector.Get<Object>(searchKey);
            }
        }
    }

    private ReferenceCollector referenceCollector;
    private Object heroPrefab;
    private string _searchKey = "";
    private void DelNullReference() { // 删除：没有赋值的空条目
        var dataProperty = serializedObject.FindProperty("data");
        for (int i = dataProperty.arraySize - 1; i >= 0; i--) { // 从尾巴向前遍历
            var gameObjectProperty = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
            if (gameObjectProperty.objectReferenceValue == null) { // 值为空、不曾赋值的、条目：删除
                dataProperty.DeleteArrayElementAtIndex(i);
				// 标记更新了：每删除一个，更新一次，Editor 是需要直接反应的
                EditorUtility.SetDirty(referenceCollector);
                serializedObject.ApplyModifiedProperties();
                serializedObject.UpdateIfRequiredOrScript();
            }
        }
    }
    private void OnEnable() {
        // 将被选中的gameobject所挂载的ReferenceCollector赋值给编辑器类中的ReferenceCollector，方便操作【源】
		// targets属性代表的是The object being inspected，即正在检视面板中操作的对象，这里表示的就是ReferenceCollector类的对象【抄自网络】
		// 上面说得有点儿绕：选中一个 gameObject 【它是 target?】, 它挂载着一个 ReferenceCollector 实例；将这个实例，赋值给当前类的成员变量
        referenceCollector = (ReferenceCollector) target; // target: Unity 里的定义
    }

    public override void OnInspectorGUI() { // 重载：实现，自定义面板，显示面板
        // 使ReferenceCollector支持撤销操作，还有Redo，不过没有在这里使用【源】：现在的Unity 里，直接面板上也是还没有【撤销】功能的
        Undo.RecordObject(referenceCollector, "Changed Settings"); // 暂时不用管它。如同安卓Paint 应用里，必要的纪录
        var dataProperty = serializedObject.FindProperty("data");
        // 【一行、水平布局】：如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
        GUILayout.BeginHorizontal();
        // 下面几个if都是点击按钮就会返回true调用里面的东西
        if (GUILayout.Button("添加引用")) {
            // 添加新的元素，具体的函数注释
            // Guid.NewGuid().GetHashCode().ToString() 就是新建后默认的key
            AddReference(dataProperty, Guid.NewGuid().GetHashCode().ToString(), null);
        }
        if (GUILayout.Button("全部删除")) {
            referenceCollector.Clear();
        }
        if (GUILayout.Button("删除空引用")) {
            DelNullReference();
        }
        if (GUILayout.Button("排序")) {
            referenceCollector.Sort();
        }
        EditorGUILayout.EndHorizontal();
        // 【一行、水平布局】：如果是比较新版本学习U3D的，可能不知道这东西，这个是老GUI系统的知识，除了用在编辑器里，还可以用在生成的游戏中
        EditorGUILayout.BeginHorizontal();
        // 可以在编辑器中对searchKey进行赋值，只要输入对应的Key值，就可以点后面的删除按钮删除相对应的元素
        searchKey = EditorGUILayout.TextField(searchKey);
        // 添加的可以用于选中Object的框，这里的object也是(UnityEngine.Object
        // 第三个参数为是否只能引用scene中的Object
// Unity-API: 添加了用户可选UnityEngine.Object 的下拉列表框，并把用户选中的 object 存入 heroPrefab 【自己的理解】
        EditorGUILayout.ObjectField(heroPrefab, typeof (Object), false); 
        if (GUILayout.Button("删除")) {
            referenceCollector.Remove(searchKey);
            heroPrefab = null;
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.Space(); // 两行：用户操作命令按钮行后，加一空行
        var delList = new List<int>();
        SerializedProperty property;
        // 遍历ReferenceCollector中data list的所有元素，显示在编辑器中
        for (int i = referenceCollector.data.Count - 1; i >= 0; i--) {
            GUILayout.BeginHorizontal();
            // 这里的知识点在ReferenceCollector中有说
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");
            EditorGUILayout.TextField(property.stringValue, GUILayout.Width(150));
            property = dataProperty.GetArrayElementAtIndex(i).FindPropertyRelative("gameObject");
            property.objectReferenceValue = EditorGUILayout.ObjectField(property.objectReferenceValue, typeof(Object), true);
            if (GUILayout.Button("X")) {
                // 将元素添加进删除list
                delList.Add(i);
            }
            GUILayout.EndHorizontal();
        }
		// 还定义了几个方便界面小事件：【拖拽、复制等功能】这几个小细节就暂时不看了，还不能用
        var eventType = Event.current.type;
        // 在Inspector 窗口上创建区域，向区域拖拽资源对象，获取到拖拽到区域的对象
        if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform) {
            // Show a copy icon on the drag
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (eventType == EventType.DragPerform) {
                DragAndDrop.AcceptDrag();
                foreach (var o in DragAndDrop.objectReferences) {
                    AddReference(dataProperty, o.name, o);
                }
            }
            Event.current.Use();
        }
        // 遍历删除list，将其删除掉
        foreach (var i in delList) {
            dataProperty.DeleteArrayElementAtIndex(i);
        }
        serializedObject.ApplyModifiedProperties();
        serializedObject.UpdateIfRequiredOrScript();
    }

    // 添加元素，具体知识点在ReferenceCollector中说了：直接在【尾巴上】添加一个新元素
    private void AddReference(SerializedProperty dataProperty, string key, Object obj) {
        int index = dataProperty.arraySize;
        dataProperty.InsertArrayElementAtIndex(index); // 在尾巴上，添加
        var element = dataProperty.GetArrayElementAtIndex(index); // 尾巴上添加的对象
		// 对所添加在尾巴上、新元素的、2 个成员变量赋值
        element.FindPropertyRelative("key").stringValue = key;
        element.FindPropertyRelative("gameObject").objectReferenceValue = obj;
    }
}
