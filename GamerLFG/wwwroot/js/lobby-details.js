/* ─────────────────────────────────────────
   lobby-details.js   —  AJAX + Toast UI
───────────────────────────────────────── */

// ── Toast notification system ──────────────────────────────────────────────
function showToast(msg, type = "default", duration = 3000) {
  let container = document.getElementById("toast-container");
  if (!container) {
    container = document.createElement("div");
    container.id = "toast-container";
    document.body.appendChild(container);
  }
  const toast = document.createElement("div");
  toast.className = `toast ${type}`;
  toast.textContent = msg;
  container.appendChild(toast);
  setTimeout(() => {
    toast.classList.add("fade-out");
    setTimeout(() => toast.remove(), 300);
  }, duration);
}

// ── Core AJAX helper ──────────────────────────────────────────────────────
async function postAction(url, body) {
  try {
    const params = new URLSearchParams(body);
    const res = await fetch(url, {
      method: "POST",
      headers: { "Content-Type": "application/x-www-form-urlencoded" },
      body: params.toString(),
    });
    if (!res.ok) {
      const txt = await res.text();
      return { success: false, message: res.statusText || txt };
    }
    const data = await res.json();
    // ถ้า action สำเร็จ ให้ sync snapshot ทันที → ผู้กด action ไม่เห็น banner
    if (data.success && typeof window.syncLobbySnapshot === "function") {
      window.syncLobbySnapshot();
    }
    return data;
  } catch (e) {
    return { success: false, message: e.message };
  }
}

// ── Set a button to loading state and return a restore function ────────────
function setLoading(btn, label = "…") {
  const orig = btn.textContent;
  btn.classList.add("loading");
  btn.textContent = label;
  return () => {
    btn.classList.remove("loading");
    btn.textContent = orig;
  };
}

// ── Full-section refresh (fetches this same URL and replaces a container) ──
async function refreshSection(selector) {
  try {
    const res = await fetch(window.location.href, { cache: "no-store" });
    const html = await res.text();
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, "text/html");
    const fresh = doc.querySelector(selector);
    const current = document.querySelector(selector);
    if (fresh && current) {
      current.innerHTML = fresh.innerHTML;
      // re-run any one-time inits inside the refreshed section
      initCountdown();
    }
  } catch (_) {
    /* silently ignore parse errors on full reload */
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Lobby host actions
// ─────────────────────────────────────────────────────────────────────────

function editMission(id) {
  window.location.href = "/Lobby/EditMission?id=" + id;
}

async function toggleRecruitment(lobbyId) {
  const toggle = document.getElementById("recruit-toggle");
  const label = document.getElementById("recruit-label");
  const result = await postAction("/Lobby/ToggleRecruitment", { id: lobbyId });
  if (result.success) {
    const isNowOpen = result.isRecruiting;
    // Update toggle + label in place — no page reload
    toggle.checked = isNowOpen;
    label.style.color = isNowOpen ? "#2ecc71" : "#e74c3c";
    label.textContent = "Recruiting " + (isNowOpen ? "Open" : "Closed");
    showToast(
      isNowOpen ? "Recruitment opened" : "Recruitment closed",
      isNowOpen ? "success" : "error",
    );
  } else {
    toggle.checked = !toggle.checked; // revert
    showToast(result.message || "Failed to toggle recruitment.", "error");
  }
}

async function completeMission(id) {
  if (
    !confirm(
      "Complete this mission? Members will be prompted to rate each other.",
    )
  )
    return;
  const btn = event.currentTarget;
  const restore = setLoading(btn, "Completing…");
  const result = await postAction("/Lobby/CompleteMission", { id });
  restore();
  if (result.success) {
    showToast("Mission completed! 🎉", "success");
    setTimeout(() => window.location.reload(), 900);
  } else {
    showToast(result.message || "Failed to complete mission.", "error");
  }
}

async function terminateLobby(id) {
  if (!confirm("Terminate this lobby? This cannot be undone.")) return;
  const result = await postAction("/Lobby/TerminateLobby", { id });
  if (result.success) {
    showToast("Lobby terminated.", "error");
    setTimeout(() => (window.location.href = result.redirectUrl || "/"), 800);
  } else {
    showToast(result.message || "Failed to terminate lobby.", "error");
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Member management
// ─────────────────────────────────────────────────────────────────────────

async function recruitMember(lobbyId, userId) {
  const card = document.querySelector(`[data-applicant="${userId}"]`);
  const result = await postAction("/Lobby/Recruit", { id: lobbyId, userId });
  if (result.success) {
    if (card) {
      card.style.transition = "opacity .35s, transform .35s";
      card.style.opacity = "0";
      card.style.transform = "translateX(20px)";
      setTimeout(() => card.remove(), 380);
    }
    showToast("Member accepted ✓", "success");
    // refresh members grid in background
    setTimeout(() => refreshSection("#members-section"), 420);
  } else {
    showToast(result.message || "Failed to recruit member.", "error");
  }
}

async function rejectApplicant(lobbyId, userId) {
  const card = document.querySelector(`[data-applicant="${userId}"]`);
  const result = await postAction("/Lobby/Reject", { id: lobbyId, userId });
  if (result.success) {
    if (card) {
      card.style.transition = "opacity .3s";
      card.style.opacity = "0";
      setTimeout(() => card.remove(), 320);
    }
    showToast("Applicant rejected.", "error");
  } else {
    showToast(result.message || "Failed to reject applicant.", "error");
  }
}

async function kickMember(lobbyId, userId) {
  if (!confirm("Kick this member?")) return;
  const card = document.querySelector(`[data-member="${userId}"]`);
  const result = await postAction("/Lobby/Kick", { id: lobbyId, userId });
  if (result.success) {
    if (card) {
      card.style.transition = "opacity .3s, height .3s";
      card.style.opacity = "0";
      setTimeout(() => card.remove(), 320);
    }
    showToast("Member kicked.", "error");
  } else {
    showToast(result.message || "Failed to kick member.", "error");
  }
}

async function changeRole(lobbyId, userId, newRole) {
  const result = await postAction("/Lobby/ChangeRole", {
    id: lobbyId,
    userId,
    newRole,
  });
  if (result.success) {
    showToast("Role updated ✓", "success");
  } else {
    showToast(result.message || "Failed to change role.", "error");
    // revert select
    refreshSection("#members-section");
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Applicant actions (non-host)
// ─────────────────────────────────────────────────────────────────────────

async function requestDeployment(lobbyId) {
  const roleSelect = document.getElementById("apply-role");
  const role = roleSelect ? roleSelect.value : "Other";
  const btn = event.currentTarget;
  const restore = setLoading(btn, "Sending…");
  const result = await postAction("/Lobby/Apply", { id: lobbyId, role });
  restore();
  if (result.success) {
    showToast("Request sent! Waiting for host approval.", "success");
    setTimeout(() => window.location.reload(), 900);
  } else {
    showToast(result.message || "Failed to send request.", "error");
  }
}

async function cancelRequest(lobbyId) {
  const btn = event.currentTarget;
  const restore = setLoading(btn, "Cancelling…");
  const result = await postAction("/Lobby/CancelRequest", { id: lobbyId });
  restore();
  if (result.success) {
    showToast("Request cancelled.", "default");
    setTimeout(() => window.location.reload(), 700);
  } else {
    showToast(result.message || "Failed to cancel request.", "error");
  }
}

async function abandonMission(lobbyId) {
  if (!confirm("Abandon this mission? You will be removed from the lobby."))
    return;
  const result = await postAction("/Lobby/AbandonMission", { id: lobbyId });
  if (result.success) {
    showToast("You have abandoned the mission.", "error");
    setTimeout(() => window.location.reload(), 800);
  } else {
    showToast(result.message || "Failed to abandon.", "error");
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Invite system
// ─────────────────────────────────────────────────────────────────────────

async function inviteFriend(lobbyId, friendId) {
  const roleSelect = document.getElementById("invite-role-" + friendId);
  const role = roleSelect ? roleSelect.value : "Other";
  const row = document.querySelector(`[data-invite="${friendId}"]`);
  const result = await postAction("/Lobby/InviteFriend", {
    id: lobbyId,
    friendId,
    role,
  });
  if (result.success) {
    if (row) {
      const btn = row.querySelector("button");
      if (btn) {
        btn.textContent = "Invited ✓";
        btn.disabled = true;
        btn.style.opacity = ".5";
      }
    }
    showToast("Invite sent ✓", "success");
  } else {
    showToast(result.message || "Failed to send invite.", "error");
  }
}

async function acceptInvite(lobbyId) {
  const btn = event.currentTarget;
  const restore = setLoading(btn, "Accepting…");
  const result = await postAction("/Lobby/AcceptInvite", { id: lobbyId });
  restore();
  if (result.success) {
    showToast("Invite accepted! Waiting for host confirmation.", "success");
    setTimeout(() => window.location.reload(), 900);
  } else {
    showToast(result.message || "Failed to accept invite.", "error");
  }
}

async function declineInvite(lobbyId) {
  if (!confirm("Decline this invite?")) return;
  const result = await postAction("/Lobby/DeclineInvite", { id: lobbyId });
  if (result.success) {
    showToast("Invite declined.", "default");
    setTimeout(() => window.location.reload(), 700);
  } else {
    showToast(result.message || "Failed to decline invite.", "error");
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Karma submission — AJAX, update card in place
// ─────────────────────────────────────────────────────────────────────────

async function submitKarma(lobbyId, targetUserId) {
  const select = document.getElementById("karma-" + targetUserId);
  const btn = select?.closest(".karma-card")?.querySelector(".karma-btn");
  const score = parseFloat(select.value);

  // animate button
  if (btn) {
    btn.classList.add("loading");
    btn.textContent = "Submitting…";
  }

  const result = await postAction("/Lobby/SubmitKarma", {
    id: lobbyId,
    targetUserId,
    score,
  });

  if (result.success) {
    // Replace karma card content with "Rated" tag — no full reload
    const card = select?.closest(".karma-card");
    if (card) {
      card.innerHTML = `
        <img src="${card.querySelector("img")?.src || ""}" class="karma-avatar" />
        <p class="text-white fw-bold mb-1" style="font-size:12px;">${card.querySelector("p")?.textContent || ""}</p>
        <div class="evaluated-tag w-100">Rated ✓</div>`;
    }
    showToast("Karma submitted ✓", "success");
  } else {
    if (btn) {
      btn.classList.remove("loading");
      btn.textContent = "Submit";
    }
    showToast(result.message || "Failed to submit karma.", "error");
  }
}

// ─────────────────────────────────────────────────────────────────────────
// Mini-profile popup
// ─────────────────────────────────────────────────────────────────────────

async function openUserMiniProfile(userId) {
  const res = await fetch("/User/GetUserDetails?userId=" + userId);
  if (!res.ok) return;
  const user = await res.json();

  let popup = document.getElementById("mini-profile-popup");
  if (!popup) {
    popup = document.createElement("div");
    popup.id = "mini-profile-popup";
    popup.style.cssText =
      "position:fixed;inset:0;z-index:1000;display:flex;align-items:center;justify-content:center;background:#0006;backdrop-filter:blur(4px);";
    popup.addEventListener("click", (e) => {
      if (e.target === popup) popup.remove();
    });
    document.body.appendChild(popup);
  }

  popup.innerHTML = `
    <div style="background:linear-gradient(145deg,#111620,#0f121a);border:1px solid #1e2535;
                border-radius:20px;padding:28px 32px;width:min(360px,90vw);
                box-shadow:0 24px 64px #000a;animation:toast-in .25s cubic-bezier(.34,1.56,.64,1) both;">
      <div style="display:flex;align-items:center;gap:14px;margin-bottom:16px;">
        <img src="${user.avatar || "https://api.dicebear.com/7.x/avataaars/svg?seed=" + user.username}"
             style="width:56px;height:56px;border-radius:14px;border:2px solid #f2960d55;object-fit:cover;" />
        <div>
          <div style="font-weight:700;color:#e8ecf2;font-size:16px;">${user.username}</div>
          <div style="font-size:12px;color:#5a7090;margin-top:2px;">${user.status || ""}</div>
        </div>
        <button onclick="this.closest('#mini-profile-popup').remove()"
                style="margin-left:auto;background:none;border:none;color:#3a4a62;font-size:20px;cursor:pointer;line-height:1;">&times;</button>
      </div>
      <p style="font-size:13px;color:#8a9ab5;margin:0 0 16px;">${user.bio || "No bio."}</p>
      <div style="display:flex;gap:8px;flex-wrap:wrap;">
        ${
          user.isFriend
            ? `<span style="font-size:11px;background:#2ecc7115;color:#2ecc71;border:1px solid #2ecc7133;padding:4px 12px;border-radius:6px;font-weight:700;">✓ Friend</span>`
            : user.isPending
              ? `<span style="font-size:11px;background:#f2960d15;color:#f2960d;border:1px solid #f2960d33;padding:4px 12px;border-radius:6px;font-weight:700;">Pending</span>`
              : ""
        }
        <a href="/User/Profiles?id=${user.id}"
           style="font-size:11px;background:#1a2535;color:#8a9ab5;border:1px solid #1e2a3a;padding:4px 12px;border-radius:6px;text-decoration:none;font-weight:600;">
          View Profile →
        </a>
      </div>
    </div>`;
}

// ─────────────────────────────────────────────────────────────────────────
// Countdown timer
// ─────────────────────────────────────────────────────────────────────────

function initCountdown() {
  const el = document.getElementById("timer");
  if (!el) return;
  let secs = parseInt(el.dataset.seconds) || 0;
  const lbl = el.dataset.label || "";

  function fmt(s) {
    if (s <= 0) return null;
    const d = Math.floor(s / 86400);
    const h = Math.floor((s % 86400) / 3600);
    const m = Math.floor((s % 3600) / 60);
    const sec = s % 60;
    const pad = (n) => String(n).padStart(2, "0");
    return d > 0
      ? `${d}d ${pad(h)}:${pad(m)}:${pad(sec)}`
      : `${pad(h)}:${pad(m)}:${pad(sec)}`;
  }

  function render() {
    const time = fmt(secs);
    if (!time) {
      el.innerHTML = `<span class="cd-timer">EXPIRED</span>`;
      return;
    }
    el.innerHTML = `<span style="color:#5a7090;">${lbl}</span> <span class="cd-timer">${time}</span>`;
    secs--;
    setTimeout(render, 1000);
  }
  render();
}

document.addEventListener("DOMContentLoaded", initCountdown);

// ─────────────────────────────────────────────────────────────────────────
// Lobby Change Detection — Polling every 5s
// ─────────────────────────────────────────────────────────────────────────
function startChangeDetection() {
  const root = document.getElementById("lobby-snapshot-root");
  if (!root) {
    console.warn("[Snapshot] lobby-snapshot-root not found");
    return;
  }

  const lobbyId = root.dataset.lobbyId;
  if (!lobbyId || !root.dataset.snapshot) {
    console.warn("[Snapshot] missing data attrs");
    return;
  }

  const banner = document.getElementById("refresh-banner");
  if (!banner) {
    console.warn("[Snapshot] refresh-banner not found");
    return;
  }

  // knownHash คือ hash ที่ user คนนี้รู้จักอยู่แล้ว — เปลี่ยนได้เมื่อ user ทำ action เอง
  let knownHash = root.dataset.snapshot;
  let changed = false;
  let paused = false;

  async function poll() {
    if (changed || paused) return;
    try {
      const res = await fetch(`/Lobby/Snapshot?id=${lobbyId}`, {
        cache: "no-store",
      });
      if (!res.ok) return;
      const data = await res.json();
      if (data.hash && data.hash !== knownHash) {
        changed = true;
        banner.classList.add("visible");
        console.log("[Snapshot] lobby changed:", knownHash, "→", data.hash);
      }
    } catch (e) {
      console.warn("[Snapshot poll error]", e);
    }
  }

  // เรียกจาก action functions เพื่อ sync hash ใหม่ → ผู้กด action ไม่เห็น banner
  window.syncLobbySnapshot = async function () {
    try {
      const res = await fetch(`/Lobby/Snapshot?id=${lobbyId}`, {
        cache: "no-store",
      });
      if (!res.ok) return;
      const data = await res.json();
      if (data.hash) {
        knownHash = data.hash;
        changed = false; // reset เผื่อ banner แสดงอยู่ก่อน action
        banner.classList.remove("visible");
        console.log("[Snapshot] synced → knownHash:", knownHash);
      }
    } catch (_) {}
  };

  window.doRefreshPage = () => {
    // ใช้ cache-bust param บังคับ browser โหลดใหม่จาก server ไม่ใช่จาก cache
    const url = new URL(window.location.href);
    url.searchParams.set("_t", Date.now());
    window.location.href = url.toString();
  };
  window.dismissBanner = () => {
    banner.classList.remove("visible");
    paused = true;
    changed = false;
    setTimeout(() => {
      paused = false;
    }, 60_000);
  };

  setInterval(poll, 5_000);
  console.log(
    "[Snapshot] polling started — lobby:",
    lobbyId,
    "| hash:",
    knownHash,
  );
}

// script อยู่ท้าย body → DOM พร้อมแน่นอนแล้ว ไม่ต้องรอ event
if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", startChangeDetection);
} else {
  startChangeDetection();
}
