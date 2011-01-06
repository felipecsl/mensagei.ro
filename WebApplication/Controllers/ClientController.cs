using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using Com.CloudTalk.Webapp.Models;
using Com.CloudTalk.Data;
using Com.CloudTalk.Controller;

namespace Com.CloudTalk.Webapp.Controllers {
	public class ClientController : System.Web.Mvc.Controller {
		private CloudTalkController Controller = CloudTalkController.Instance;

		public ActionResult Dashboard(int? id) {
			if (!id.HasValue) {
				return View();
			}

			var clnt = Controller.GetClientById(id.Value);

			if (clnt == null) {
				return View();
			}

			var viewModel = new DashboardViewModel { TheClient = clnt };

			return View(viewModel);
		}

		[HttpPost]
		public ActionResult Enqueue(int id, FormCollection vars) {
			var clnt = Controller.GetClientById(id);

			if (clnt == null) {
				throw new ArgumentException("id");
			}

			string nome = vars["Name"];
			string email = vars["Email"];

			if (String.IsNullOrWhiteSpace(nome) ||
				string.IsNullOrWhiteSpace(email)) {
				throw new ArgumentException();
			}

			clnt.WaitingMembers.Enqueue(new ChatWaiter {
				Name = nome,
				Email = email,
				Role = ClientRoles.Member
			});
			return View();
		}

		public ActionResult Status(int id) {
			return View(Controller.GetClientRooms(id));
		}

		public ActionResult Queue(int id) {
			var clnt = Controller.GetClientById(id);

			if (clnt == null) {
				throw new ArgumentException("id");
			}

			return Json(
				clnt.WaitingMembers.Select(c => new {
					name = c.Name,
					email = c.Email,
					roomName = c.RoomName
				}).ToList(),
				JsonRequestBehavior.AllowGet);
		}

		public ActionResult Online(int id, FormCollection vars) {
			var clnt = Controller.GetClientById(id);

			if (clnt == null) {
				throw new ArgumentException("id");
			}

			clnt.IsOnline = bool.Parse(vars["online"]);

			// TODO: Remove all the client rooms

			return Json(new { });
		}		
	}
}