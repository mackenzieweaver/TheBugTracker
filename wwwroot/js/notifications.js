var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()

connection.on("ReceiveNotification", (id, returnUrl, title, message, created) => {
    if(window.location.pathname === returnUrl) {
        connection.invoke("RemoveNotificationFromDb", id)
        return
    }

    let n = newNotification(id, returnUrl, title, message, created)

    const unseenNotifications = document.getElementById('unseen-notifications')
    if(unseenNotifications.children[0].classList.contains('noNotificationsMessage'))
        unseenNotifications.children[0].remove()
    unseenNotifications.prepend(n)

    let clone = n.cloneNode(true)
    const allNotifications = document.getElementById('all-notifications')
    allNotifications.prepend(clone)
    
    updateCounter(1)
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

function newNotification(id, returnUrl, title, message, created) {
    let div = document.createElement('div')
    div.classList.add('border')
    div.classList.add('m-3')
    div.classList.add('p-3')
    div.style.overflow = 'auto'
    div.style.position = 'relative'
    
    let a = document.createElement('a')
    a.classList.add('text-dark')
    a.classList.add('text-decoration-none')
    a.href = returnUrl    
    
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

    let button = document.createElement('button')
    button.title = 'Mark as Read'
    button.classList.add('btn')
    button.classList.add('btn-outline-danger')
    button.classList.add('markNotificationAsRead')
    button.onclick = () => markAsRead(id, button)
    button.innerText = 'x'
    
    a.appendChild(h5)
    a.appendChild(p1)
    a.appendChild(p2)
    div.appendChild(a)
    div.appendChild(button)

    return div
}

function updateCounter(n){
    const count = document.getElementById('notificationCount')
    count.innerText = parseInt(count.innerText) + n
}

function markAsRead(id, el) {
    const seenNotifications = document.getElementById('seen-notifications')
    if(seenNotifications.children[0].classList.contains('noNotificationsMessage'))
        seenNotifications.children[0].remove()
    seenNotifications.prepend(el.parentElement)
    
    const unseenNotifications = document.getElementById('unseen-notifications')
    if(unseenNotifications.children.length === 0) {
        addNoNotificationsMessage(unseenNotifications, "No New Notifications!", "Click on another tab to see old notifications.")
    }

    connection.invoke("MarkNotificationAsRead", id);
    el.remove()    
    updateCounter(-1)
}

function addNoNotificationsMessage(el, h5text, ptext) {
    let div = document.createElement('div')
    div.classList.add('noNotificationsMessage')
    div.classList.add('m-3')
    div.classList.add('p-3')

    let h5 = document.createElement('h5')
    h5.innerText = h5text

    let p = document.createElement('p')
    p.innerText = ptext

    div.appendChild(h5)
    div.appendChild(p)
    el.appendChild(div)
}