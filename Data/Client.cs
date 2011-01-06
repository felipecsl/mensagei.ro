using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Data {
	public partial class Client {
		public ChatQueue<ChatWaiter> WaitingMembers { get; set; }
		public bool IsOnline { get; set; }

		public Client() {
			WaitingMembers = new ChatQueue<ChatWaiter>();
		}

		public bool HasMember(string _userName, string _userEmail) {
			return WaitingMembers.Any(cm => cm.Name == _userName && cm.Email == _userEmail);
		}

		public List<ChatWaiter> GetRoomWaiters(string _roomName) {
			return WaitingMembers.Where(cw => cw.RoomName == _roomName).ToList();
		}
	}
}
