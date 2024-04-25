using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using MongoDB.Bson.Serialization;
using OfficeOpenXml;
using ProtoBuf;
using LicenseContext = OfficeOpenXml.LicenseContext;
namespace ET {
    public enum ConfigType {
        c = 0,
        s = 1,
        cs = 2,
    }
    class HeadInfo {
        public string FieldCS;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;
        public HeadInfo(string cs, string desc, string name, string type, int index) {
            this.FieldCS = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
            this.FieldIndex = index;
        }
    }
    // 这里加个标签是为了防止编译时裁剪掉protobuf，因为整个tool工程没有用到protobuf，编译会去掉引用，然后动态编译就会出错
    [ProtoContract]
    class Table {
        public bool C; // Client 
        public bool S; // Server?
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new Dictionary<string, HeadInfo>();
    }
    public static class ExcelExporter {
        private static string template;
        private const string ClientClassDir = "../Unity/Assets/Scripts/Codes/Model/Generate/Client/Config";
        // 服务端因为机器人的存在必须包含客户端所有配置，所以单独的c字段没有意义,单独的c就表示cs
        private const string ServerClassDir = "../Unity/Assets/Scripts/Codes/Model/Generate/Server/Config";
        private const string CSClassDir = "../Unity/Assets/Scripts/Codes/Model/Generate/ClientServer/Config";
        private const string excelDir = "../Unity/Assets/Config/Excel/";
        private const string jsonDir = "../Config/Json/{0}/{1}";
        private const string clientProtoDir = "../Unity/Assets/Bundles/Config";
        private const string serverProtoDir = "../Config/Excel/{0}/{1}";
        private static Assembly[] configAssemblies = new Assembly[3];
        private static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        private static Dictionary<string, ExcelPackage> packages = new Dictionary<string, ExcelPackage>();
        private static Table GetTable(string protoName) {
            if (!tables.TryGetValue(protoName, out var table)) {
                table = new Table();
                tables[protoName] = table;
            }
            return table;
        }
        public static ExcelPackage GetPackage(string filePath) {
            if (!packages.TryGetValue(filePath, out var package)) {
                using Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                package = new ExcelPackage(stream);
                packages[filePath] = package;
            }
            return package;
        }
        public static void Export() { // 看这个从Excel 对服务端？【物理机、进程、场景、Zone】等的配置，帮助【导入内存，或写到C#.bytes 字节流，这个过程看懂】的逻辑
            try {
                // 防止编译时裁剪掉protobuf
                ProtoBuf.WireType.Fixed64.ToString(); // 随便调用一个Protobuf 里的函数：人为手动添加一个对、ProtoBuf 程序域的调用引用，防裁剪 defensive-programming?
				// 下面，为什么它需要一个模板呢？生成不同的 .cs 的时候，它只修改宏变量，就可以了，是直接套用模板的
                template = File.ReadAllText("Template.txt");
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                if (Directory.Exists(ClientClassDir)) { // 删除存在的目录
                    Directory.Delete(ClientClassDir, true);
                }
                if (Directory.Exists(ServerClassDir)) {
                    Directory.Delete(ServerClassDir, true);
                }
                List<string> files = FileHelper.GetAllFiles(excelDir); // 所有 Excel 的配置文件
                foreach (string path in files) {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#")) {
                        continue;
                    }
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs"; // 相要区分端：S-Server, C-Client,cs- 双端 
                    if (fileNameWithoutExtension.Contains("@")) {
                        string[] ss = fileNameWithoutExtension.Split("@");
                        fileNameWithoutCS = ss[0];
                        cs = ss[1];
                    }
                    if (cs == "") {
                        cs = "cs";
                    }
                    ExcelPackage p = GetPackage(Path.GetFullPath(path));
                    string protoName = fileNameWithoutCS;
                    if (fileNameWithoutCS.Contains('_')) {
                        protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
                    }
                    Table table = GetTable(protoName);
                    if (cs.Contains("c")) {
                        table.C = true;
                    }
                    if (cs.Contains("s")) {
                        table.S = true;
                    }
                    ExportExcelClass(p, protoName, table); // 【TODO】：先看这里
                }
                foreach (var kv in tables) {
                    if (kv.Value.C) {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }
                    if (kv.Value.S) {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s);
                    }
                    ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.cs);
                }
                // 动态编译生成的配置代码【源】：【TODO】：这里也是先前没看懂的。动态编译【服务端、客户端、和双端模式下】动态启动库：四大启动配制类
                configAssemblies[(int) ConfigType.c] = DynamicBuild(ConfigType.c);
                configAssemblies[(int) ConfigType.s] = DynamicBuild(ConfigType.s);
                configAssemblies[(int) ConfigType.cs] = DynamicBuild(ConfigType.cs);
                List<string> excels = FileHelper.GetAllFiles(excelDir, "*.xlsx");
                
                foreach (string path in excels) {
                    ExportExcel(path); // 到这里，服务端的四大类配置文件、编译、写成 .bytes 完成
                }
                
                if (Directory.Exists(clientProtoDir)) { // 更新客户端目录
                    Directory.Delete(clientProtoDir, true);
                }
                FileHelper.CopyDirectory("../Config/Excel/c", clientProtoDir);
                
                Log.Console("Export Excel Sucess!");
            }
            catch (Exception e) {
                Log.Console(e.ToString());
            }
            finally {
                tables.Clear();
                foreach (var kv in packages) {
                    kv.Value.Dispose();
                }
                packages.Clear();
            }
        }
        private static void ExportExcel(string path) {
            string dir = Path.GetDirectoryName(path);
            string relativePath = Path.GetRelativePath(excelDir, dir);
            string fileName = Path.GetFileName(path);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#")) {
                return;
            }
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithoutCS = fileNameWithoutExtension;
            string cs = "cs";
            if (fileNameWithoutExtension.Contains("@")) {
                string[] ss = fileNameWithoutExtension.Split("@");
                fileNameWithoutCS = ss[0];
                cs = ss[1];
            }
            if (cs == "") {
                cs = "cs";
            }
            string protoName = fileNameWithoutCS;
            if (fileNameWithoutCS.Contains('_')) {
                protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
            }
            Table table = GetTable(protoName);
            ExcelPackage p = GetPackage(Path.GetFullPath(path));
            if (cs.Contains("c")) {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.c, relativePath);
                ExportExcelProtobuf(ConfigType.c, protoName, relativePath);
            }
            if (cs.Contains("s")) {
                ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.s, relativePath);
                ExportExcelProtobuf(ConfigType.s, protoName, relativePath);
            }
            ExportExcelJson(p, fileNameWithoutCS, table, ConfigType.cs, relativePath);
            ExportExcelProtobuf(ConfigType.cs, protoName, relativePath);
        }
        private static string GetProtoDir(ConfigType configType, string relativeDir) {
            return string.Format(serverProtoDir, configType.ToString(), relativeDir);
        }
        private static Assembly GetAssembly(ConfigType configType) {
            return configAssemblies[(int) configType];
        }
        private static string GetClassDir(ConfigType configType) {
            return configType switch {
                ConfigType.c => ClientClassDir,
                ConfigType.s => ServerClassDir,
                _ => CSClassDir
            };
        }
        // 动态编译生成的cs代码：【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
        private static Assembly DynamicBuild(ConfigType configType) {
            string classPath = GetClassDir(configType); // 每种模式，有其固定的 .cs 文件的存放路径
            List<SyntaxTree> syntaxTrees = new List<SyntaxTree>();
            List<string> protoNames = new List<string>(); // 文件名，链条
            foreach (string classFile in Directory.GetFiles(classPath, "*.cs")) { // 有四大类的生成的 .cs 文件
                protoNames.Add(Path.GetFileNameWithoutExtension(classFile));
                syntaxTrees.Add(CSharpSyntaxTree.ParseText(File.ReadAllText(classFile))); // 语法树。。
            }
            List<PortableExecutableReference> references = new List<PortableExecutableReference>(); // 索引 
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies(); // 现【程序域】里：所有引用到的、必要的程序域、引用索引的添加。要不然某些程序域可能被裁剪吧
            foreach (Assembly assembly in assemblies) { // 遍历，当前【程序域】里所有包、索引包
                try { // 【TODO】：这个块，像是什么事也没有做。。。检错吗？出错会抛异常、某端启动不起来。。
                    if (assembly.IsDynamic) {
                        continue;
                    }
                    if (assembly.Location == "") {
                        continue;
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    throw; // 感觉像是，仅只是为了检错排错。。
                }
                PortableExecutableReference reference = MetadataReference.CreateFromFile(assembly.Location); // 每个添加 meta 索引 
                references.Add(reference); // 记入索引，否则裁剪掉、不要
            }
			// 【编译成、动态库】
            CSharpCompilation compilation = CSharpCompilation.Create(null,
                syntaxTrees.ToArray(),
                references.ToArray(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using MemoryStream memSteam = new MemoryStream();
            EmitResult emitResult = compilation.Emit(memSteam);
            if (!emitResult.Success) {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (Diagnostic t in emitResult.Diagnostics) {
                    stringBuilder.Append($"{t.GetMessage()}\n");
                }
                throw new Exception($"动态编译失败:\n{stringBuilder}");
            }
            memSteam.Seek(0, SeekOrigin.Begin);
            Assembly ass = Assembly.Load(memSteam.ToArray());
            return ass;
        }
        #region 导出class
        static void ExportExcelClass(ExcelPackage p, string name, Table table) {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets) {
                ExportSheetClass(worksheet, table);
            }
        }
        static void ExportSheetClass(ExcelWorksheet worksheet, Table table) { // 读出 Excel/Sheet 里的信息，封装到某个 HeadInfo-table 里
            const int row = 2;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col) {
                if (worksheet.Name.StartsWith("#")) {
                    continue;
                }
                string fieldName = worksheet.Cells[row + 2, col].Text.Trim(); // 格式固定，Excel 表格里第3 行是【列的名称】
                if (fieldName == "") {
                    continue;
                }
                if (table.HeadInfos.ContainsKey(fieldName)) {
                    continue;
                }
                string fieldCS = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCS.Contains("#")) {
                    table.HeadInfos[fieldName] = null;
                    continue;
                }
                
                if (fieldCS == "") {
                    fieldCS = "cs";
                }
                if (table.HeadInfos.TryGetValue(fieldName, out var oldClassField)) {
                    if (oldClassField.FieldCS != fieldCS) {
                        Log.Console($"field cs not same: {worksheet.Name} {fieldName} oldcs: {oldClassField.FieldCS} {fieldCS}");
                    }
                    continue;
                }
                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();
                table.HeadInfos[fieldName] = new HeadInfo(fieldCS, fieldDesc, fieldName, fieldType, ++table.Index);
            }
        } // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
		// 套Template.txt 模块：现在就根据 Excel 里读到了服务端的四大类的配置，为双端启动模式下，与客户端的交互？生成Protobuf 跨进程配置消息类
        static void ExportClass(string protoName, Dictionary<string, HeadInfo> classField, ConfigType configType) {
            string dir = GetClassDir(configType);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
			// 服务端、四大主要的配置类型，生成对应的可【跨进程传递消息】的 ProtoBuf-contract 的 .cs 类
			string exportPath = Path.Combine(dir, $"{protoName}.cs");
            using FileStream txt = new FileStream(exportPath, FileMode.Create); // 创建了，相应类型的 .cs 文件
            using StreamWriter sw = new StreamWriter(txt);
            StringBuilder sb = new StringBuilder();
            foreach ((string _, HeadInfo headInfo) in classField) {
                if (headInfo == null) {
                    continue;
                }
				// 如果不是【双端、启动模式】，不会自己生成。【双端模式】：是服务端也是客户端！所以会生成双端合约，与客户端共享；客户端应该是之后得到服务端的这些配置信息
                if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(configType.ToString())) {
                    continue;
                }
				// 套用模板：赋值宏变量，生成四大类的双端共通、ProtoBuf 跨进程配置消息类
                sb.Append($"\t\t// <summary>{headInfo.FieldDesc}</summary>\n");
                sb.Append($"\t\t[ProtoMember({headInfo.FieldIndex})]\n");
                string fieldType = headInfo.FieldType;
                sb.Append($"\t\tpublic {fieldType} {headInfo.FieldName} {{ get; set; }}\n");
            }
            string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());
            sw.Write(content);
        }
#endregion // 导出 json: 作什么用的呢？
        #region 导出json
        static void ExportExcelJson(ExcelPackage p, string name, Table table, ConfigType configType, string relativeDir) {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"list\":[\n");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets) {
                if (worksheet.Name.StartsWith("#")) {
                    continue;
                }
                ExportSheetJson(worksheet, name, table.HeadInfos, configType, sb);
            }
            sb.Append("]}\n");
            string dir = string.Format(jsonDir, configType.ToString(), relativeDir);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            string jsonPath = Path.Combine(dir, $"{name}.txt");
            using FileStream txt = new FileStream(jsonPath, FileMode.Create);
            using StreamWriter sw = new StreamWriter(txt);
            sw.Write(sb.ToString());
        }
        static void ExportSheetJson(ExcelWorksheet worksheet, string name, 
                Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb) {
            string configTypeStr = configType.ToString();
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row) {
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#")) {
                    continue;
                }
                if (prefix == "") {
                    prefix = "cs";
                }
                
                if (configType != ConfigType.cs && !prefix.Contains(configTypeStr)) {
                    continue;
                }
                if (worksheet.Cells[row, 3].Text.Trim() == "") {
                    continue;
                }
                sb.Append("{");
                sb.Append($"\"_t\":\"{name}\"");
                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col) {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!classField.ContainsKey(fieldName)) {
                        continue;
                    }
                    HeadInfo headInfo = classField[fieldName];
                    if (headInfo == null) {
                        continue;
                    }
                    if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(configTypeStr)) {
                        continue;
                    }
                    string fieldN = headInfo.FieldName;
                    if (fieldN == "Id") {
                        fieldN = "_id";
                    }
                    sb.Append($",\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }
                sb.Append("},\n");
            }
        }
        private static string Convert(string type, string value) {
            switch (type) {
                case "uint[]":
                case "int[]":
                case "int32[]":
                case "long[]":
                    return $"[{value}]";
                case "string[]":
                case "int[][]":
                    return $"[{value}]";
                case "int":
                case "uint":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    if (value == "") {
                        return "0";
                    }
                    return value;
                case "string":
                    value = value.Replace("\\", "\\\\");
                    value = value.Replace("\"", "\\\"");
                    return $"\"{value}\"";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }
        #endregion
        // 根据生成的类，把json转成protobuf 【源】：转了之后，感觉像是【跨进程传递了的？】可以传到客户端？得把这些弄明白，
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
		// 亲爱的表哥的活宝妹，今天下午算是找到了无数、亲爱的表哥的活宝妹还看不懂的启动配置相关。。。
		// 亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！
        private static void ExportExcelProtobuf(ConfigType configType, string protoName, string relativeDir) {
            string dir = GetProtoDir(configType, relativeDir); // Unity 同级目录下 Config/Excel/cs/..*.bytes 目标文件
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            Assembly ass = GetAssembly(configType);
            Type type = ass.GetType($"ET.{protoName}Category");
            Type subType = ass.GetType($"ET.{protoName}");
            Serializer.NonGeneric.PrepareSerializer(type);
            Serializer.NonGeneric.PrepareSerializer(subType);
            IMerge final = Activator.CreateInstance(type) as IMerge; // 这里创建了实例
            string p = Path.Combine(string.Format(jsonDir, configType, relativeDir));
            string[] ss = Directory.GetFiles(p, $"{protoName}*.txt");
            List<string> jsonPaths = ss.ToList();
            jsonPaths.Sort();
            jsonPaths.Reverse();
            foreach (string jsonPath in jsonPaths) {
                string json = File.ReadAllText(jsonPath);
                try {
                    object deserialize = BsonSerializer.Deserialize(json, type);
                    final.Merge(deserialize); // 这一句是精华 
                }
                catch {
                    #region 为了定位该文件中具体那一行出现了异常
                    List<string> list = new List<string>(json.Split('\n'));
                    if (list.Count > 0)
                        list.RemoveAt(0);
                    if (list.Count > 0)
                        list.RemoveAt(list.Count-1);
                    foreach (string s in list) {
                        try
                        {
                            BsonSerializer.Deserialize(s.Substring(0, s.Length-1), subType);
                        }
                        catch (Exception)
                        {
                            Log.Console($"json : {s}");
                            throw;
                        }
                    }
                    #endregion
                }
            }
            string path = Path.Combine(dir, $"{protoName}Category.bytes"); // 最终写入到这个【服务端配置目录】里的、四大类配置文件 .bytes 
            using FileStream file = File.Create(path);
            Serializer.Serialize(file, final); // 不同服务端四大类【物理机、进程、场景、区】IMerge 后的结果，写到了文件里，是合并后的服务端信息
        }
    }
}
 // 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！】
