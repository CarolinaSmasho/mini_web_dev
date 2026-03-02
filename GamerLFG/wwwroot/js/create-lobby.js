const counting = (adder, target) => {
  let current = document.getElementById(target);
  let curentVal = parseInt(current.value);
  if (curentVal + adder >= 1) {
    current.value = curentVal + adder;
  }
};
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
                    <span>${item.lable}</span>
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
                       value="${item.lable}"
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
  if (roleIn.value == "") {
    roleIn.value = "อะไรก็ได้ but in english";
  }
  const newItem = {
    id: Date.now(),
    lable: roleIn.value,
    quantity: parseInt(qtyInput.value),
    locked: false,
    timestamp: new Date().toLocaleTimeString(),
  };

  itemsArray.push(newItem);

  const itemDiv = document.createElement("div");
  itemDiv.className = "item-box";
  itemDiv.id = `item-${newItem.id}`;
  itemDiv.innerHTML = buildRoleCard(newItem);

  container.appendChild(itemDiv);
  qtyInput.value = 1;
  roleIn.value = "";
  updateHiddenInput();
}

function renameItem(id, newName) {
  const item = itemsArray.find((i) => i.id === id);
  if (item && !item.locked) {
    item.lable = newName;
    updateHiddenInput();
  }
}

function removeItem(id) {
  const item = itemsArray.find((i) => i.id === id);
  if (item && item.locked) {
    alert("This role is occupied by a member and cannot be removed.");
    return;
  }
  itemsArray = itemsArray.filter((item) => item.id !== id);
  const elementToRemove = document.getElementById(`item-${id}`);
  elementToRemove.remove();
  updateHiddenInput();
}

function updateHiddenInput() {
  const container = document.getElementById("roles-hidden-container");
  if (!container) return;
  container.innerHTML = "";
  itemsArray.forEach(function (item) {
    const input = document.createElement("input");
    input.type = "hidden";
    input.name = "Roles";
    input.value = item.lable;
    container.appendChild(input);
  });
}
