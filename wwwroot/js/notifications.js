var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()

connection.on("ReceiveNotification", (returnUrl, title, message, created, fromId, toId) => {
    if(window.location.pathname === returnUrl) return
    connection.invoke("AddNotificationToDb", {
        ReturnUrl: returnUrl,
        Title: title,
        Message: message,
        RecipientId: toId,
        SenderId: fromId
    })
    const notifications = document.getElementById('notifications')
    let a = createLink(returnUrl, title, message, created)
    notifications.insertBefore(a, notifications.children[1])
    updateCounter()
    showToastNotification(returnUrl, title, message)
})

function showToastNotification(returnUrl, title, message){
    let toast = document.createElement('div')
    toast.classList.add('customToast')
    toast.classList.add('bg-white')
    toast.classList.add('shadow-lg')
    toast.classList.add('border')
    toast.classList.add('rounded')
    toast.classList.add('p-3')
    
    let header = document.createElement('div')
    header.classList.add('customToastHeader')
    header.classList.add('d-flex')
    
    let strong = document.createElement('strong')
    strong.innerText = title
    header.appendChild(strong)    
    
    let button = document.createElement('button')
    button.classList.add('customBtnClose')
    button.classList.add('btn')
    button.classList.add('btn-outline-danger')
    button.onclick = () => toast.style.display = 'none'
    setTimeout(() => toast.style.display = 'none', 5000)

    let span = document.createElement('span')
    span.innerText = 'X'
    
    button.appendChild(span)
    header.appendChild(button)
    toast.append(header)    

    let a = document.createElement('a')
    a.classList.add('text-dark')
    a.classList.add('text-decoration-none')
    a.href = returnUrl
    a.innerText = message
    
    let toastBody = document.createElement('div')
    toastBody.classList.add('customToastBody')
    toastBody.appendChild(a)
    toast.appendChild(toastBody)
    
    let toastContainer = document.getElementById('toast-container')
    toastContainer.appendChild(toast)
}

function createLink(returnUrl, title, message, created) {
    let a = document.createElement('a')
    a.classList.add('text-dark')
    a.classList.add('text-decoration-none')
    a.href = returnUrl
    
    let div = document.createElement('div')
    div.classList.add('border')
    div.classList.add('m-3')
    div.classList.add('p-3')
    div.style.overflow = 'auto'
    
    let h5 = document.createElement('h5')
    h5.innerText = title
    
    let p1 = document.createElement('p')
    p1.classList.add('mb-0')
    p1.innerText = message
    
    let p2 = document.createElement('p')
    p2.classList.add('mb-0')
    p2.style.fontSize = '12px'
    p2.style.color = 'gray'
    p2.innerText = created
    
    div.appendChild(h5)
    div.appendChild(p1)
    div.appendChild(p2)
    a.appendChild(div)

    return a
}

function updateCounter(){
    const count = document.getElementById('notificationCount')
    count.innerText = parseInt(count.innerText) + 1
}