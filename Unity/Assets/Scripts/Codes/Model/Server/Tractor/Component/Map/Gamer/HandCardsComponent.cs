﻿using System.Collections.Generic;
using ET;
namespace ET.Server {
    public class HandCardsComponent : Entity, IAwake {
        // 所有手牌
        public readonly List<Card> library = new List<Card>();
        // 身份
        public Identity AccessIdentity { get; set; }
        // 是否托管
        public bool IsTrusteeship { get; set; }
        // 手牌数
        public int CardsCount { get { return library.Count; } }
        public override void Dispose() {
            if (this.IsDisposed) {
                return;
            }
            base.Dispose();
            this.library.Clear();
            AccessIdentity = Identity.None;
            IsTrusteeship = false;
        }
    }
}