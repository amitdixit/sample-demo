"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

document.getElementById("sendButton").disabled = true;
var connectionId = "";

connection.on("ReceiveMessage", (user, message) => {
    var li = document.createElement("li");
    document.getElementById("messagesList").appendChild(li);

    li.textContent = `${user} says ${message}`;
});

connection.start().then(() => {
    getConnectionId();
    document.getElementById("sendButton").disabled = false;
}).catch((err) => {
    return console.error(err.toString());
});


document.getElementById("sendButton").addEventListener("click", (event) => {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", user, message).catch((err) => {
        return console.error(err.toString());
    });
    event.preventDefault();
});


getConnectionId = () => {
    connection.invoke('GetConnectionId').then(
        (data) => {
            console.log(data);
            connectionId = data;
        }
    );
}



document.getElementById("sendButton1").addEventListener("click", (event) => {
    
    broadcastChartData();
    event.preventDefault();
});



broadcastChartData = () => {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value + "Specific";
    connection.invoke('BroadcastChartData', message, connectionId)
        .catch(err => console.error(err));
}