const counting = (adder,target) =>{
    let current = document.getElementById(target);
    let curentVal = parseInt(current.value)
    if(curentVal+adder >=1){
       current.value=curentVal+adder; 
    }
}
const countingTime = (adder, target) => {
    let current = document.getElementById(target);
    let val = parseInt(current.value) || 0;
    let max = (target === 'hour') ? 23 : 59;

    let nextVal = val + adder;

  
    if (nextVal > max) nextVal = 0;
    if (nextVal < 0) nextVal = max;


    current.value = nextVal.toString().padStart(2, '0');
}


let itemsArray = [];

function addToDOM() {
    const qtyInput = document.getElementById("count");
    const roleIn = document.getElementById("key-role");
    const container = document.getElementById("roles-added");
    if(roleIn.value == ""){
        roleIn.value="อะไรก็ได้ but in english"
    }
    const newItem = {
        id: Date.now(),
        lable:roleIn.value,
        quantity: parseInt(qtyInput.value),
        timestamp: new Date().toLocaleTimeString()
    };

    itemsArray.push(newItem);

    const itemDiv = document.createElement("div");
    itemDiv.className = "item-box";
    itemDiv.id = `item-${newItem.id}`;
    itemDiv.innerHTML = `
        <div style="border: 1px solid #ccc; padding: 10px; margin: 5px; border-radius: 5px;" class="role-box">
                <div>
                <strong>Role: </strong>
                <p>
                    ${newItem.lable}
                </p>
            </div>
            <div>
                <strong>Amount:</strong>
                <p>
                    ${newItem.quantity} players
                </p>
            </div>
            <button type="button" onclick="removeItem(${newItem.id})"class="del-role-btn">cancel</button>
        </div>
    `;

    container.appendChild(itemDiv);
    qtyInput.value = 1;
    roleIn.value ="";
    updateHiddenInput();
}

function removeItem(id) {
    itemsArray = itemsArray.filter(item => item.id !== id);
    const elementToRemove = document.getElementById(`item-${id}`);
    elementToRemove.remove();
    updateHiddenInput();
}

function updateHiddenInput() {
    document.getElementById("ItemsData").value = JSON.stringify(itemsArray);

}