using System.Collections.Generic;
using System.Linq;

namespace ET.Server
{
	[ComponentOf(typeof(Scene))] // 不知道这个系统台式机的处理延迟得要多久，删除了说找不到类，不删除说重复了。。。
	public class PlayerComponent : Entity, IAwake, IDestroy
	{
	}
}