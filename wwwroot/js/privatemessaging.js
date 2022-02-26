var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()
scrollBottom()
connection.on("ReceivePrivateMessage", message => addmessage(message, 'left'));

let form = document.getElementById("form")
form.addEventListener("submit", e => {
    e.preventDefault();
    let message = document.getElementById("messageInput").value
    addmessage(message, 'right')
    clearAndFocusInput()
    connection.invoke("SendPrivateMessage", getToUserId(), message)
});

function clearAndFocusInput() {
    const x = document.getElementById('messageInput')
    x.value = ''
    x.focus()
}

function getToUserId(){
    var inputs = document.getElementsByTagName("input")
    inputs = Array.from(inputs).filter(x => x.getAttribute('name') == 'ToUser')
    return inputs[0].id
}

function addmessage(message, classname){
    let messages = document.getElementById("messages")
    
    var divrow = document.createElement("div")
    divrow.classList.add('row')
    divrow.classList.add('m-0')
    if(classname == 'right')
        divrow.classList.add('justify-content-end')
    messages.appendChild(divrow)

    var divcolmsg = document.createElement("div")
    divcolmsg.textContent = message
    divcolmsg.classList.add(classname)
    divcolmsg.classList.add('col-auto')
    divrow.appendChild(divcolmsg)

    var divcoltime = document.createElement("div")
    divcoltime.classList.add('col-12')
    divcoltime.classList.add('date')

    const date = new Date()
    const [month, day, year] = [date.getMonth(), date.getDate(), date.getFullYear()]
    const [hour, minutes] = [date.getHours(), date.getMinutes()]
    divcoltime.innerText = `${month + 1}/${day}/${year} ${hour > 12 ? hour - 12 : hour}:${minutes < 10 ? '0' + minutes : minutes} ${hour > 12 ? 'PM' : 'AM'}`
    divrow.appendChild(divcoltime)
    
    messages.scrollTop = messages.scrollHeight
}

function scrollBottom(){    
    let messages = document.getElementById("messages")
    messages.scrollTop = messages.scrollHeight
}
