using System.Linq;
using System.Collections.Generic;
using ET;
namespace ET.Server {
	[FriendOf(typeof(MatchRoomComponent))]
	public static class MatchRoomComponentSystem
	{
		// 添加匹配房间
		public static void Add(this MatchRoomComponent self, Room room)
		{
			self.rooms.Add(room.Id, room);
			self.idleRooms.Enqueue(room);
		}
		// 回收匹配房间
		public static void Recycle(this MatchRoomComponent self, long id)
		{
			Room room = self.readyRooms[id];
			self.readyRooms.Remove(room.Id);
			self.idleRooms.Enqueue(room);
		}
		// 获取匹配房间
		public static Room Get(this MatchRoomComponent self, long id)
		{
			Room room;
			self.rooms.TryGetValue(id, out room);
			return room;
		}
		// 获取等待中的匹配房间
		public static Room GetReadyRoom(this MatchRoomComponent self)
		{
			return self.readyRooms.Where(p => p.Value.Count < 3).FirstOrDefault().Value;
		}
		// 获取空闲的匹配房间
		public static Room GetIdleRoom(this MatchRoomComponent self)
		{
			if (self.IdleRoomCount > 0)
			{
				Room room = self.idleRooms.Dequeue();
				self.readyRooms.Add(room.Id, room);
				return room;
			}
			else
			{
				return null;
			}
		}
		// 匹配房间开始游戏
		public static void RoomStartGame(this MatchRoomComponent self, long id)
		{
			Room room = self.readyRooms[id];
			self.readyRooms.Remove(id);
			self.gameRooms.Add(room.Id, room);
		}
		// 匹配房间结束游戏
		public static void RoomEndGame(this MatchRoomComponent self, long id)
		{
			Room room = self.gameRooms[id];
			self.gameRooms.Remove(id);
			self.readyRooms.Add(room.Id, room);
		}
	}
}
