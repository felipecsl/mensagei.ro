using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Controller.Events {
	public class QueueSizeChangedEvent : EventBase {
		public override string Type {
			get { return EventType.QueueSizeChanged.ToString(); }
		}
	}
}
