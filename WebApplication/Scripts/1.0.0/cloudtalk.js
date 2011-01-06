var CloudTalk = function (_options) {

	this.options = _options;

	var that = this;
	var currRequest = null;

	/*
	* Asks for a room to join
	* Params: 
	*	success: callback function
	*	error: callback function
	*/
	this.getRoom = function (_options) {
		currRequest = $.ajax({
			url: that.options.baseUrl + "Start/" + that.options.clientId,
			type: "POST",
			data: {
				Name: that.options.userName,
				Email: that.options.userEmail
			},
			success: _options.success,
			error: _options.error,
			dataType: "json"
		});
	}

	/*
	* Joins the specified chat room
	* Params: 
	*	success: callback function
	*	error: callback function
	*/
	this.joinRoom = function (_options) {
		currRequest = $.ajax({
			url: that.options.baseUrl + "JoinRoom/" + that.options.clientId,
			type: "POST",
			data: {
				Name: that.options.userName,
				Email: that.options.userEmail,
				ClientId: that.options.clientId,
				RoomName: that.options.roomName
			},
			success: _options.success,
			error: _options.error,
			dataType: "json"
		});
	};


	/*
	* Leaves all current joined rooms and queues
	*/
	this.abandon = function () {
		currRequest = $.ajax({
			url: that.options.baseUrl + "Stop/" + that.options.clientId,
			type: "POST",
			data: {
				Name: that.options.userName,
				Email: that.options.userEmail
			}
		});
	};

	this.cancelPendingRequests = function() {
		if(currRequest) {
			currRequest.abort();
		}
	};

	/*
	* Leaves the specified chat room room
	*/
	this.leaveRoom = function () {
		currRequest = $.ajax({
			url: that.options.baseUrl + "LeaveRoom/" + that.options.clientId,
			type: "POST",
			data: {
				Email: that.options.userEmail,
				RoomName: that.options.roomName,
			}
		});
	};

	/*
	* Returns a list of available events for the current user
	* Params: 
	*	success: callback function
	*	error: callback function
	*/
	this.getEvents = function (_options) {
		currRequest = $.ajax({
			url: that.options.baseUrl + 'Events/' + that.options.roomName + '/' + that.options.userEmail + '?ts=' + new Date().getTime(),
			success: _options.success,
			error: _options.error,
			type: "POST",
			dataType: "json"
		});
	};

	/*
	* Sends a message.
	* Params: 
	*	message: text
	*	success: callback function
	*	error: callback function
	*/
	this.sendMessage = function (_options) {
		currRequest = $.ajax({
			url: that.options.baseUrl + "SendMessage/" + that.options.clientId,
			type: "POST",
			data: {
				Message: _options.message,
				ClientEmail: that.options.userEmail,
				RoomName: that.options.roomName
			},
			success: _options.success,
			error: _options.error
		});
	};
};