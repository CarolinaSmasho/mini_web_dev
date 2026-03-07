async function postAction(url, body) {
  try {
    const params = new URLSearchParams(body);
    const response = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: params.toString(),
    });
    if (!response.ok) {
      const errorText = await response.text();
      alert("Error: " + (response.statusText || errorText));
      return { success: false, message: response.statusText };
    }
    return await response.json();
  } catch (e) {
    console.error(e);
    return { success: false, message: e.message };
  }
}

function editMission(id) {
  window.location = "/Lobby/EditMission?id=" + id;
}

async function toggleRecruitment(lobbyId) {
  const result = await postAction("/Lobby/ToggleRecruitment", { id: lobbyId });
  if (result.success) window.location.reload();
  else {
    alert(result.message || "Failed to toggle recruitment.");
    document.getElementById("recruit-toggle").checked =
      !document.getElementById("recruit-toggle").checked;
  }
}

async function completeMission(id) {
  if (!confirm("Complete this mission?")) return;
  const result = await postAction("/Lobby/CompleteMission", { id: id });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to complete mission.");
}

async function terminateLobby(id) {
  if (!confirm("Terminate this lobby? This cannot be undone.")) return;
  const result = await postAction("/Lobby/TerminateLobby", { id: id });
  if (result.success) window.location.href = result.redirectUrl || "/";
  else alert(result.message || "Failed to terminate lobby.");
}

async function recruitMember(lobbyId, userId) {
  const result = await postAction("/Lobby/Recruit", {
    id: lobbyId,
    userId: userId,
  });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to recruit member.");
}

async function rejectApplicant(lobbyId, userId) {
  const result = await postAction("/Lobby/Reject", {
    id: lobbyId,
    userId: userId,
  });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to reject applicant.");
}

async function kickMember(lobbyId, userId) {
  if (!confirm("Kick this member?")) return;
  const result = await postAction("/Lobby/Kick", {
    id: lobbyId,
    userId: userId,
  });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to kick member.");
}

async function requestDeployment(lobbyId) {
  const roleSelect = document.getElementById("apply-role");
  const role = roleSelect ? roleSelect.value : "Member";
  const result = await postAction("/Lobby/Apply", { id: lobbyId, role: role });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to send deployment request.");
}

async function cancelRequest(lobbyId) {
  const result = await postAction("/Lobby/CancelRequest", { id: lobbyId });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to cancel request.");
}

async function changeRole(lobbyId, userId, newRole) {
  const result = await postAction("/Lobby/ChangeRole", {
    id: lobbyId,
    userId: userId,
    newRole: newRole,
  });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to change role.");
}

async function abandonMission(lobbyId) {
  if (!confirm("Abandon this mission?")) return;
  const result = await postAction("/Lobby/AbandonMission", { id: lobbyId });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to abandon mission.");
}

async function submitKarma(lobbyId, targetUserId) {
  const select = document.getElementById("karma-" + targetUserId);
  const score = parseFloat(select.value);
  const result = await postAction("/Lobby/SubmitKarma", {
    id: lobbyId,
    targetUserId: targetUserId,
    score: score,
  });
  if (result.success) window.location.reload();
  else alert(result.message || "Failed to submit karma rating.");
}

// Countdown timer — reads initial seconds from #timer[data-seconds]
(function () {
  var el = document.getElementById("timer");
  if (!el) return;
  var secs = parseInt(el.dataset.seconds) || 0;
  function tick() {
    if (secs <= 0) {
      el.textContent = "EXPIRED";
      return;
    }
    var h = Math.floor(secs / 3600);
    var m = Math.floor((secs % 3600) / 60);
    var s = secs % 60;
    el.textContent =
      String(h).padStart(2, "0") +
      ":" +
      String(m).padStart(2, "0") +
      ":" +
      String(s).padStart(2, "0");
    secs--;
    setTimeout(tick, 1000);
  }
  tick();
})();
