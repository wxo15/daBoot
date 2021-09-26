// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function openNav() {
    const vw = Math.max(document.documentElement.clientWidth || 0, window.innerWidth || 0);
    if (vw > 500) {
        document.getElementById("mySidebar").style.width = "250px";
        document.getElementById("header").style.marginLeft = "250px";
        document.getElementById("main").style.marginLeft = "250px";
        document.getElementById("footer").style.marginLeft = "250px";
        document.getElementById("footer").style.width = "calc(100% - 250px)";
    } else {
        document.getElementById("mySidebar").style.width = "100vw";
        document.getElementById("header").style.marginLeft = "100vw";
        document.getElementById("main").style.marginLeft = "100vw";
        document.getElementById("footer").style.marginLeft = "100vw";
        document.getElementById("footer").style.width = "0%";
    }
        
}

/* Set the width of the sidebar to 0 and the left margin of the page content to 0 */
function closeNav() {
    document.getElementById("mySidebar").style.width = "0";
    document.getElementById("header").style.marginLeft = "0";
    document.getElementById("main").style.marginLeft = "20px";
    document.getElementById("footer").style.marginLeft = "0";
    document.getElementById("footer").style.width = "100%";
}