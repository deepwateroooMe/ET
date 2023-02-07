namespace ET.Server {
    
    public class DBManagerComponent: Entity, IAwake, IDestroy {

        [StaticField]
        public static DBManagerComponent Instance;
// Manager: 就管理了这些小区        
        public DBComponent[] DBComponents = new DBComponent[IdGenerater.MaxZone]; // 这里说,分区分服时,每个小区都有一个 DBComponent
    }
}