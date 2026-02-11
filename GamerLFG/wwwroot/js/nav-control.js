const toggleMenu = () =>{
    const menu = document.getElementById("mobile-menu")
    const icon = document.getElementById("hamburger-icon")
    if (menu.className === "nav-mobile-hidden") {
        menu.className = "nav-mobile-active";
        icon.className = "hamburger-icon-hidden"
    } else {
        menu.className = "nav-mobile-hidden";
        icon.className = "hamburger-icon"
    }

  
    document.addEventListener("click", (event) => {
    const menu = document.getElementById("mobile-menu");
    const icon = document.getElementById("hamburger-icon");

    if (menu.classList.contains("nav-mobile-active") && !menu.contains(event.target) && !icon.contains(event.target)) {
        toggleMenu();
    }
});
}