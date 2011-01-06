using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Data {
	public class ChatWaiter : ChatMember {
		/// <summary>
		/// Gets or sets the name of the room that this member is waiting to join.
		/// </summary>
		/// <value>The name of the room.</value>
		public string RoomName { get; set; }
	}
}
