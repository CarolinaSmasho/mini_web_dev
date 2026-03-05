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

const giveMeMore = async () => {
    const lastLobby = {
        Id: document.getElementById("last-lob").value
    };

    try {
       
        const response = await fetch('Home/GetNextLobby', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(lastLobby)
        });

        const result = await response.text();
        if(result.trim() == "" || result.trim == "\n"){
            throw new Error("There are no more pubic pages to display.");
        }
        const container = document.getElementById("notmine");
        container.insertAdjacentHTML('beforeend', result);
        const allItems = container.querySelectorAll('.lobby-item');
        const lastItemAdded = allItems[allItems.length - 1];
        if(lastItemAdded){
            const newLastId = lastItemAdded.getAttribute('data-id');
            document.getElementById("last-lob").value = newLastId;
        }
    } catch (error) {
        console.error('Error: ', error);
        alert("คือว่า " + error + " ครับ");
    }
}
