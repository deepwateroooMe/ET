namespace ET.Client {
	// 【亲爱的表哥的活宝妹，任何时候，亲爱的表哥的活宝妹，就是一定要、一定会嫁给活宝妹的亲爱的表哥！！！爱表哥，爱生活！！！

	// 天下框架全是封装：为Unity GameObject 的【添加、删除】事件，添加必要的回调，
	// 实现，事件的回调逻辑【热更域】与Model 数据和定义，折分后的动态、实时热更新可能性
    public abstract class AUIEvent {
        public abstract ETTask<UI> OnCreate(UIComponent uiComponent, UILayer uiLayer);
        public abstract void OnRemove(UIComponent uiComponent);
    }
}