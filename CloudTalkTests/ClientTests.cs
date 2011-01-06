using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.CloudTalk.Controller;
using Com.CloudTalk.Data;

namespace CloudTalkTests {
	[TestClass]
	public class ClientTests {
		private const string TestRoomName = "Quavio_TestRoom_101";
		private const int QuavioClientId = 5;
		private CloudTalkController Controller;

		[TestInitialize()]
		public void TestSetup() {
			Controller.ClearRooms();
			Controller.CreateChatRoom(QuavioClientId, TestRoomName);
		}

		[TestCleanup()]
		public void MyTestCleanup() {
			Controller.ClearRooms();
		}
		
		[TestMethod]
		public void TestMethod1() {
			Controller = CloudTalkController.Instance;
		}

		public void Test_Client_Queue() {
			var client = Controller.GetClientById(QuavioClientId);

			client.WaitingMembers.Add(new ChatWaiter {
				Name = "Fulano de Tal",
				RoomName = TestRoomName,
				Email = "fulano@gmail.com",
				Role = ClientRoles.Member
			});
		}
	}
}
