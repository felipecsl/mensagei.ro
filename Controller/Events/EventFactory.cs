using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.CloudTalk.Controller.Events {
	internal class EventFactory {
		public static EventBase NewEvent(EventType _type) {
			switch(_type) {
				case EventType.Message:
					return new MessageEvent();
				case EventType.EnterRoom:
					return new EnterRoomEvent();
				case EventType.LeaveRoom:
					return new LeaveRoomEvent();
				case EventType.QueueSizeChanged:
					return new QueueSizeChangedEvent();
				case EventType.RoomSpaceAvailable:
					return new RoomSpaceAvailableEvent();
				default:
					throw new NotImplementedException();
			}
		}
	}
}
