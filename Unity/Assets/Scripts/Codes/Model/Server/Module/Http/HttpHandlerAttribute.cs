namespace ET.Server {

// 《》：这里是定义自定义标注吗 ?
    public class HttpHandlerAttribute: BaseAttribute {

        public SceneType SceneType { get; }
        public string Path { get; }

        public HttpHandlerAttribute(SceneType sceneType, string path) {
            this.SceneType = sceneType;
            this.Path = path;
        }
    }
}