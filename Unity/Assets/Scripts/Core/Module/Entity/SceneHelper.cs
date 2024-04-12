namespace ET {
    public static class SceneHelper {
        public static int DomainZone(this Entity entity) { // 把它理解成为了：场景的分区，不知道对不对 
            return (entity.Domain as Scene)?.Zone ?? 0;
        }
        public static Scene DomainScene(this Entity entity) {
            return entity.Domain as Scene;
        }
    }
}