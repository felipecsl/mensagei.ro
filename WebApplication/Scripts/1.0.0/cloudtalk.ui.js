var CloudTalkUI = function (_options) {
	this.options = _options;

	// private variable declarations
	var that = this,
		clientName, 		// The user's name as typed in the text field
		clientEmail, 		// the user's email as typed in the text field
		loginPanel, 		// input panel with name and email prompt
		pleaseWaitPanel, 	// panel showing that the user will be helped soon
		messagesPanel, 		// the whole message exchange panel
		messagesDiv, 		// sent and received messages div
		txtMessage, 		// input area for the message to be sent
		txtName, 			// input field for client name
		txtEmail, 			// input field for client email
		btnSubmit, 			// submit message button
		btnJoin, 			// join room button
		imgLoading, 		// 'loading' image
		lblQueue, 			// Label indicating the client position on the queue
		lblQueueSize, 		// Label with the client position number
		exitButton,         // Exit button
		isWaiting = false, // Are we currently in the queue, waiting to join the room?
		userDisconnected = false, // Are we currently in the queue, waiting to join the room?
		hasFocus = false, // Do we have the browser focus?
		msgIndex = false, // Title message index
		titleChanger,       // Timeout ID for changing the page title
		titleMessages = [	// Array with messages to be displayed in the title
			"Atendimento Online",
			"Atendimento Online "];

	var cloudTalk = new CloudTalk(this.options);

	$(function () {

		// assign elements
		loginPanel = $('#login-panel');
		messagesPanel = $('#messages-panel');
		messagesDiv = $('#msgs-panel');
		txtMessage = $('textarea#input-area');
		txtName = $('input#txt-name');
		pleaseWaitPanel = $("div.please-wait");
		txtEmail = $('input#txt-email');
		imgLoading = $('.loading-image');
		btnSubmit = $('input#submit-button');
		btnJoin = $('input#enter-button');
		lblQueue = $('span#queue-indicator');
		lblQueueSize = $('span#queue-size');
		exitButton = $('#btnSair');

		// hide panels
		imgLoading.hide();
		messagesPanel.hide();
		lblQueue.hide();

		if (!that.options.isOnline.bool() ||
			that.options.isInvalid.bool()) {

			$('div.not-available').show();
			loginPanel.hide();
		}

		txtMessage.keypress(onKeyPress);
		btnJoin.click(onJoinRoom);
		btnSubmit.click(onClickSubmit);
		exitButton.click(onClickExit);

		$(window).bind("focus", gainedFocus).bind("blur", lostFocus);

		// auto-join the specified room
		if (that.options.autoStart.bool()) {
			setUpChat();
			getEvents();
		}
	});

	function gainedFocus() {
		hasFocus = true;
		clearInterval(titleChanger);
		titleChanger = -1;
		document.title = titleMessages[0];
	}

	function lostFocus() {
		hasFocus = false;
	}

	function changeTitle(_msg) {
		if (hasFocus) return;
		if (titleChanger > 0) return;

		titleMessages[1] = _msg;

		titleChanger = setInterval(function () {
			msgIndex = !msgIndex;
			document.title = titleMessages[+msgIndex];
		}, 1500);
	}

	// text message key press event handler
	function onKeyPress(evt) {
		var keyPressEvent = (evt) ? evt : event;
		var charCode = (evt.which) ? evt.which : evt.keyCode;

		// enter key press submits the message (if shift is not pressed)
		if (charCode == 13 && !keyPressEvent.shiftKey) {
			keyPressEvent.preventDefault(); // suppress the line break the line in the input field
			btnSubmit.click(); // programatically submit the form on ENTER key press
		}

		hasFocus = true;
	}

	// send message event handler
	function onClickSubmit() {
		cloudTalk.sendMessage({
			message: txtMessage.val()
		});

		appendOwnMessage(
			that.options.userName,
			new Date().format("HH:MM:ss"),
			txtMessage.val());

		txtMessage.val('');   // clear the input field
	}

	// join room event handler
	function onJoinRoom(evt) {
		evt.preventDefault();

		if (txtName.val() == '' ||
			txtEmail.val() == '') {
			alert('Favor preencher todos os campos');
			return;
		}

		if (!Utils.isValidEmail(txtEmail.val())) {
			alert("Favor preencher um endereço de email válido");
			return;
		}

		imgLoading.show();

		clientName = txtName.val();
		clientEmail = txtEmail.val();

		cloudTalk.options.userName = clientName;
		cloudTalk.options.userEmail = clientEmail;

		cloudTalk.getRoom({
			success: onRoomNameReceived,
			error: onAjaxError
		});
	}

	function onAjaxError() {
		alert("Ocorreu um erro.\nPor favor, Tente novamente mais tarde.");
	}

	function onRoomNameReceived(data, textStatus) {
		imgLoading.hide();

		cloudTalk.options.roomName = data.RoomName;

		loginPanel.hide();
		pleaseWaitPanel.show();
		setUpQuitConfirmation();

		isWaiting = true;

		getEvents();    // start comet streaming
	}

	function onRoomJoined(data, textStatus) {
		var retCode = data.Code;
		var retMsg = data.Message;

		if (retCode == 3) {
			// the room is currently full :( Wait in the line.
			lblQueue.show();
			lblQueueSize.text(data.Position);
			loginPanel.hide();
			isWaiting = true;
			getEvents();
			setUpQuitConfirmation();
			return;
		}
		else if (retCode != 1) {
			// error occurred.
			alert(retMsg);
		}
		else {
			setUpChat();
		}
	}

	function setUpChat() {
		loginPanel.hide("normal");
		pleaseWaitPanel.hide();
		messagesPanel.show('normal');
		setUpQuitConfirmation();

		if (!that.options.isHost.bool()) {
			appendAdminMsg("Bem vindo, " + clientName + ". Em que posso ajudar?");
		}
	}

	function setUpQuitConfirmation() {
		// bind window unload events
		window.onbeforeunload = function () { return "Se você sair, suas mensagens não serão mantidas!" };
		$(window).bind('unload', disconnect);
	}

	function disconnect() {
		cloudTalk.cancelPendingRequests();

		if (!isWaiting) {
			cloudTalk.leaveRoom();
		}
		else {
			cloudTalk.abandon();
		}
		isWaiting = false;
		userDisconnected = true;
	}

	function onClickExit() {
		// update panels
		messagesPanel.hide();
		loginPanel.hide();
		$("div.thanks").show();
		
		// unbind close window event handlers
		$(window).unbind('unload');
		window.onbeforeunload = null;
		
		disconnect();
	}

	function onRoomJoinFailed(xhr, textStatus, errorThrown) {
		imgLoading.hide();

		alert(xhr.responseText);    // alert the failure
	}

	// makes an ajax request for new events for this client
	function getEvents() {
		cloudTalk.getEvents({
			success: onEventsComplete,
			error: function () { /*alert("Falha ao obter lista de eventos");*/ }
		});
	}

	function appendAdminMsg(_msg) {
		appendText('<div class="message">' + Utils.formatMessage(_msg) + "</div>");
	}

	function appendOwnMessage(_sender, _ts, _msg) {
		appendText('<div class="message"><span class="client">' + _sender + " (" + _ts + "): </span>" + Utils.formatMessage(_msg) + "</div>");
	}

	// Appends text to the messages panel and scrolls it to the bottom
	function appendMessage(_sender, _ts, _msg) {
		changeTitle(_sender + " diz...");
		appendText('<div class="message"><span>' + _sender + " (" + _ts + "): </span>" + Utils.formatMessage(_msg) + "</div>");
	}

	function appendText(_msg) {
		messagesDiv.append(_msg);
		messagesDiv[0].scrollTop = messagesDiv[0].scrollHeight;
	}

	// Called when new message(s) have been received
	function onEventsComplete(data) {
		// update messages list
		var arrMessages = data;

		if (arrMessages == null) return;

		for (var i = 0; i < arrMessages.length; i++) {

			var theMessage = arrMessages[i];

			if (theMessage.Code) {
				if (theMessage.Code == 0 ||
					theMessage.Code == 2) {
					onClickExit();
					return;
				}
			}

			var sEventType = theMessage.EventType;
			var msgObject;

			if (isWaiting && sEventType == "RoomSpaceAvailable") {
				// we've just received a message telling us that there is free space
				// for us to join the room! cool :)
				isWaiting = false;
				setUpChat();
			}
			else if (sEventType == "QueueSizeChanged") {
				msgObject = JSON.parse(theMessage.EventObject);

				lblQueueSize.text(msgObject.QueueSize);
			}
			else if (sEventType == "Message" ||
					sEventType == "EnterRoom" ||
					sEventType == "LeaveRoom") {

				msgObject = JSON.parse(theMessage.EventObject);

				appendMessage(
					theMessage.Sender,
					theMessage.Timestamp,
					msgObject.Message);

				$.sound.play(that.options.tunePath);

				// check if the host left the room and we're left alone. In this case, we'll be disconnected.
				if (sEventType == "LeaveRoom" &&
					!that.options.isHost.bool()) {
					// in this case, we'll be forcibly disconnected
					onClickExit();
					return;
				}
			}
		}

		if (!userDisconnected) {
			getEvents();    // restart the events request
		}
	}
}