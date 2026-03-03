const counting = (adder,target,min,max) =>{
    let current = document.getElementById(target);
    let curentVal = parseInt(current.value);
    if(curentVal+adder >= min && curentVal+adder <= max){
       current.value=curentVal+adder; 
    }
}

const countingTime = (adder, target) => {
  let current = document.getElementById(target);
  let val = parseInt(current.value) || 0;
  let max = target === "hour" ? 23 : 59;

  let nextVal = val + adder;

  if (nextVal > max) nextVal = 0;
  if (nextVal < 0) nextVal = max;

  current.value = nextVal.toString().padStart(2, "0");
};

let itemsArray = [];

/** Build the HTML card for a role item.
 *  - locked = true  → show role name as text + 🔒 badge (no rename, no remove)
 *  - locked = false → show editable input for rename + remove button
 */
function buildRoleCard(item) {
  if (item.locked) {
    return `
            <div style="border:1px solid #e74c3c; padding:10px; margin:5px; border-radius:5px; opacity:.7;" class="role-box">
                <div style="display:flex; align-items:center; gap:8px;">
                    <strong>Role:</strong>
                    <span>${item.label}</span>
                    <span style="background:#e74c3c22; color:#e74c3c; border:1px solid #e74c3c44;
                                 padding:2px 8px; border-radius:4px; font-size:11px; font-weight:700;">
                        🔒 Occupied
                    </span>
                </div>
                <small style="color:#888;">This role is held by a member and cannot be removed or renamed.</small>
            </div>`;
  }
  return `
        <div style="border:1px solid #ccc; padding:10px; margin:5px; border-radius:5px;" class="role-box">
            <div style="display:flex; align-items:center; gap:8px; margin-bottom:6px;">
                <strong>Role:</strong>
                <input type="text"
                       value="${item.label}"
                       style="background:#111; border:1px solid #444; color:#ddd; padding:4px 8px; border-radius:4px; font-size:13px;"
                       onchange="renameItem(${item.id}, this.value)"
                       placeholder="Role name">
            </div>
            <div style="margin-bottom:6px;"><strong>Amount:</strong> ${item.quantity} players</div>
            <button type="button" onclick="removeItem(${item.id})" class="del-role-btn">cancel</button>
        </div>`;
}

function addToDOM() {
    const qtyInput = document.getElementById("count");
    const roleIn = document.getElementById("key-role");
    const container = document.getElementById("roles-added");
    const MaxPlayer = document.getElementById("amountPlayer");
    let currentPlayer = 0;
    itemsArray.forEach(element => {
        currentPlayer += element.quantity;
    });
    if(currentPlayer+parseInt(qtyInput.value) > parseInt(MaxPlayer.value)){
        alert("You can't Add player in role Over Player amount");
        return
    }



    if(roleIn.value == ""){
        roleIn.value="Others"
    }
    const newItem = {
        id: Date.now(),
        label:roleIn.value,
        quantity: parseInt(qtyInput.value),
        timestamp: new Date().toLocaleTimeString()
    };

    let flag = true;
    itemsArray.forEach(element => {
        if (element.label == newItem.label){
            alert("There is already has THIS Role on You Lobby pls Cancel And Choose  Amount Again");
            flag = false;
            return
        } 
    });
    if(!flag){
        return
    }
    

    const itemDiv = document.createElement("div");
    itemDiv.className = "item-box";
    itemDiv.id = `item-${newItem.id}`;
    itemDiv.innerHTML = `
        <div style="border: 1px solid #ccc; padding: 10px; margin: 5px; border-radius: 5px;" class="role-box">
                <div>
                <strong>Role: </strong>
                <p>
                    ${newItem.label}
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
    itemsArray.push(newItem);
    addRoleToHost(newItem.label);
    updateHiddenInput();

}

function removeItem(id) {
    const itemData = itemsArray.find(item => item.id === id);
    itemsArray = itemsArray.filter(item => item.id !== id);
    const elementToRemove = document.getElementById(`item-${id}`);

    if (elementToRemove) {
        elementToRemove.remove();
    }
    if (itemData) {

        let roleId = itemData.label.replaceAll(" ", "-");         
        const inputToRemove = document.getElementById(roleId);
        const labelToRemove = document.querySelector(`label[for="${roleId}"]`); 
        if (inputToRemove) inputToRemove.remove();
        if (labelToRemove) labelToRemove.remove(); 
    }

    updateHiddenInput();
}

function updateHiddenInput() {
    document.getElementById("ItemsData").value = JSON.stringify(itemsArray);
    console.log(JSON.stringify(itemsArray));
    // const localInput = document.getElementById("myDateTime").value; // "2026-03-02T20:48"
    

}

const addRoleToHost = (newRole) => {
    let roleId = newRole.replaceAll(" ","-");
    const hostDiv = document.getElementById("host-role-display");
    const item = document.createElement("input");
    item.className = "check-box";
    item.name = "HostRole";
    item.type = "radio"
    item.value = newRole;
    item.id = roleId;
    item.setAttribute("style","width: fit-content");
    const inputLabel = document.createElement("label");
    inputLabel.setAttribute("for",roleId);
    inputLabel.innerHTML = newRole;
    inputLabel.className = "mood-label";
    hostDiv.appendChild(item);
    hostDiv.appendChild(inputLabel);
}

// function validateLobbyData() {
//     const maxPlayer = parseInt(document.getElementById("amountPlayer").value);
    
//     // ตรวจสอบว่ามี Role ใน itemsArray หรือยัง
//     if (itemsArray.length === 0) {
//         alert("ต้องเพิ่มอย่างน้อย 1 Role ก่อนสร้าง Lobby นะครับ!");
//         return false;
//     }

//     // ตรวจสอบจำนวนผู้เล่นรวม
//     let totalAssigned = itemsArray.reduce((sum, item) => sum + item.quantity, 0);
//     if (totalAssigned > maxPlayer) {
//         alert("จำนวนผู้เล่นใน Role รวมกันเกินจำนวน Max Player!");
//         return false;
//     }

//     return true;
// }
 function renameItem(id, newName) {
  const item = itemsArray.find((i) => i.id === id);
  if (item && !item.locked) {
    item.label = newName;
    updateHiddenInput();
  }
}

