$(function() {
    getEvents();
});

function getEvents() {
    $.getJSON(sEventsURL, onJSONComplete);
}

function onSendSuccess() {
    var eMessages = $('#test-area');
    var eTxtInput = $('#input-area');
    eMessages.append(sClientName + ': ' + eTxtInput.val() + "<br />");
    setTimeout(function() { eTxtInput.val(''); }, 2000);    // wait some time before clearing the textbox
}

function onJSONComplete(data) {
    // update messages
    var arrMessages = data[0];
    var eMessages = $('#test-area');
    
    for (var i = 0; i < arrMessages.length; i++) {
        eMessages.append(arrMessages[i].Sender + ": " + arrMessages[i].Msg + "<br />");
    }

    // update members
    var arrMembers = data[1];
    var eMembers = $('#test-members');
    eMembers.text(''); // clear the members pane

    // re-add all members
    for (var i = 0; i < arrMembers.length; i++) {
        eMembers.append(arrMembers[i].Name + "<br />");
    }

    // restart the events request
    getEvents();
}