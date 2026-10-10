const LOGIN_URL = "/connect/login";   // change to your endpoint
const TENANT_HEADER = "tenant";           // change to the header your API reads
const TENANT_KEY = "buzmag:lastTenant";
const POST_LOGIN = null;
const ME_LOCATION = "/api/v1/oidc/me";
const IS_AUTHENTICATION = "/api/v1/oidc/authentication";

const form = document.getElementById("login-form");
const tenantInput = document.getElementById("tenant");
const emailInput = document.getElementById("email");
const passwordInput = document.getElementById("password");
const submitBtn = document.getElementById("submit");
const errorEl = document.getElementById("error");

fetch(IS_AUTHENTICATION).then(data =>{
    if (data.ok){
        window.location.href = ME_LOCATION;
    }
});

// Restore last used tenant
try {
    const last = localStorage.getItem(TENANT_KEY);
    if (last) tenantInput.value = last;
} catch {}

// Focus first empty field
(tenantInput.value ? emailInput : tenantInput).focus();

form.addEventListener("submit", async (e) => {
    e.preventDefault();
    errorEl.textContent = "";

    const tenant = tenantInput.value.trim();
    const email = emailInput.value.trim();
    const password = passwordInput.value;

    if (!tenant || !email || !password) {
        errorEl.textContent = "Enter your tenant, email, and password.";
        return;
    }

    submitBtn.disabled = true;
    submitBtn.textContent = "Signing in…";

    try {
        const res = await fetch(LOGIN_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                [TENANT_HEADER]: tenant,
            },
            body: JSON.stringify({ email, password }),
        });

        if (!res.ok) {
            const msg = res.status === 401 || res.status === 400
                ? "Email or password is incorrect for this tenant."
                : "Couldn't sign in. Try again in a moment.";
            throw new Error(msg);
        }

        try { localStorage.setItem(TENANT_KEY, tenant); } catch {}

        const data = await res.json().catch(() => ({}));
        const params = new URLSearchParams(window.location.search);
        window.location.href = params.get("returnUrl") || POST_LOGIN || ME_LOCATION;
    } catch (err) {
        errorEl.textContent = err.message || "Couldn't reach the server.";
        submitBtn.disabled = false;
        submitBtn.textContent = "Sign in";
    }
});