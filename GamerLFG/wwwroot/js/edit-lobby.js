// ── Edit Lobby JS ─────────────────────────────────────────────────────
// Expects these globals from the Razor view:
//   occupiedRoleCounts  – { "RoleName": numberOfActiveMembers, ... }
//   currentMemberCount  – total active (non-Pending) members
//   initialRoles        – [{ name, quantity }, ...] from server

let editRolesArray = [];

// ── MaxPlayers constraint ────────────────────────────────────────────
function setupMaxPlayersConstraint() {
    const minInput = document.getElementById("min");
    if (!minInput) return;
    minInput.setAttribute("min", currentMemberCount);
    if (parseInt(minInput.value) < currentMemberCount) {
        minInput.value = currentMemberCount;
    }
}

function countingTimeEdit(adder, target) {
    const el = document.getElementById(target);
    let val = parseInt(el.value) || 0;
    let next = val + adder;
    const min = target === "min" ? currentMemberCount : 0;
    if (next < min) next = min;
    if (next > 100) next = 100;
    el.value = next;
}

// ── Role card builder ────────────────────────────────────────────────
function buildEditRoleCard(item) {
    const occupied = occupiedRoleCounts[item.label] || 0;
    const isOccupied = occupied > 0;

    let nameHtml;
    if (isOccupied) {
        nameHtml = `<span class="erc-name">${item.label}</span>
                    <span class="erc-lock">${occupied} in use</span>`;
    } else {
        nameHtml = `<input type="text" class="erc-name-input" value="${item.label}"
                       onchange="renameEditRole(${item.id}, this.value)"
                       placeholder="Role name">`;
    }

    return `
        <div class="edit-role-card ${isOccupied ? 'locked' : ''}">
            <div class="erc-header">
                ${nameHtml}
            </div>
            <div class="erc-qty">
                <button type="button" class="erc-qty-btn" onclick="changeEditRoleQty(${item.id}, -1)">-</button>
                <span id="qty-${item.id}">${item.quantity}</span>
                <button type="button" class="erc-qty-btn" onclick="changeEditRoleQty(${item.id}, 1)">+</button>
            </div>
            ${!isOccupied
                ? `<button type="button" class="erc-remove" onclick="removeEditRole(${item.id})">Remove</button>`
                : ''
            }
        </div>`;
}

// ── Add role ─────────────────────────────────────────────────────────
function addEditRole() {
    const roleIn = document.getElementById("key-role");
    const qtyInput = document.getElementById("count");
    const container = document.getElementById("roles-added");

    let roleName = roleIn.value.trim();
    if (!roleName) roleName = "All Class";

    if (editRolesArray.some(r => r.label === roleName)) {
        alert("This role already exists.");
        return;
    }

    const qty = Math.max(1, parseInt(qtyInput.value) || 1);
    const newItem = { id: Date.now(), label: roleName, quantity: qty };
    editRolesArray.push(newItem);

    const itemDiv = document.createElement("div");
    itemDiv.className = "item-box";
    itemDiv.id = `item-${newItem.id}`;
    itemDiv.innerHTML = buildEditRoleCard(newItem);
    container.appendChild(itemDiv);

    roleIn.value = "";
    qtyInput.value = "1";
    updateEditHiddenInputs();
}

// ── Remove role ──────────────────────────────────────────────────────
function removeEditRole(id) {
    const item = editRolesArray.find(r => r.id === id);
    if (!item) return;

    if ((occupiedRoleCounts[item.label] || 0) > 0) {
        alert("Cannot remove — has active members.");
        return;
    }

    editRolesArray = editRolesArray.filter(r => r.id !== id);
    const el = document.getElementById(`item-${id}`);
    if (el) el.remove();
    updateEditHiddenInputs();
}

// ── Change role quantity ─────────────────────────────────────────────
function changeEditRoleQty(id, delta) {
    const item = editRolesArray.find(r => r.id === id);
    if (!item) return;

    const occupied = occupiedRoleCounts[item.label] || 0;
    const minQty = Math.max(1, occupied);
    const newQty = item.quantity + delta;

    if (newQty < minQty) return;

    item.quantity = newQty;
    const qtySpan = document.getElementById(`qty-${id}`);
    if (qtySpan) qtySpan.textContent = newQty;
    updateEditHiddenInputs();
}

// ── Rename role ──────────────────────────────────────────────────────
function renameEditRole(id, newName) {
    const item = editRolesArray.find(r => r.id === id);
    if (!item) return;
    if ((occupiedRoleCounts[item.label] || 0) > 0) return;

    if (editRolesArray.some(r => r.id !== id && r.label === newName)) {
        alert("Duplicate role name.");
        return;
    }

    item.label = newName;
    updateEditHiddenInputs();
}

// ── Counting for the quantity input box ──────────────────────────────
function editCounting(adder, target) {
    const el = document.getElementById(target);
    let val = parseInt(el.value) || 0;
    let next = val + adder;
    if (next < 1) next = 1;
    el.value = next;
}

// ── Sync hidden inputs for form submission ───────────────────────────
function updateEditHiddenInputs() {
    const container = document.getElementById("roles-hidden-container");
    container.innerHTML = "";

    editRolesArray.forEach(function(item, i) {
        const nameInput = document.createElement("input");
        nameInput.type = "hidden";
        nameInput.name = `Roles[${i}].Name`;
        nameInput.value = item.label;

        const qtyInput = document.createElement("input");
        qtyInput.type = "hidden";
        qtyInput.name = `Roles[${i}].Quantity`;
        qtyInput.value = item.quantity;

        container.appendChild(nameInput);
        container.appendChild(qtyInput);
    });
}

// ── Initialize on DOM ready ──────────────────────────────────────────
document.addEventListener('DOMContentLoaded', function() {
    if (typeof initialRoles !== 'undefined' && initialRoles.length > 0) {
        initialRoles.forEach(function(role, idx) {
            editRolesArray.push({
                id: Date.now() + idx,
                label: role.name,
                quantity: role.quantity
            });
        });
    }

    const container = document.getElementById("roles-added");
    editRolesArray.forEach(function(item) {
        const itemDiv = document.createElement("div");
        itemDiv.className = "item-box";
        itemDiv.id = `item-${item.id}`;
        itemDiv.innerHTML = buildEditRoleCard(item);
        container.appendChild(itemDiv);
    });

    updateEditHiddenInputs();
    setupMaxPlayersConstraint();
});
