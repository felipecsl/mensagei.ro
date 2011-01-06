using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Com.CloudTalk.Webapp.Models {
	public class ChatViewModel {
		public bool Online { get; set; }
		public bool InvalidSession { get; set; }
		public bool AutoStart { get; set; }
		public bool IsHost { get; set; }
		public string RoomName { get; set; }
		public string Name { get; set; }
		public string Email { get; set; }
		public string Logo { get; set; }
	}
}