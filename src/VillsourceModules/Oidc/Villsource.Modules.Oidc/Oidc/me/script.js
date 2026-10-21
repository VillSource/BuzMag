const LOGIN_LOCATION = "/api/v1/oidc/login";
const IS_AUTHENTICATION = "/api/v1/oidc/authentication";
const LOGOUT_LOCATION = "/api/v1/oidc/logout";
const CLAIM_LOCATION = "/api/v1/oidc/claims";


// fetch(IS_AUTHENTICATION).then(data =>{
//     if (!data.ok){
//         window.location.href = LOGIN_LOCATION;
//     }
// });

document.addEventListener("DOMContentLoaded", () => {

    // โหลดข้อมูล claims ทันทีที่หน้าเว็บโหลดเสร็จ
    fetchUserClaims();

    // กำหนด Event Listener ให้ปุ่ม Logout
    const logoutBtn = document.getElementById('logout-btn');
    if (logoutBtn) {
        logoutBtn.addEventListener('click', handleLogout);
    }
});

async function fetchUserClaims() {
    const container = document.getElementById('claims-container');

    try {
        const claims = await fetch(CLAIM_LOCATION)
            .then(res => {
                if (res.status === 401) window.location.href = LOGIN_LOCATION;
                if (res.ok) return res.json()
                return ({})
            });
        
        renderClaims(claims, container);
    } catch (error) {
        console.error("Error fetching claims:", error);
        container.innerHTML = '<p class="error-text">ไม่สามารถดึงข้อมูลผู้ใช้ได้ในขณะนี้</p>';
    }
}

function renderClaims(claims, container) {
    // เคลียร์ข้อความ Loading
    container.innerHTML = '';

    // ตรวจสอบว่ามีข้อมูลหรือไม่
    if (!claims || Object.keys(claims).length === 0) {
        container.innerHTML = '<p class="loading-text">ไม่พบข้อมูล Claims</p>';
        return;
    }

    // สร้างตารางเพื่อแสดงผล Key-Value
    const table = document.createElement('table');
    table.className = 'claims-table';

    for (const [key, value] of Object.entries(claims)) {
        const row = document.createElement('tr');

        const keyCell = document.createElement('th');
        keyCell.textContent = key.split('/').pop();

        const valueCell = document.createElement('td');
        // จัดการกรณีที่ value เป็น object หรือ array ให้อ่านง่ายขึ้น
        valueCell.textContent = typeof value === 'object' ? JSON.stringify(value) : value;

        row.appendChild(keyCell);
        row.appendChild(valueCell);
        table.appendChild(row);
    }

    container.appendChild(table);
}

async function handleLogout() {
    const res = await fetch(LOGOUT_LOCATION, {method: 'POST'});
    if (res.status === 200) {
        window.location.href = LOGIN_LOCATION;
    }
}

// ฟังก์ชันจำลองการตอบกลับของ API (เอาไว้ทดสอบ UI)
function mockApiCall() {
    return new Promise((resolve) => {
        setTimeout(() => {
            resolve({
                "sub": "user_123456789",
                "name": "Anirut Chaogla",
                "email": "anirut@example.com",
                "role": "Software Engineer",
                "department": "oCRM Team",
                "iss": "https://auth.example.com",
                "exp": 1704067200
            });
        }, 800); // ดีเลย์ 0.8 วินาทีเพื่อจำลองการรอโหลด
    });
}