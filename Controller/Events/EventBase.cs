using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.CloudTalk.Data;

namespace Com.CloudTalk.Controller.Events {
	public abstract class EventBase {
		public string Text { get; set; }
		public string SenderName { get; set; }
		public string SenderEmail { get; set; }
		public List<string> Recipients { get; set; }
		public string RoomName { get; set; }
		public DateTime Timestamp { get; set; }
		public abstract string Type { get; }

		public EventBase() {
			Recipients = new List<string>();
		}
	}

	public enum EventType {
		Message,
		LeaveRoom,
		EnterRoom,
		RoomSpaceAvailable,
		QueueSizeChanged
	}
}