using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Data {
	public class DuplicateRoomMemberException : Exception {
		public DuplicateRoomMemberException()
			: base() {
		}

		public DuplicateRoomMemberException(string _sMessage)
			: base(_sMessage) {
		}
	}
}