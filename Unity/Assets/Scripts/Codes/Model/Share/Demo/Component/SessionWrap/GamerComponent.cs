using System.Linq;
using System.Collections.Generic;

namespace ET {
    [ComponentOf(typeof(Room))]
    public class GamerComponent : Entity, IAwake {
        public Dictionary<long, int> seats = new Dictionary<long, int>();
        public Gamer[] gamers = new Gamer[3];
        public Gamer LocalGamer { get; set; }
    }
}