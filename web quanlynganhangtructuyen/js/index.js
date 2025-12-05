// Generate stars
const starsContainer = document.getElementById('stars');
if (starsContainer) {
    for (let i = 0; i < 100; i++) {
        const star = document.createElement('div');
        star.className = 'star';
        star.style.left = Math.random() * 100 + '%';
        star.style.top = Math.random() * 100 + '%';
        star.style.animationDelay = Math.random() * 3 + 's';
        starsContainer.appendChild(star);
    }
}

// Generate windows for center building (only if it exists)
const windowGrid = document.getElementById('windowGrid');
if (windowGrid) {
    for (let i = 0; i < 60; i++) {
        const window = document.createElement('div');
        window.className = 'window';
        window.style.animationDelay = Math.random() * 4 + 's';
        windowGrid.appendChild(window);
    }
}

// Toggle password visibility
function togglePassword() {
    const passwordInput = document.getElementById('password');
    passwordInput.type = passwordInput.type === 'password' ? 'text' : 'password';
}

// Refresh captcha
function refreshCaptcha() {
    const captchaText = document.getElementById('captchaText');
    const newCaptcha = Math.floor(10000 + Math.random() * 90000);
    captchaText.textContent = newCaptcha;
    captchaText.style.transform = 'rotate(360deg)';
    setTimeout(() => {
        captchaText.style.transform = 'rotate(0deg)';
    }, 300);
}

// Handle form submission
document.getElementById('loginForm').addEventListener('submit', function(e) {
    e.preventDefault();
    alert('Đăng nhập thành công!');
});
