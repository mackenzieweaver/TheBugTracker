$( document ).ready(function() {
    const queryString = window.location.search
    const urlParams = new URLSearchParams(queryString)

    const callerid = urlParams.get('callerid')
    const calleeid = urlParams.get('calleeid')
    const teammates = Array.from(document.querySelectorAll('a[data-callee-id]'))

    teammates.forEach(teammate => {
        const id = teammate.dataset.calleeId
        if (id === callerid)
            removeTeammate(teammate)
        else if(id=== calleeid)
            removeTeammate(teammate)
    })
})

function removeTeammate(teammate) {
    // teammate is the <a> element, the 3rd parent is the <li> in the teammates list
    teammate.parentElement.parentElement.parentElement.remove()
}
