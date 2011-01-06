using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Data {
	public class FullChatRoomException : Exception {
		public int QueueSize { get; set; }

		public FullChatRoomException(
			string _sMessage)
			: this(_sMessage, -1) {
		}

		public FullChatRoomException(
			string _sMessage,
			int _nQueueSize)
			: base(_sMessage) {
			this.QueueSize = _nQueueSize;
		}

		public FullChatRoomException()
			: base() {
		}
	}
}