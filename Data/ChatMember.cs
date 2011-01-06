using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.CloudTalk.Data;
using System.Data.Objects;

namespace Com.CloudTalk.Data {
	public class ChatMember {
		public string Name { get; set; }
		public string Email { get; set; }
		public ClientRoles Role { get; set; }
	}
}