$( document ).ready(function() {
    const queryString = window.location.search
    const urlParams = new URLSearchParams(queryString)
    const userId = urlParams.get('userid')
    if(userId === null) return
    const teammates = Array.from(document.querySelectorAll('[data-call-id]'))
    const teammate = teammates.filter(x => x.dataset.callId === userId)
    teammate[0].parentElement.parentElement.parentElement.remove()
})
