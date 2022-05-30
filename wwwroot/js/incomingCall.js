(async () => {
    var connection = new signalR.HubConnectionBuilder().withUrl("/callHub").build();
    await connection.start()
    connection.on("IncomingCall", async (callerFirstName, url) => {
        console.log('incoming call')
        const toastContainer = document.getElementById('toast-container')
        const notification = document.createElement('a')
        notification.href = url
        notification.classList.add('bg-white', 'p-3', 'border', 'shadow-lg')
        notification.innerText = `Incoming call from ${callerFirstName}`
        toastContainer.appendChild(notification)
    })
})()
