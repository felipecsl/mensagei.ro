using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.CloudTalk.Data;
using System.Threading;
using System.Collections.Specialized;
using System.Collections;
using Com.CloudTalk.Controller.Events;
using System.Web.Script.Serialization;
using Br.Com.Quavio.Tools.Web;

namespace Com.CloudTalk.Controller {
	public sealed class CloudTalkController {

		/// <summary>
		/// Singleton instance
		/// </summary>
		static readonly CloudTalkController instance = new CloudTalkController();
		
		/// <summary>
		/// Dictionary for keeping a value indicating whether we have new events ready for each client,
		/// with each element being a pair [client email, event available].
		/// </summary>
		private Dictionary<string, AutoResetEvent> dictEvents = new Dictionary<string, AutoResetEvent>();

		/// <summary>
		/// Dictionary for keeping sent and received messages
		/// </summary>
		private List<EventBase> messages = new List<EventBase>();

		private List<ChatRoom> chatRooms = new List<ChatRoom>();

		/// <summary>
		/// All the available clients
		/// </summary>
		/// 
		private List<Client> AllClients = new List<Client>();

		static CloudTalkController() {
		}

		public CloudTalkController() {
			using (var db = new CloudTalkEntities()) {
				AllClients = db.Client.ToList();
			}
		}

		public static CloudTalkController Instance {
			get {
				return instance;
			}
		}

		public IEnumerable<ChatRoom> GetClientRooms(int id) {
			return chatRooms.Where(cr => cr.OwnerId == id);
		}

		/// <summary>
		/// Gets the client by id.
		/// </summary>
		/// <param name="id">The id.</param>
		/// <returns></returns>
		public Client GetClientById(int id) {
			return AllClients.FirstOrDefault(c => c.Id == id);
		}

		public void AddMemberWaitingForClient(int _clientId, string _name, string _email, string _roomName) {
			var clnt = GetClientById(_clientId);

			if (clnt == null) {
				throw new ArgumentException("_clientId");
			}

			clnt.WaitingMembers.Enqueue(new ChatWaiter {
				Name = _name,
				Email = _email,
				Role = ClientRoles.Member,
				RoomName = _roomName
			});
		}

		/// <summary>
		/// Returns the client that matches the provided email address. 
		/// Searches also in the room queues.
		/// If no one is found, null is returned
		/// </summary>
		/// <param name="_sEmail">The email address</param>
		/// <returns>The instance of the member</returns>
		public ChatMember GetMemberByEmail(string _roomName, string _email) {
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentException("_roomName");
			}

			if (String.IsNullOrWhiteSpace(_email)) {
				throw new ArgumentException("_email");
			}

			var room = chatRooms.FirstOrDefault(cr => cr.Name == _roomName);

			if (room == null) {
				throw new ArgumentException("Chat room not found");
			}

			var member = room.GetMember(_email);

			if (member != null) {
				return member;
			}

			// if the client was not found in the room, try to find in the queue
			member = room.GetWaitingMember(_email);

			if (member != null) {
				return member;
			}

			// search in the clients waiters
			foreach (var client in AllClients) {
				member = client.WaitingMembers.FirstOrDefault(cw => cw.Email == _email);

				if (member != null) {
					return member;
				}
			}

			return null;
		}

		/// <summary>
		/// Opens an existing chat room with the specificed name.
		/// </summary>
		/// <param name="_roomName">The room name</param>
		/// <returns>The chat room object</returns>
		public ChatRoom GetChatRoom(string _roomName) {
			return chatRooms.FirstOrDefault(cr => cr.Name == _roomName);
		}

		/// <summary>
		/// Creates a new chat room for the specified client
		/// </summary>
		/// <returns></returns>
		public ChatRoom CreateChatRoom(int _clientId, string _name = null) {
			var room = new ChatRoom {
				MaxUsers = 2,
				Name = (_name == null) ? WebTools.GetRandomText(6) : _name,
				OwnerId = _clientId
			};
			
			chatRooms.Add(room);

			return room;
		}

		/// <summary>
		/// Deletes all clients and pending events from the system
		/// </summary>
		public void ClearRooms() {
			foreach (var room in chatRooms) {
				room.Clear();
				room.WaitingMembers.Clear();
			}
			dictEvents.Clear();
			messages.Clear();
		}

		public void ClearRoom(string _roomName) {
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentNullException("_roomName");
			}

			var room = GetChatRoom(_roomName);

			if (room == null) {
				throw new ArgumentException("_roomName");
			}
			
			room.Clear();
		}

		/// <summary>
		/// Releases all waiting threads on the provided client email
		/// </summary>
		/// <param name="_client">The client that will be checked</param>
		public void SignalMember(string _email) {
			if (!dictEvents.ContainsKey(_email)) {
				dictEvents[_email] = new AutoResetEvent(true);  // initialize the event as signalled
			}
			else {
				var autoEvent = dictEvents[_email];
				autoEvent.Set();                                // signal and allow the waiting threads to proceed
			}
		}

		public bool WaitForEvent(string _email, int _nTimeout) {
			if (String.IsNullOrWhiteSpace(_email)) {
				throw new ArgumentNullException("_email");
			}

			AutoResetEvent autoEvent;

			if (!dictEvents.ContainsKey(_email)) {
				autoEvent = new AutoResetEvent(false);  // initialize the event as non-signalled
				dictEvents[_email] = autoEvent;
			}
			else {
				autoEvent = dictEvents[_email];
			}

			return autoEvent.WaitOne(_nTimeout);        // block the current thread
		}

		public IEnumerable<EventBase> GetMessages(string _to, string _roomName) {
			if (String.IsNullOrWhiteSpace(_to)) {
				throw new ArgumentNullException("_to");
			}
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentNullException("_roomName");
			}

			var clientEvents = new List<EventBase>();
			var events = messages.Where(e => e.Recipients.Any(r => r == _to) && e.RoomName == _roomName);
			clientEvents.AddRange(events);

			foreach (var item in events) {
				item.Recipients.Remove(_to); // update event list
			}

			return clientEvents;
		}

		public int RoomQueueSize(string _roomName) {
			return GetChatRoom(_roomName).WaitingMembers.Count;
		}

		public void AddMember(string _roomName, string _clientEmail, string _clientName) {
			if (String.IsNullOrWhiteSpace(_clientEmail)) {
				throw new ArgumentNullException("_clientEmail");
			}
			if (String.IsNullOrWhiteSpace(_clientName)) {
				throw new ArgumentNullException("_clientName");
			}
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentNullException("_roomName");
			}

			var room = GetChatRoom(_roomName);

			if (room == null) {
				throw new ArgumentException("_roomName");
			}

			if(!room.IsFull) {
				room.AddMember(_clientEmail, _clientName);

				SendMessage(_clientEmail, new { Message = "Entrou na sala" }, _roomName);
			}
			else {
				var waiter = new ChatMember { 
					Name = _clientName, 
					Email = _clientEmail 
				};

				room.WaitingMembers.Enqueue(waiter);

				// Let the client know his position on the queue
				throw new FullChatRoomException(String.Empty, RoomQueueSize(room.Name));
			}
		}

		public void RemoveMember(string _roomName, string _clientEmail) {
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentNullException("_roomName");
			}
			if (String.IsNullOrWhiteSpace(_clientEmail)) {
				throw new ArgumentNullException("_clientEmail");
			}

			var room = GetChatRoom(_roomName);

			if (room == null) {
				throw new ArgumentException("Invalid room name");
			}

			ChatMember client = room.GetMember(_clientEmail);

			if (client != null) {
				room.RemoveMember(client.Email);

				SendMessage(
					client,
					new { Message = "Saiu da sala" },
					_roomName, null,
					EventType.LeaveRoom);

				if (room.WaitingMembers.Count > 0) {
					var nextInQueue = room.WaitingMembers.Dequeue();

					// add the waiting client to the room
					AddMember(_roomName, nextInQueue.Email, nextInQueue.Name);

					// The leaving client sends a message to the next client 
					// in line, notifying that there is a position available in 
					// the requested room.
					SendMessage(
						client,
						new { Room = _roomName },
						_roomName,
						nextInQueue.Email,
						EventType.RoomSpaceAvailable);

					SignalMember(nextInQueue.Email);

					// notify other waiting clients that the queue size has reduced
					foreach (var item in room.WaitingMembers.ToList()) {
						SendMessage(
							client,
							new { Room = _roomName, QueueSize = room.WaitingMembers.Count },
							_roomName, item.Email,
							EventType.QueueSizeChanged);

						SignalMember(item.Email);
					}
				}
			}
			else {
				client = room.GetWaitingMember(_clientEmail);

				if (client != null) {
					// found the client in the room queue 
					// Just remove him from the queue and we're all set!!
					room.RemoveWaitingMember(_clientEmail);

					// notify other waiting clients that the queue size has reduced
					foreach (var item in room.WaitingMembers.ToList()) {
						SendMessage(
							client,
							new { Room = _roomName, QueueSize = room.WaitingMembers.Count },
							_roomName, item.Email,
							EventType.QueueSizeChanged);

						SignalMember(item.Email);
					}
				}
			}
		}

		/// <summary>
		/// Sends a message to the specified client(s).
		/// </summary>
		/// <param name="_from">The message sender.</param>
		/// <param name="_msg">The message text.</param>
		/// <param name="_roomName">Name of the room.</param>
		/// <param name="_to">The message recipient(s).</param>
		/// <param name="_type">The type of the message.</param>
		public void SendMessage(string _from, string _msg, string _roomName, string _to = null, EventType _type = EventType.Message) {
			SendMessage(_from, new { Message = _msg }, _roomName, _to, _type);
		}

		private void SendMessage(string _from, object _msg, string _roomName, string _to = null, EventType _type = EventType.Message) {
			SendMessage(GetMemberByEmail(_roomName, _from), _msg, _roomName, _to, _type);
		}

		private void SendMessage(ChatMember _from, object _msg, string _roomName, string _to = null, EventType _type = EventType.Message) {

			if (_from == null) {
				throw new ArgumentNullException("_from");
			}
			if (String.IsNullOrWhiteSpace(_roomName)) {
				throw new ArgumentNullException("_roomName");
			}
			if (_msg == null) {
				throw new ArgumentNullException("_msg");
			}
			
			var room = GetChatRoom(_roomName);

			if (room == null) {
				throw new ArgumentException("_roomName");
			}

			if (_to != null && room.GetMember(_to) == null && room.GetWaitingMember(_to) == null) {
				throw new ArgumentException("_to");
			}

			var newEvent = EventFactory.NewEvent(_type);
			var jss = new JavaScriptSerializer();

			newEvent.RoomName = _from.Name;
			newEvent.SenderName = _from.Name;
			newEvent.SenderEmail = _from.Email;
			newEvent.Text = jss.Serialize(_msg);
			newEvent.Timestamp = DateTime.Now;
			newEvent.RoomName = room.Name;
			
			var roomClients = room.Members.Where(c => c.Email != _from.Email);
			
			if (_to != null) {
				// direct message
				newEvent.Recipients.Add(_to);
			}
			else {
				// broadcast message
				newEvent.Recipients.AddRange(roomClients.Select(c => c.Email));
			}

			messages.Add(newEvent);

			using (var db = new CloudTalkEntities()) {
				db.AddToHistory(new History {
					Room = _roomName,
					Client = db.Client.First(c => c.Id == room.OwnerId),
					Sender = _from.Email,
					Text = _msg.ToString(),
					Timestamp = DateTime.Now.ToString()
				});

				db.SaveChanges();
			}

			foreach (var item in roomClients) {
				SignalMember(item.Email);
			}
		}

		public void AbandonAll(string _userName, string _userEmail) {
			var memberRooms = chatRooms.Where(cr => cr.HasMember(_userName, _userEmail));
			var clients = AllClients.Where(cl => cl.HasMember(_userName, _userEmail));

			foreach (var r in memberRooms) {
				try {
					r.RemoveMember(_userEmail);
				}
				catch(InvalidOperationException) { }
				try {
					r.RemoveWaitingMember(_userEmail);
				}
				catch (InvalidOperationException) { }
			}

			foreach (var clnt in clients) {
				var waiter = clnt.WaitingMembers.FirstOrDefault(cw => cw.Email == _userEmail);

				if (waiter != null) {
					clnt.WaitingMembers.Remove(waiter);
				}
			}
		}
	}
}