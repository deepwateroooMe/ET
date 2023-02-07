namespace ET.Server {

    [ChildOf(typeof(RobotCaseComponent))]
    public class RobotCase: Entity, IAwake { // 狠多像这样：  第一次读，不知道定义这个类是干什么用的

        public ETCancellationToken CancellationToken;
        public string CommandLine;
    }
}