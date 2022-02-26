var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()

connection.on("ReceiveNotification", (returnUrl, title, message, created) => {
    const notifications = document.getElementById('notifications')

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

    notifications.insertBefore(a, notifications.children[1])

    const count = document.getElementById('notificationCount')
    count.innerText = parseInt(count.innerText) + 1
})