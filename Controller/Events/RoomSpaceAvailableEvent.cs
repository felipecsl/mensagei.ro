using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Controller.Events {
	public class RoomSpaceAvailableEvent : EventBase {
		public override string Type {
			get { return EventType.RoomSpaceAvailable.ToString(); }
		}
	}
}
