const localVideo = document.getElementById('localVideo')
localVideo.style.width = "95%"
localVideo.style.height = "95%"

let constraints = { video: true, audio: true }
async function LoadMedia() {
    try {
        localVideo.srcObject = await navigator.mediaDevices.getUserMedia(constraints)
    } catch {
        localVideo.srcObject = null
    }
}

async function ToggleMic(el) {    
    el.children[0].remove()
    let img = document.createElement('img')
    img.style.height = '24px'

    if(constraints.audio === true) {
        constraints.audio = false
        img.src = '/img/microphone-slash-solid.svg'
    } else {
        constraints.audio = true
        img.src = '/img/microphone-solid.svg'
    }

    el.appendChild(img)
    await LoadMedia()
}

async function ToggleCam(el) {
    el.children[0].remove()
    let img = document.createElement('img')
    img.style.height = '24px'

    if(constraints.video === true) {
        constraints.video = false
        img.src = '/img/video-slash-solid.svg'
    } else {
        constraints.video = true
        img.src = '/img/video-solid.svg'
    }

    el.appendChild(img)
    await LoadMedia()
}

(async () => {
    await LoadMedia()
})()
  