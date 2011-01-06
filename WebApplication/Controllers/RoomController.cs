using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Com.CloudTalk.Controller;
using System.Diagnostics;
using System.Collections;
using System.Threading;
using System.Collections.Specialized;
using Com.CloudTalk.Controller.Events;
using Br.Com.Quavio.Tools.Web.Mvc;
using Com.CloudTalk.Data;
using Com.CloudTalk.Webapp.Controllers;
using Com.CloudTalk.Webapp.Models;

namespace WebApplication.Controllers {
	public class RoomController : QuavioController {
		private const int MaxEventWaitTime = 30000;     // request for events lasts at most 30 seconds
		private CloudTalkController Controller = CloudTalkController.Instance;
		
		public ActionResult RoomStatus(string id) {
			var room = Controller.GetChatRoom(id);

			if (room == null) {
				throw new ArgumentException();
			}

			return Jsonp(
				new { online = (room.Members.Count > 0) },
				Request.Params["callback"]);
		}

		public ActionResult Start(int id) {
			var clnt = Controller.GetClientById(id);
			var viewModel = new ChatViewModel();

			if (clnt == null) {
				viewModel.InvalidSession = true;
			}
			else {
				viewModel.AutoStart = false;
				viewModel.IsHost = false;
				viewModel.Online = clnt.IsOnline;
				viewModel.Logo = clnt.Logo;
			}

			return View(viewModel);
		}

		public ActionResult HostStart(int id, string roomName) {
			var clnt = Controller.GetClientById(id);
			var room = Controller.GetChatRoom(roomName);
			var viewModel = new ChatViewModel();

			if (clnt == null || room == null) {
				viewModel.InvalidSession = true;
			}
			else {
				viewModel.AutoStart = true;
				viewModel.Online = true;
				viewModel.IsHost = true;
				viewModel.RoomName = room.Name;
				viewModel.Name = clnt.Name;
				viewModel.Email = clnt.Email;
				viewModel.Logo = clnt.Logo;

				//add the client to the room
				room.AddMember(clnt.Email, clnt.Name);
				
				// add waiting members to the room
				foreach (var waiter in clnt.GetRoomWaiters(room.Name)) {
					room.AddMember(waiter.Email, waiter.Name);
					clnt.WaitingMembers.Remove(waiter);
					// Notify waiter that he is able to join now
					Controller.SendMessage(clnt.Email, "Entrou na sala", room.Name, waiter.Email, EventType.RoomSpaceAvailable);
				}
			}

			return View("Start", viewModel);
		}

		[HttpPost]
		public ActionResult Start(int id, FormCollection vars) {
			// assign a new, empty generated room to the user
			var newRoom = Controller.CreateChatRoom(id);

			Controller.AddMemberWaitingForClient(id, vars["Name"], vars["Email"], newRoom.Name);

			return Json(new { RoomName = newRoom.Name });
		}

		[HttpPost]
		public ActionResult Stop(FormCollection vars) {
			Controller.AbandonAll(vars["Name"], vars["Email"]);
			return Json(new { });
		}

		[HttpPost]
		public ActionResult LeaveRoom(int id, FormCollection vars) {
			string clientEmail = vars["Email"];
			string roomName = vars["RoomName"];
			bool forced = false;
			
			if (vars["Forced"] != null) {
				bool.TryParse(vars["Forced"].ToString(), out forced);
			}

			if (String.IsNullOrWhiteSpace(clientEmail) ||
				String.IsNullOrEmpty(roomName)) {
				return Json(new { Code = 0, Message = "Email ou nome da sala inválidos." });
			}

			if (Controller.GetMemberByEmail(roomName, clientEmail) == null) {
				return Json(new { Code = 0, Message = "Email não encontrado." });
			}

			var room = Controller.GetChatRoom(roomName);

			if (room == null) {
				return Json(new { Code = 0, Message = "Sala não encontrada." });
			}

			Controller.RemoveMember(roomName, clientEmail);

			return Json(new { Code = 1, Message = "Sucesso ao sair da sala." });
		}

		/// <summary>
		/// Joins the specified chat room using the provided name and email address
		/// </summary>
		/// <returns>
		/// Return codes:
		/// 0: Failure
		/// 1: Success
		/// 3: The room is full
		/// </returns>
		[HttpPost]
		public ActionResult JoinRoom(int id, FormCollection vars) {
			string clientName = vars["Name"];
			string clientEmail = vars["Email"];
			string roomName = vars["RoomName"];

			if (String.IsNullOrWhiteSpace(clientName) ||
				String.IsNullOrWhiteSpace(clientEmail)) {
				// invalid arguments
				return Json(new { Code = 0, Message = "Nome ou email inválido." });
			}

			ViewData["ClientName"] = clientName;
			ViewData["ClientEmail"] = clientEmail;

			var room = Controller.GetChatRoom(roomName);

			try {
				Controller.AddMember(
					roomName,
					clientEmail,
					clientName);
			}
			catch (FullChatRoomException e) {
				return Json(new { Code = 3, Message = e.Message, Position = e.QueueSize });
			}
			catch (DuplicateRoomMemberException e) {
				return Json(new { Code = 0, Message = e.Message });
			}

			// success
			return Json(new { Code = 1, Message = "Sucesso ao entrar na sala." });
		}

		[HttpPost]
		public ActionResult Events(string id, string email, FormCollection vars) {
			if (String.IsNullOrWhiteSpace(id)) {
				// invalid arguments
				return Json(new { Code = 0, Message = "Sala inválida." });
			}

			if (String.IsNullOrWhiteSpace(email)) {
				return Json(new[] { new { 
					Code = 0, 
					Message = "Email inválido." 
				}});
			}

			var c = Controller.GetMemberByEmail(id, email);

			if (c == null) {
				return Json(new[] { new { 
					Code = 2, 
					Message = "O cliente especificado não faz parte desta sala de chat." 
				}});
			}

			return Json(this.GetMessages(id, email));
		}

		private object[] GetMessages(string room, string client) {
			if (Controller.WaitForEvent(client, MaxEventWaitTime)) {
				// the thread has been signaled
				var messages = Controller.GetMessages(client, room);
				var lstJson = new List<object>();

				foreach (var m in messages) {
					lstJson.Add(new {
						EventObject = m.Text,
						Sender = m.SenderName,
						Timestamp = m.Timestamp.ToString("HH:mm:ss"),
						EventType = m.Type.ToString()
					});
				}

				return lstJson.ToArray();
			}

			// no signal received
			return new object[] { };
		}

		private object[] GetMembers(string _sRoomName) {
			var room = Controller.GetChatRoom(_sRoomName);
			var lstJson = new List<object>();

			foreach (var item in room.Members.Select(c => c.Name).ToList()) {
				lstJson.Add(new { Name = item });
			}

			return lstJson.ToArray();
		}

		[HttpPost, ValidateInput(false)]
		public ActionResult SendMessage(int id, FormCollection vars) {
			string message = vars["Message"];
			string clientEmail = vars["ClientEmail"];
			string roomName = vars["RoomName"];

			if (String.IsNullOrEmpty(message) ||
				String.IsNullOrEmpty(clientEmail) ||
				String.IsNullOrEmpty(roomName)) {
				throw new ArgumentNullException();
			}

			var c = Controller.GetMemberByEmail(roomName, clientEmail);

			if (c == null) {
				throw new InvalidOperationException();
			}

			new Thread(new ThreadStart(delegate {
				ChatRoom testRoom = Controller.GetChatRoom(roomName);
				Controller.SendMessage(clientEmail, message, roomName);
			})).Start();

			return Json(new { }, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public ActionResult ClearRoom(FormCollection vars) {
			string roomName = vars["RoomName"];

			if (String.IsNullOrEmpty(roomName)) {
				throw new ArgumentNullException();
			}

			Controller.ClearRoom(roomName);

			return Json(new { }, JsonRequestBehavior.AllowGet);
		}
	}
}