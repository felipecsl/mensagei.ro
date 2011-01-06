function Dashboard(_options) {
	this.options = _options;
	
	var that = this;

	$(function () {
		var iId; // interval id

		$("#btnState").toggle(
			function () {
				$(this).val("Finalizar Atendimento").toggleClass("btnStart").toggleClass("btnStop");

				// set the client as online
				changeState(true);
				iId = setInterval(checkQueue, 5000);
			},
			function () {
				$(this).val("Iniciar Atendimento").toggleClass("btnStart").toggleClass("btnStop");

				// set the client as offline
				changeState(false);
				clearInterval(iId);
			});

		$("#table-queue tbody td").live("click", function () {
			var roomName = $(this).parent().attr("roomName");

			window.open(that.options.chatUrl + "/" + roomName, "chat_" + roomName, " toolbar=no, location=no, directories=no, status=no, menubar=no, scrollbars=no, resizable=no, width=500,height=500"); 
		});

		if (that.options.clientOnline.bool()) {
			// client is already online
			$("#btnState").click();
		}
	});

	function changeState(_online) {
		$.ajax({
			type: 'POST',
			url: that.options.onlineUrl + "/" + that.options.clientId,
			data: {
				online: _online
			},
			dataType: "json"
		});
	}

	function checkQueue() {
		$.ajax({
			type: 'POST',
			url: that.options.queueUrl + "/" + that.options.clientId,
			data: {
				id: that.options.clientId
			},
			success: function (data, textStatus, xhr) {
				$("#table-queue tbody tr").remove();

				$.each(data, function(i, val) {
					$("<tr roomName='" + val.roomName + "'><td>"+ val.name + "</td><td>"+ val.email + "</td></tr>").appendTo("#table-queue tbody");
				});
			},
			dataType: "json"
		});
	};
}