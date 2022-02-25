"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

document.getElementById("sendButton").disabled = true;
var myConnectionId = "";

connection.on("ReceiveMessage", (user, message, connectionId) => {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.innerHTML = connectionId === myConnectionId ? `<b><em>You</em></b> ${message}` : `<b>${user}</b> says ${message}`;
});

connection.on("UserLoggedIn", (name, connectionId) => {
    getConnectionId(name, connectionId);
    bindLoggedInUser();

});

connection.on("MyMessageReceived", (user, message, fromConnectionId) => {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.innerHTML = fromConnectionId === myConnectionId ? `<b><em>You</em></b> says ${message}` : `<b>${user}</b> says ${message}`;
});


connection.start().then(() => {

    bindLoggedInUser();

    document.getElementById("sendButton").disabled = false;
}).catch((err) => {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", (event) => {
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", message).catch((err) => {
        return console.error(err.toString());
    });
    event.preventDefault();
});



function getConnectionId(name, connectionId) {
    connection.invoke('GetConnectionId').then(
        (data) => {
            console.log(data);
            myConnectionId = data;

            console.log(`${name} -- ${connectionId}`);

            var li = document.createElement("li");
            document.getElementById("messagesList").appendChild(li);

            li.innerHTML = connectionId === myConnectionId ? `<b><em>You</em></b> have joined` : `<b>${name}</b> has Joined`;
        }
    );
}

function bindLoggedInUser() {
    connection.invoke('GetAllUsers').then(
        (data) => {
            var ddlUserList = document.getElementById("ddlUserList");
            var option = document.createElement("option");
            ddlUserList.innerHTML = "";

            option.text = "All";
            option.value = "-1";

            for (var i = 0; i < data.length; i++) {
                if (data[i].connectionId !== myConnectionId) {
                    option = document.createElement("option");
                    option.text = data[i].userName;
                    option.value = data[i].connectionId;
                    ddlUserList.add(option);
                }

            }
        }
    );
}



document.getElementById("sendButton1").addEventListener("click", (event) => {
    SendMessageToSelectedUser();
    event.preventDefault();
});



function SendMessageToSelectedUser() {
    var toUser = document.getElementById("ddlUserList");
    var message = document.getElementById("messageInput").value + ` Specific to ${toUser.options[toUser.selectedIndex].text}`;
    connection.invoke('SendMessageToSelectedUser', toUser.value, message)
        .catch(err => console.error(err));
}