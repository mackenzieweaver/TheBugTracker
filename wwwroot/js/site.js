function mobilenav() {
    const main = document.getElementsByTagName('main')[0]
    main.classList.contains('d-none') ? main.classList.remove('d-none') : main.classList.add('d-none')
    
    const mobileNavLinks = document.getElementById('mobileNavLinks')
    mobileNavLinks.classList.contains('d-none') ? mobileNavLinks.classList.remove('d-none') : mobileNavLinks.classList.add('d-none')
}