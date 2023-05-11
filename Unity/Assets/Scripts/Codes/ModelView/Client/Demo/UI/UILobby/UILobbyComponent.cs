
using UnityEngine;
using UnityEngine.UI;
namespace ET.Client {

    // 拖拉机厅：是要修改的地方，三个按钮，选择模式
    [ComponentOf(typeof(UI))]
    public class UILobbyComponent : Entity, IAwake {

        public GameObject matchRoom;
        public GameObject enterRoom;
        public GameObject createRoom;

        // 提示信息：其实可以不用 todo
        public Text text;
    }
}
