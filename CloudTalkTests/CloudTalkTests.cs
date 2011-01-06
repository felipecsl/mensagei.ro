using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Com.CloudTalk.Controller;
using Com.CloudTalk.Controller.Events;
using Com.CloudTalk.Data;

namespace CloudTalkTests {
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class CloudTalkTests {
		public CloudTalkTests() {
			Controller = CloudTalkController.Instance;
		}

		private TestContext testContextInstance;
		private const string TestRoomName = "Quavio_TestRoom_101";
		private const int QuavioClientId = 5;
		private CloudTalkController Controller;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext {
			get {
				return testContextInstance;
			}
			set {
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		//
		// You can use the following additional attributes as you write your tests:
		//
		// Use ClassInitialize to run code before running the first test in the class
		// [ClassInitialize()]
		// public static void MyClassInitialize(TestContext testContext) { }
		//
		// Use ClassCleanup to run code after all tests in a class have run
		// [ClassCleanup()]
		// public static void MyClassCleanup() { }
		//
		// Use TestInitialize to run code before running each test 
		// [TestInitialize()]
		// public void MyTestInitialize() { }
		//
		// Use TestCleanup to run code after each test has run
		// [TestCleanup()]
		// public void MyTestCleanup() { }
		//
		#endregion

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
		public void TestOneToOneRoom() {
			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");

			var room = Controller.GetChatRoom(TestRoomName);

			Assert.AreNotEqual(room.Members, 0);

			Assert.AreEqual<string>(
				room.Members.FirstOrDefault().Email,
				"felipe.lima@gmail.com");
		}

		[TestMethod]
		public void TestSingleMessage() {
			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");

			var room = Controller.GetChatRoom(TestRoomName);

			Controller.SendMessage("felipe.lima@gmail.com", "BlaBlaBla", TestRoomName);

			var msgsReceived = Controller.GetMessages("tomas@quavio.com.br", TestRoomName);

			Assert.AreNotEqual(msgsReceived.Count(), 0);

			EventBase m = msgsReceived.FirstOrDefault();

			Assert.AreEqual("{\"Message\":\"BlaBlaBla\"}", m.Text);
			Assert.AreEqual("Felipe Lima", m.SenderName);
			Assert.AreEqual(EventType.Message.ToString(), m.Type);

			msgsReceived = Controller.GetMessages("felipe.lima@gmail.com",  TestRoomName);

			Assert.AreEqual(1, msgsReceived.Count());

			m = msgsReceived.FirstOrDefault();

			Assert.AreEqual("{\"Message\":\"Entrou na sala\"}", m.Text);
			Assert.AreEqual("Tomás de Lara", m.SenderName);
		}

		
		[TestMethod]
		public void TestRemoveUser() {
			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");
			Controller.RemoveMember(TestRoomName, "felipe.lima@gmail.com");

			var room = Controller.GetChatRoom(TestRoomName);

			Assert.AreEqual(room.Members.Count, 0);
		}

		[TestMethod]
		[ExpectedException(typeof(FullChatRoomException))]
		public void TestRoomEnqueue() {
			var room = Controller.GetChatRoom(TestRoomName);

			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");
			Controller.AddMember(TestRoomName, "ico@quavio.com.br", "Ico Portela");

			Assert.AreEqual(1, Controller.RoomQueueSize(TestRoomName));
		}

		
		[TestMethod]
		public void TestRoomDequeue() {

			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");

			var room = Controller.GetChatRoom(TestRoomName);

			try {
				Controller.AddMember(TestRoomName, "ico@quavio.com.br", "Ico Portela");
			}
			catch (FullChatRoomException) { }

			Controller.RemoveMember(TestRoomName, "felipe.lima@gmail.com");
			var c3 = Controller.GetMemberByEmail(TestRoomName, "ico@quavio.com.br");
			var events = Controller.GetMessages("ico@quavio.com.br", TestRoomName);

			Assert.AreNotEqual(events.Count(), 0);

			EventBase firstEvent = events.FirstOrDefault();

			Assert.AreEqual(EventType.RoomSpaceAvailable.ToString(), firstEvent.Type);
			Assert.AreEqual(0, Controller.RoomQueueSize(TestRoomName));
		}

		/// <summary>
		/// It should add waiting clients to the room whenever there is space available,
		/// usually when one of the clients leaves.
		/// </summary>
		[TestMethod]
		public void TestRoomQueueAdvanced() {
			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");

			var room = Controller.GetChatRoom(TestRoomName);

			try {
				Controller.AddMember(TestRoomName, "ico@quavio.com.br", "Ico Portela");
			}
			catch (FullChatRoomException) { }
			try {
				Controller.AddMember(TestRoomName, "nicolas@quavio.com.br", "Nicolas Iensen");
			}
			catch (FullChatRoomException) { }

			Assert.AreEqual(2, Controller.RoomQueueSize(TestRoomName));

			Controller.RemoveMember(TestRoomName, "tomas@quavio.com.br");
			var events = Controller.GetMessages("ico@quavio.com.br", TestRoomName);

			Assert.AreEqual(1, Controller.RoomQueueSize(TestRoomName));

			Controller.SendMessage("ico@quavio.com.br", "BlaBlaBla", TestRoomName);

			var msgsReceived = Controller.GetMessages("felipe.lima@gmail.com", TestRoomName);
			Assert.AreNotEqual(0, msgsReceived.Count());
			EventBase m = msgsReceived.FirstOrDefault();
			Assert.IsNotNull(m);
		}

		/// <summary>
		/// It should add waiting clients to the room whenever there is space available,
		/// usually when one of the clients leaves.
		/// </summary>
		[TestMethod]
		public void TestRoomQueueAdvanced2() {
			ChatRoom room = Controller.GetChatRoom(TestRoomName);

			Controller.AddMember(TestRoomName, "felipe@quavio.com.br", "Felipe Lima");    // felipe entra
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");     // tomas entra

			try {
				Controller.AddMember(TestRoomName, "ico@quavio.com.br", "Ico Portela");    // ico na fila
			}
			catch (FullChatRoomException) { }

			var tomas = Controller.GetMemberByEmail(TestRoomName, "tomas@quavio.com.br");
			var ico = Controller.GetMemberByEmail(TestRoomName, "ico@quavio.com.br");
			var felipe = Controller.GetMemberByEmail(TestRoomName, "felipe@quavio.com.br");
			
			Assert.AreEqual(Controller.RoomQueueSize(TestRoomName), 1);

			Controller.RemoveMember(TestRoomName, tomas.Email);                      // tomas sai
			var events = Controller.GetMessages(ico.Email, TestRoomName);

			Assert.AreEqual(Controller.RoomQueueSize(TestRoomName), 0);

			Controller.SendMessage(ico.Email, "BlaBlaBla", TestRoomName);

			var msgsReceived = Controller.GetMessages(felipe.Email, TestRoomName);
			Assert.AreNotEqual(msgsReceived.Count(), 0);
			EventBase m = msgsReceived.FirstOrDefault();
			Assert.IsNotNull(m);

			try {
				Controller.AddMember(TestRoomName, "nicolas@quavio.com.br", "Nicolas Iensen");
			} // nicolas na fila
			catch (FullChatRoomException) { }

			var nicolas = Controller.GetMemberByEmail(TestRoomName, "nicolas@quavio.com.br");

			Assert.IsNotNull(nicolas);
			Assert.AreEqual(Controller.RoomQueueSize(TestRoomName), 1);

			try { 
				Controller.AddMember(TestRoomName, "cleber@quavio.com.br", "Cleber Hensel"); 
			} // cleber na fila
			catch (FullChatRoomException) { }

			var cleber = Controller.GetMemberByEmail(TestRoomName, "cleber@quavio.com.br");

			Assert.IsNotNull(cleber);
			Assert.AreEqual(2, Controller.RoomQueueSize(TestRoomName));

			try {
				Controller.AddMember(TestRoomName, "aloisio@quavio.com.br", "Aloisio Dias");
			} // aloisio na fila
			catch (FullChatRoomException) { }

			var aloisio = Controller.GetMemberByEmail(TestRoomName, "aloisio@quavio.com.br");

			Assert.IsNotNull(aloisio);
			Assert.AreEqual(3, Controller.RoomQueueSize(TestRoomName));

			Controller.RemoveMember(TestRoomName, felipe.Email);							 // felipe sai
			nicolas = Controller.GetMemberByEmail(TestRoomName, "nicolas@quavio.com.br");    // nicolas entra no lugar
			Controller.SendMessage(nicolas.Email, "LaLaRi", TestRoomName);
			Assert.AreEqual(2, Controller.RoomQueueSize(TestRoomName));

			cleber = Controller.GetMemberByEmail(TestRoomName, "cleber@quavio.com.br");
			msgsReceived = Controller.GetMessages(cleber.Email, TestRoomName);
			Assert.AreNotEqual(0, msgsReceived.Count());
			m = msgsReceived.FirstOrDefault();
			Assert.IsNotNull(m);
			Assert.AreEqual(m.Type, EventType.QueueSizeChanged.ToString());

			aloisio = Controller.GetMemberByEmail(TestRoomName, "aloisio@quavio.com.br");
			msgsReceived = Controller.GetMessages(aloisio.Email, TestRoomName);
			Assert.AreNotEqual(0, msgsReceived.Count());
			m = msgsReceived.FirstOrDefault();
			Assert.IsNotNull(m);
			Assert.AreEqual(m.Type, EventType.QueueSizeChanged.ToString());
		}

		/// <summary>
		/// It should notify waiting users whenever their position in the queue has changed
		/// </summary>
		[TestMethod]
		public void TestAbandonQueue() {
			ChatRoom room = Controller.GetChatRoom(TestRoomName);

			Controller.AddMember(TestRoomName, "felipe.lima@gmail.com", "Felipe Lima");    // felipe entra
			Controller.AddMember(TestRoomName, "tomas@quavio.com.br", "Tomás de Lara");     // tomas entra

			try {
				Controller.AddMember(TestRoomName, "ico@quavio.com.br", "Ico Portela"); 
			}  // ico na fila
			catch (FullChatRoomException) { }

			try {
				Controller.AddMember(TestRoomName, "nicolas@quavio.com.br", "Nicolas Iensen"); 
			} // nicolas na fila
			catch (FullChatRoomException) { }

			var ico = Controller.GetMemberByEmail(TestRoomName, "ico@quavio.com.br");
			Controller.RemoveMember(TestRoomName, ico.Email);

			Assert.AreEqual(1, Controller.RoomQueueSize(TestRoomName));

			var nicolas = Controller.GetMemberByEmail(TestRoomName, "nicolas@quavio.com.br");
			var msgsReceived = Controller.GetMessages(nicolas.Email, TestRoomName);

			Assert.IsNotNull(msgsReceived);
			Assert.AreNotEqual(0, msgsReceived.Count());
			
			var m = msgsReceived.FirstOrDefault();
			
			Assert.IsNotNull(m);
			Assert.AreEqual(m.Type, EventType.QueueSizeChanged.ToString());
		}
	}
}