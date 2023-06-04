<<<<<<< HEAD
namespace ET.Client {

    [ComponentOf(typeof(Scene))]
    public class SessionComponent: Entity, IAwake, IDestroy {
        public Session Session { get; set; }
    }
=======
﻿namespace ET.Client
{
	[ComponentOf(typeof(Scene))]
	public class SessionComponent: Entity, IAwake, IDestroy
	{
		private EntityRef<Session> session;

		public Session Session
		{
			get
			{
				return session;
			}
			set
			{
				this.session = value;
			}
		}
	}
>>>>>>> 754634147ad9acf18faf318f2e566d59bc43f684
}
