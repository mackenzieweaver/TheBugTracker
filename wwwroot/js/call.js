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

// 2 The caller creates RTCPeerConnection and calls RTCPeerConnection.addTrack()
const pc = new RTCPeerConnection(servers)
const cameras = document.getElementById('cameras')
const selectedDeviceIdEl = document.getElementById('selectedDeviceIdEl')
const localVideo = document.getElementById('localVideo')
let localStream

async function getDeviceId(i = 1) {
    let devices = await navigator.mediaDevices.enumerateDevices()    
    devices = devices.filter(device => device.kind === 'videoinput' && !device.label.includes('OBS'))
    return devices[i].deviceId
}

async function call(id, callerId) {
    // 1 The caller captures local Media via MediaDevices.getUserMedia
    let deviceId = await getDeviceId()
    localStream = await navigator.mediaDevices.getUserMedia({ video: { deviceId: { exact: deviceId }}})
    localVideo.srcObject = localStream
    
    // 2 The caller creates RTCPeerConnection and calls RTCPeerConnection.addTrack()
    localStream.getTracks().forEach(track => pc.addTrack(track, localStream))

    // 3 The caller calls RTCPeerConnection.createOffer() to create an offer.
    const offerDescription = await pc.createOffer();

    // 4 The caller calls RTCPeerConnection.setLocalDescription() to set that offer as the local description (that is, the description of the local end of the connection).
    await pc.setLocalDescription(offerDescription);

    // 5 After setLocalDescription(), the caller asks STUN servers to generate the ice candidates
    pc.onicecandidate = e => {
        connection.invoke("SendIceCandidate", id, JSON.stringify(e.candidate))
    }

    // 6 The caller uses the signaling server to transmit the offer to the intended receiver of the call.
    connection.invoke("SendOfferToUser", id, JSON.stringify(offerDescription), callerId)
}

connection.on("ReceiveOffer", async (offer, id) => {    
    // 7 The recipient receives the offer and calls RTCPeerConnection.setRemoteDescription() to record it as the remote description (the description of the other end of the connection).
    await pc.setRemoteDescription(JSON.parse(offer))

    // 8 The recipient does any setup it needs to do for its end of the call: capture its local media
    let deviceId = await getDeviceId(0)
    localStream = await navigator.mediaDevices.getUserMedia({ video: { deviceId: { exact: deviceId }}})
    localVideo.srcObject = localStream
    // and attach each media track into the peer connection via RTCPeerConnection.addTrack()
    localStream.getTracks().forEach(track => pc.addTrack(track, localStream))

    // 9 The recipient then creates an answer by calling RTCPeerConnection.createAnswer().
    const answerDescription = await pc.createAnswer()
    
    // 10 The recipient calls RTCPeerConnection.setLocalDescription(), passing in the created answer, to set the answer as its local description. The recipient now knows the configuration of both ends of the connection.
    await pc.setLocalDescription(answerDescription)

    // After setLocalDescription(), the caller asks STUN servers to generate the ice candidates
    pc.onicecandidate = e => {
        connection.invoke("SendIceCandidate", id, JSON.stringify(e.candidate))
    }

    // 11 The recipient uses the signaling server to send the answer to the caller.
    connection.invoke("SendAnswerToCaller", id, JSON.stringify(answerDescription))
})

// 12 The caller receives the answer.
connection.on("ReceiveAnswer", async (answer) => {
    // 13 The caller calls RTCPeerConnection.setRemoteDescription() to set the answer as the remote description for its end of the call. 
    await pc.setRemoteDescription(JSON.parse(answer))

    // The caller now knows the configuration of both peers. Media begins to flow as configured.
})

connection.on("ReceiveIceCandidate", async (iceCandidate) => {
    pc.addIceCandidate(JSON.parse(iceCandidate))
})

navigator.mediaDevices.enumerateDevices().then(
    devices => {
        devices = devices.filter(device => device.kind === 'videoinput' && !device.label.includes('OBS'))
        devices.forEach(device => {
            let option = document.createElement('option')
            option.setAttribute('value', device.deviceId)
            option.innerText = device.label
            cameras.appendChild(option)
        })
    }
)

cameras.addEventListener('change', async (e) => {
    let constraints = {
        video: {
            deviceId: {
                exact: e.target.value
            }
        }
    }
    localStream = await navigator.mediaDevices.getUserMedia(constraints)
    localVideo.srcObject = localStream
})

const remoteVideo = document.getElementById('remoteVideo')
let remoteStream = new MediaStream()

pc.ontrack = (event) => {
    remoteStream = event.streams[0]
    remoteVideo.srcObject = remoteStream
}