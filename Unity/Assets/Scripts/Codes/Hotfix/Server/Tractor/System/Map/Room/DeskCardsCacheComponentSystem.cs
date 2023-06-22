using ET;
namespace ET.Server {

    [FriendOfAttribute(typeof(ET.DeskCardsCacheComponent))]
    public static class DeskCardsCacheComponentSystem {
        // 获取总权值
        public static int GetTotalWeight(this DeskCardsCacheComponent self) {
            return CardsHelper.GetWeight(self.library.ToArray(), self.Rule);
        }
        // 获取牌桌所有牌
        public static Card[] GetAll(this DeskCardsCacheComponent self) {
            return self.library.ToArray();
        }
        // 发牌
        public static Card Deal(this DeskCardsCacheComponent self) {
            Card card = self.library[self.CardsCount - 1];
            self.library.Remove(card);
            return card;
        }
        // 向牌库中添加牌
        public static void AddCard(this DeskCardsCacheComponent self, Card card) {
            self.library.Add(card);
        }
        // 清空牌桌
        public static void Clear(this DeskCardsCacheComponent self) {
            // 【报错：】它说，禁止在 entity 类中直接调用 Child 和Component 。不知道说的是什么意思，但理解需要，就是去拿这个组件的 reference, 去想想有什么办法可以拿到？活宝妹就是一定要嫁给亲爱的表哥！！！爱表哥，爱生活！！！
            DeckComponent deck = self.GetParent<Room>().GetComponent<DeckComponent>();
            while (self.CardsCount > 0) {
                Card card = self.library[self.CardsCount - 1];
                self.library.Remove(card);
                deck.AddCard(card);
            }
            self.Rule = CardsType.None;
        }
        // 手牌排序
        public static void Sort(this DeskCardsCacheComponent self) {
            CardsHelper.SortCards(self.library);
        }
    }
}
