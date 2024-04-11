namespace ET {
	// 框架封装的标签系里：Invoke 标签 
    public class InvokeAttribute: BaseAttribute {
        public int Type { get; }
        public InvokeAttribute(int type = 0) {
            this.Type = type;
        }
    }
}