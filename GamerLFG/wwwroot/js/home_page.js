const toggleLobShow = () =>{
    var mylob = document.getElementById("mylob");
    var btn = document.getElementById("hidden");
    if (mylob.className == "lob_off"){
        mylob.className = "lobbies";
        btn.style="background-color:black;color:white;margin:1rem;";
        btn.innerHTML = '<i class="fa fa-toggle-off fa-2x" aria-hidden="true"></i>';

    }else{
        mylob.className = "lob_off";
        btn.style="background-color:orange;margin:1rem";
        btn.innerHTML = '<i class="fa fa-toggle-on fa-2x" aria-hidden="true"></i>';

    }
    
    
}