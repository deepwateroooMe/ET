namespace ET.Client
{
    public abstract class AUIEvent
    {
        // 这里就是UI组件的添加与卸载:可以分别需要做点儿什么呢  ？
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer);
        public abstract void OnRemove(UIComponent uiComponent);
    }
}