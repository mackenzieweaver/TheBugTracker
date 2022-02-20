var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()

connection.on("ReceivePrivateMessage", message => addmessage(message, 'left'));

let form = document.getElementById("form")
form.addEventListener("submit", e => {
    e.preventDefault();
    let message = document.getElementById("messageInput").value
    addmessage(message, 'right')
    connection.invoke("SendPrivateMessage", getToUserId(), message)
});

function addmessage(message, classname){
    var p = document.createElement("p")
    let messages = document.getElementById("messages")
    messages.appendChild(p)
    p.textContent = message
    p.classList.add(classname)
    messages.scrollTop = messages.scrollHeight
}

function getToUserId(){
    var inputs = document.getElementsByTagName("input")
    inputs = Array.from(inputs).filter(x => x.getAttribute('name') == 'ToUser')
    return inputs[0].id
}