using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Com.CloudTalk.Data;

namespace Com.CloudTalk.Data {
	public class ChatRoom {
		
		public string Name { get; set; }
		public int OwnerId { get; set; }
		public List<ChatMember> Members { get; set; }
		public ChatQueue<ChatMember> WaitingMembers { get; set; }
		public int MaxUsers { get; set; }

		public bool IsFull {
			get {
				return (MaxUsers > 0 && Members.Count >= MaxUsers);
			}
		}

		public ChatRoom() {
			Members = new List<ChatMember>();
			WaitingMembers = new ChatQueue<ChatMember>();
			MaxUsers = 2;
		}

		public void AddMember(string _clientEmail, string _clientName) {
			if (Members.Any(c => c.Email == _clientEmail)) {
				// client is already a member of this room. Not allowed to re-join.
				throw new DuplicateRoomMemberException("A conta de e-mail escolhida já faz parte desta sala de chat.");
			}

			if (MaxUsers > 0 &&
				Members.Count >= MaxUsers) {
				throw new FullChatRoomException("A sala já está lotada. Você será colocado na fila de espera.");
			}

			Members.Add(new ChatMember {
				Name = _clientName,
				Email = _clientEmail
			});
		}

		/// <summary>
		/// Removes the client from this room (does not alter the queue).
		/// </summary>
		/// <param name="_email">The client email.</param>
		public void RemoveMember(string _email) {
			var member = Members.FirstOrDefault(c => c.Email == _email);
			
			if(member == null) {
				throw new InvalidOperationException("O cliente a ser removido nao foi encontrado");
			}

			Members.Remove(member);
		}

		/// <summary>
		/// Removes the client from the waiting queue
		/// </summary>
		/// <param name="_email">The client email to be removed.</param>
		public void RemoveWaitingMember(string _email) {
			var member = WaitingMembers.FirstOrDefault(cm => cm.Email == _email);

			if (member == null) {
				throw new InvalidOperationException("O cliente a ser removido nao foi encontrado");
			}

			WaitingMembers.Remove(member);
		}

		/// <summary>
		/// Gets the client (does not search in the queue).
		/// </summary>
		/// <param name="_email">The client email.</param>
		/// <returns></returns>
		public ChatMember GetMember(string _email) {
			return Members.FirstOrDefault(c => c.Email == _email);
		}

		/// <summary>
		/// Determines whether the room has the specified member (also searches in the waiting queue)
		/// </summary>
		/// <param name="_userName">The member name.</param>
		/// <param name="_email">The member email.</param>
		/// <returns>
		/// 	<c>true</c> if the specified user was found; otherwise, <c>false</c>.
		/// </returns>
		public bool HasMember(string _userName, string _email) {
			return Members.Any(cm => cm.Name == _userName && cm.Email == _email) ||
				WaitingMembers.Any(cm => cm.Name == _userName && cm.Email == _email);
		}

		public ChatMember GetWaitingMember(string _email) {
			return WaitingMembers.FirstOrDefault(clnt => clnt.Email == _email);
		}

		public void Clear() {
			Members.Clear();
		}
	}	
}