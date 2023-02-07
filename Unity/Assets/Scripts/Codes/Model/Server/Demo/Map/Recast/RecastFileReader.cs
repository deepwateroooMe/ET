using System.IO;
namespace ET.Server {

    [Invoke]
    public class RecastFileReader: AInvokeHandler<NavmeshComponent.RecastFileLoader, byte[]> {

 // 这里的意思：  大概是说，游戏中地图，某一片的地图Cell，可能都是由配置文件夹下的文件自动加载生成的。那么这里就提供了一个从文件中读取加载NavMesh的调用方法        
        public override byte[] Handle(NavmeshComponent.RecastFileLoader args) {
            return File.ReadAllBytes(Path.Combine("../Config/Recast", args.Name));
        }
    }
}