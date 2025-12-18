window.passkeyAuth = {
  login: async function (passkey) {
    const resp = await fetch("/auth/login", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ passkey })
    });

    if (resp.ok) {
      localStorage.setItem("passkey", passkey);
      return true;
    }
    return false;
  },

  logout: async function () {
    try {
      await fetch("/auth/logout", { method: "POST" });
    } catch { }
    localStorage.removeItem("passkey");
    return true;
  },

  me: async function () {
    try {
      const resp = await fetch("/auth/me");
      return resp.ok;
    } catch {
      return false;
    }
  },

  // ✅ dùng cho HubConnection (server-side) lấy passkey từ browser storage
  getPasskey: function () {
    return localStorage.getItem("passkey") || "";
  }
};
