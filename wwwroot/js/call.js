var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();
connection.start()

const servers = {
    iceServers: [
        {
            urls: ['stun:stun1.l.google.com:19302', 'stun:stun2.l.google.com:19302'],
        },
    ],
    iceCandidatePoolSize: 10,
};

const pc = new RTCPeerConnection(servers)

async function call(id, callerId) {
    const offerDescription = await pc.createOffer();
    await pc.setLocalDescription(offerDescription);
    connection.invoke("SendOfferToUser", id, JSON.stringify(offerDescription), callerId)
}

connection.on("ReceiveOffer", async (offer, id) => {
    await pc.setRemoteDescription(JSON.parse(offer))
    const answerDescription = await pc.createAnswer()
    await pc.setLocalDescription(answerDescription)
    connection.invoke("SendAnswerToCaller", id, JSON.stringify(answerDescription))
})

connection.on("ReceiveAnswer", async (answer) => {
    await pc.setRemoteDescription(JSON.parse(answer))
})