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



document.addEventListener('DOMContentLoaded', () => {

const searchInput = document.getElementById("searchID");
const displayOutput = document.getElementById("resultsContainer");
displayOutput.className ="lob_off";
let timeoutId; // ตัวแปรสำหรับเก็บเวลาหน่วง

searchInput.addEventListener('input', (e) => {
    
    // alert("work");
    const query = e.target.value;
    let tagSearch = document.getElementById("tag-search");
    // เคลียร์เวลาเก่าทิ้งทุกครั้งที่พิมพ์ใหม่
    clearTimeout(timeoutId);

    if (query.length < 2) {
        displayOutput.innerHTML = "";
        displayOutput.className ="lob_off";
        tagSearch.className = "lob_off"
        return;
    }
    window.scrollTo({
        top: 0,
        behavior: 'smooth'
    });
    // หน่วงเวลา 300ms ก่อนเริ่มค้นหา
    timeoutId = setTimeout(async () => {
        try {
            const obj = { Title: query, HostName: "" };
            
            const response = await fetch('Home/SearchLobby', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(obj)
            });

            if (response.ok) {
                const result = await response.text();
                console.log(result.trim());
                // แสดงผลลัพธ์ (ถ้าเป็น HTML)
                if(result.trim().length === 0){
                    displayOutput.innerHTML = `<h3>No result</h3>`;
                }else{
                    displayOutput.innerHTML = result;
                }
                
                displayOutput.className = "lobbies";
            }
           
            tagSearch.className = displayOutput.className;
        } catch (error) {
            console.error("Search error:", error);
        }
    }, 300);
    
});

})

const togleSearch = () =>{
    let tagSearch = document.getElementById("tag-search");
    let allLobby = document.getElementById("all-lob");
    let searchResult = document.getElementById("resultsContainer");
    let searchInp = document.getElementById("searchID");
    let btn = document.getElementById("searchBtn");
    if(allLobby.className=="all-lob" && searchResult.childElementCount > 0){
        allLobby.className="lob_off";
        searchInp.value = "";
        btn.className="fa fa-times fa-3x";
        searchResult.className ="lobbies";
    }
    else{
        allLobby.className="all-lob";
        searchResult.innerHTML="";
        btn.className="fa fa-search fa-3x";
        searchInp.value = "";
        searchResult.className ="lob_off";
    }
    tagSearch.className = searchResult.className;
}

const searchForm = document.getElementById("search-form");

searchForm.addEventListener('submit', (e) => {
    e.preventDefault(); // สำคัญมาก! บรรทัดนี้จะหยุดการ Refresh หน้าเว็บ
    
    // เรียกใช้ฟังก์ชันที่คุณต้องการ (เหมือน onclick)
    togleSearch(); 
});
// const searchInput = document.getElementById("searchId");
// const displayOutput = document.getElementById("resultsContainer");
// searchInput.addEventListener('input',async (e) =>{
//     const query = e.target.value;
//     if(query.length < 2){
//         displayOutput.innerHTML="";
//         return;
//     }
//     try {
//         const obj = {
//         Title:query,HostName:""
//                     };
//         const response = await  fetch('Home/SearchLobby', {
//             method: 'POST',
//             headers: {
//                 'Content-Type': 'application/json'
//             },
//             body: JSON.stringify(obj)
//         })
//         result = await response.text();
//         if(result.trim()==""){
//             displayOutput.innerHTML=""
//         }
//         else{
//             displayOutput.innerHTML=result;
//         }
        
//     }
//     catch(error){
//         console.log(error);
//     }
// })


