// ==========================================
// ENHANCED AUTHENTICATION JAVASCRIPT
// ==========================================

// Global Variables
let loginAttempts = 0;
const maxLoginAttempts = 5;
let lockoutTimer = null;

// ==========================================
// 1. PASSWORD VISIBILITY TOGGLE
// ==========================================
function togglePassword(inputId, button) {
    const input = document.getElementById(inputId);
    const icon = button.querySelector('i');
    
    if (input.type === 'password') {
        input.type = 'text';
        icon.classList.remove('fa-eye');
        icon.classList.add('fa-eye-slash');
        button.setAttribute('aria-label', 'Hide password');
    } else {
        input.type = 'password';
        icon.classList.remove('fa-eye-slash');
        icon.classList.add('fa-eye');
        button.setAttribute('aria-label', 'Show password');
    }
}

// ==========================================
// 2. ACCEPT TERMS & CONDITIONS
// ==========================================
function acceptTerms() {
    const termsCheckbox = document.getElementById('AgreeToTerms');
    if (termsCheckbox) {
        termsCheckbox.checked = true;
        termsCheckbox.classList.remove('is-invalid');
    }
}

// ==========================================
// 3. FORM SUBMISSION WITH LOADING STATE
// ==========================================
function setupFormSubmission() {
    const authForms = document.querySelectorAll('.auth-form');
    
    authForms.forEach(form => {
        form.addEventListener('submit', function(e) {
            const submitBtn = this.querySelector('button[type="submit"]');
            
            if (submitBtn && !submitBtn.disabled) {
                // Show loading state
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span>Đang xử lý...';
                
                // Store original text for potential restoration
                submitBtn.setAttribute('data-original-text', originalText);
                
                // Set timeout to re-enable button (fallback)
                setTimeout(() => {
                    if (submitBtn.disabled) {
                        submitBtn.disabled = false;
                        submitBtn.innerHTML = originalText;
                    }
                }, 10000); // 10 seconds timeout
            }
        });
    });
}

// ==========================================
// 4. REAL-TIME FORM VALIDATION
// ==========================================
function setupRealtimeValidation() {
    // Email validation
    const emailInputs = document.querySelectorAll('input[type="email"]');
    emailInputs.forEach(input => {
        input.addEventListener('blur', function() {
            validateEmail(this);
        });
        
        input.addEventListener('input', function() {
            if (this.classList.contains('is-invalid')) {
                validateEmail(this);
            }
        });
    });
    
    // Phone number validation
    const phoneInputs = document.querySelectorAll('input[name="PhoneNumber"]');
    phoneInputs.forEach(input => {
        input.addEventListener('blur', function() {
            validatePhoneNumber(this);
        });
        
        input.addEventListener('input', function() {
            // Auto-format phone number
            this.value = formatPhoneNumber(this.value);
            
            if (this.classList.contains('is-invalid')) {
                validatePhoneNumber(this);
            }
        });
    });
    
    // Password confirmation
    const confirmPasswordInput = document.getElementById('ConfirmPassword');
    const passwordInput = document.getElementById('Password') || document.getElementById('registerPassword');
    
    if (confirmPasswordInput && passwordInput) {
        confirmPasswordInput.addEventListener('input', function() {
            validatePasswordMatch(passwordInput, confirmPasswordInput);
        });
        
        passwordInput.addEventListener('input', function() {
            if (confirmPasswordInput.value) {
                validatePasswordMatch(passwordInput, confirmPasswordInput);
            }
        });
    }
}

// ==========================================
// 5. VALIDATION HELPER FUNCTIONS
// ==========================================
function validateEmail(input) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const isValid = emailRegex.test(input.value.trim());
    
    if (input.value.trim() === '') {
        clearValidation(input);
        return true;
    }
    
    if (isValid) {
        showValid(input);
        return true;
    } else {
        showInvalid(input, 'Email không hợp lệ');
        return false;
    }
}

function validatePhoneNumber(input) {
    const phoneRegex = /^(\+84|0)[1-9]\d{8}$/;
    const cleanPhone = input.value.replace(/\s/g, '');
    const isValid = phoneRegex.test(cleanPhone);
    
    if (input.value.trim() === '') {
        clearValidation(input);
        return true;
    }
    
    if (isValid) {
        showValid(input);
        return true;
    } else {
        showInvalid(input, 'Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)');
        return false;
    }
}

function validatePasswordMatch(passwordInput, confirmInput) {
    if (confirmInput.value === '') {
        clearValidation(confirmInput);
        return true;
    }
    
    if (passwordInput.value === confirmInput.value) {
        showValid(confirmInput);
        return true;
    } else {
        showInvalid(confirmInput, 'Mật khẩu xác nhận không khớp');
        return false;
    }
}

function formatPhoneNumber(value) {
    // Remove all non-numeric characters except +
    let cleaned = value.replace(/[^\d+]/g, '');
    
    // Format: 0xxx xxx xxx or +84 xxx xxx xxx
    if (cleaned.startsWith('+84')) {
        cleaned = cleaned.substring(0, 12);
    } else if (cleaned.startsWith('0')) {
        cleaned = cleaned.substring(0, 10);
    }
    
    return cleaned;
}

function showValid(input) {
    input.classList.remove('is-invalid');
    input.classList.add('is-valid');
    
    const feedback = input.parentElement.querySelector('.invalid-feedback, .text-danger');
    if (feedback) {
        feedback.style.display = 'none';
    }
}

function showInvalid(input, message) {
    input.classList.remove('is-valid');
    input.classList.add('is-invalid');
    
    let feedback = input.parentElement.querySelector('.invalid-feedback, .text-danger');
    
    if (!feedback) {
        feedback = document.createElement('div');
        feedback.className = 'invalid-feedback';
        input.parentElement.appendChild(feedback);
    }
    
    feedback.textContent = message;
    feedback.style.display = 'block';
}

function clearValidation(input) {
    input.classList.remove('is-valid', 'is-invalid');
    
    const feedback = input.parentElement.querySelector('.invalid-feedback, .text-danger');
    if (feedback) {
        feedback.style.display = 'none';
    }
}

// ==========================================
// 6. AUTO-FILL DETECTION
// ==========================================
function detectAutofill() {
    const inputs = document.querySelectorAll('input:-webkit-autofill');
    
    inputs.forEach(input => {
        input.addEventListener('animationstart', function(e) {
            if (e.animationName === 'onAutoFillStart') {
                input.classList.add('autofilled');
            }
        });
    });
}

// ==========================================
// 7. CAPS LOCK DETECTION
// ==========================================
function setupCapsLockDetection() {
    const passwordInputs = document.querySelectorAll('input[type="password"]');
    
    passwordInputs.forEach(input => {
        input.addEventListener('keyup', function(e) {
            const capsLockOn = e.getModifierState && e.getModifierState('CapsLock');
            toggleCapsLockWarning(this, capsLockOn);
        });
    });
}

function toggleCapsLockWarning(input, isOn) {
    let warning = input.parentElement.querySelector('.caps-lock-warning');
    
    if (isOn) {
        if (!warning) {
            warning = document.createElement('small');
            warning.className = 'caps-lock-warning text-warning';
            warning.innerHTML = '<i class="fas fa-exclamation-triangle"></i> Caps Lock đang bật';
            input.parentElement.appendChild(warning);
        }
        warning.style.display = 'block';
    } else {
        if (warning) {
            warning.style.display = 'none';
        }
    }
}

// ==========================================
// 8. LOGIN ATTEMPT TRACKING
// ==========================================
function trackLoginAttempt(success = false) {
    if (success) {
        loginAttempts = 0;
        localStorage.removeItem('loginAttempts');
        localStorage.removeItem('lockoutTime');
        return;
    }
    
    loginAttempts++;
    localStorage.setItem('loginAttempts', loginAttempts);
    
    if (loginAttempts >= maxLoginAttempts) {
        const lockoutTime = Date.now() + (15 * 60 * 1000); // 15 minutes
        localStorage.setItem('lockoutTime', lockoutTime);
        lockAccount(lockoutTime);
    } else {
        const remainingAttempts = maxLoginAttempts - loginAttempts;
        showLoginWarning(`Còn ${remainingAttempts} lần thử. Tài khoản sẽ bị khóa tạm thời sau ${remainingAttempts} lần thất bại.`);
    }
}

function checkAccountLockout() {
    const lockoutTime = localStorage.getItem('lockoutTime');
    
    if (lockoutTime) {
        const remainingTime = parseInt(lockoutTime) - Date.now();
        
        if (remainingTime > 0) {
            lockAccount(parseInt(lockoutTime));
            return true;
        } else {
            // Lockout expired
            localStorage.removeItem('lockoutTime');
            localStorage.removeItem('loginAttempts');
            loginAttempts = 0;
        }
    } else {
        loginAttempts = parseInt(localStorage.getItem('loginAttempts') || '0');
    }
    
    return false;
}

function lockAccount(lockoutTime) {
    const loginForm = document.querySelector('.auth-form');
    const submitBtn = loginForm?.querySelector('button[type="submit"]');
    
    if (submitBtn) {
        submitBtn.disabled = true;
    }
    
    // Disable all inputs
    loginForm?.querySelectorAll('input').forEach(input => {
        input.disabled = true;
    });
    
    updateLockoutTimer(lockoutTime);
    
    // Start countdown
    lockoutTimer = setInterval(() => {
        const remaining = lockoutTime - Date.now();
        
        if (remaining <= 0) {
            clearInterval(lockoutTimer);
            unlockAccount();
        } else {
            updateLockoutTimer(lockoutTime);
        }
    }, 1000);
}

function updateLockoutTimer(lockoutTime) {
    const remaining = Math.max(0, lockoutTime - Date.now());
    const minutes = Math.floor(remaining / 60000);
    const seconds = Math.floor((remaining % 60000) / 1000);
    
    showLoginError(`Tài khoản tạm thời bị khóa. Vui lòng thử lại sau ${minutes}:${seconds.toString().padStart(2, '0')}`);
}

function unlockAccount() {
    localStorage.removeItem('lockoutTime');
    localStorage.removeItem('loginAttempts');
    loginAttempts = 0;
    
    const loginForm = document.querySelector('.auth-form');
    const submitBtn = loginForm?.querySelector('button[type="submit"]');
    
    if (submitBtn) {
        submitBtn.disabled = false;
    }
    
    loginForm?.querySelectorAll('input').forEach(input => {
        input.disabled = false;
    });
    
    hideLoginError();
    showLoginSuccess('Tài khoản đã được mở khóa. Bạn có thể đăng nhập lại.');
}

function showLoginWarning(message) {
    showMessage('warning', message);
}

function showLoginError(message) {
    showMessage('error', message);
}

function showLoginSuccess(message) {
    showMessage('success', message);
}

function hideLoginError() {
    const alertContainer = document.querySelector('.alert-container');
    if (alertContainer) {
        alertContainer.remove();
    }
}

function showMessage(type, message) {
    hideLoginError();
    
    const alertClass = {
        'success': 'alert-success',
        'error': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    }[type] || 'alert-info';
    
    const icon = {
        'success': 'fa-check-circle',
        'error': 'fa-exclamation-circle',
        'warning': 'fa-exclamation-triangle',
        'info': 'fa-info-circle'
    }[type] || 'fa-info-circle';
    
    const alertHTML = `
        <div class="alert ${alertClass} alert-dismissible fade show alert-container" role="alert">
            <i class="fas ${icon} me-2"></i>
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;
    
    const form = document.querySelector('.auth-form');
    if (form) {
        form.insertAdjacentHTML('afterbegin', alertHTML);
    }
}

// ==========================================
// 9. REMEMBER ME FUNCTIONALITY
// ==========================================
function setupRememberMe() {
    const rememberCheckbox = document.getElementById('RememberMe');
    const usernameInput = document.getElementById('UsernameOrEmail');
    
    if (rememberCheckbox && usernameInput) {
        // Load saved username
        const savedUsername = localStorage.getItem('rememberedUsername');
        if (savedUsername) {
            usernameInput.value = savedUsername;
            rememberCheckbox.checked = true;
        }
        
        // Save username on form submit
        const loginForm = usernameInput.closest('form');
        if (loginForm) {
            loginForm.addEventListener('submit', function() {
                if (rememberCheckbox.checked) {
                    localStorage.setItem('rememberedUsername', usernameInput.value);
                } else {
                    localStorage.removeItem('rememberedUsername');
                }
            });
        }
    }
}

// ==========================================
// 10. KEYBOARD SHORTCUTS
// ==========================================
function setupKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl/Cmd + Enter to submit form
        if ((e.ctrlKey || e.metaKey) && e.key === 'Enter') {
            const activeForm = document.querySelector('.auth-form');
            if (activeForm) {
                e.preventDefault();
                activeForm.querySelector('button[type="submit"]')?.click();
            }
        }
        
        // Escape to close modal/reset
        if (e.key === 'Escape') {
            const modal = document.querySelector('.modal.show');
            if (modal) {
                const closeBtn = modal.querySelector('.btn-close');
                closeBtn?.click();
            }
        }
    });
}

// ==========================================
// 11. FOCUS MANAGEMENT
// ==========================================
function setupFocusManagement() {
    // Auto-focus first input
    const firstInput = document.querySelector('.auth-form input:not([type="hidden"])');
    if (firstInput && !firstInput.value) {
        setTimeout(() => firstInput.focus(), 100);
    }
    
    // Tab trap in modals
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        modal.addEventListener('shown.bs.modal', function() {
            const focusableElements = this.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
            const firstElement = focusableElements[0];
            const lastElement = focusableElements[focusableElements.length - 1];
            
            this.addEventListener('keydown', function(e) {
                if (e.key === 'Tab') {
                    if (e.shiftKey && document.activeElement === firstElement) {
                        e.preventDefault();
                        lastElement.focus();
                    } else if (!e.shiftKey && document.activeElement === lastElement) {
                        e.preventDefault();
                        firstElement.focus();
                    }
                }
            });
        });
    });
}

// ==========================================
// 12. PASTE PREVENTION FOR PASSWORD CONFIRMATION
// ==========================================
function preventPasswordPaste() {
    const confirmPasswordInput = document.getElementById('ConfirmPassword');
    
    if (confirmPasswordInput) {
        confirmPasswordInput.addEventListener('paste', function(e) {
            e.preventDefault();
            showMessage('warning', 'Vui lòng nhập lại mật khẩu thay vì dán để đảm bảo chính xác');
        });
    }
}

// ==========================================
// 13. SESSION TIMEOUT WARNING
// ==========================================
let sessionTimeoutWarning = null;
let sessionTimeout = null;

function setupSessionTimeout(timeoutMinutes = 30) {
    const warningTime = (timeoutMinutes - 2) * 60 * 1000; // Warn 2 minutes before
    const timeoutTime = timeoutMinutes * 60 * 1000;
    
    function resetTimers() {
        clearTimeout(sessionTimeoutWarning);
        clearTimeout(sessionTimeout);
        
        sessionTimeoutWarning = setTimeout(() => {
            showMessage('warning', 'Phiên làm việc sắp hết hạn. Vui lòng lưu công việc của bạn.');
        }, warningTime);
        
        sessionTimeout = setTimeout(() => {
            window.location.href = '/Account/Login?timeout=true';
        }, timeoutTime);
    }
    
    // Reset on user activity
    const events = ['mousedown', 'keydown', 'scroll', 'touchstart'];
    events.forEach(event => {
        document.addEventListener(event, resetTimers, { passive: true });
    });
    
    resetTimers();
}

// ==========================================
// 14. EMAIL SUGGESTIONS
// ==========================================
function setupEmailSuggestions() {
    const emailInput = document.querySelector('input[type="email"]');
    
    if (emailInput) {
        const commonDomains = ['gmail.com', 'yahoo.com', 'outlook.com', 'hotmail.com'];
        
        emailInput.addEventListener('blur', function() {
            const email = this.value.trim();
            const atIndex = email.indexOf('@');
            
            if (atIndex > 0) {
                const domain = email.substring(atIndex + 1);
                const suggestions = commonDomains.filter(d => d.startsWith(domain) && d !== domain);
                
                if (suggestions.length > 0) {
                    showEmailSuggestion(this, email.substring(0, atIndex + 1) + suggestions[0]);
                }
            }
        });
    }
}

function showEmailSuggestion(input, suggestion) {
    let suggestionEl = input.parentElement.querySelector('.email-suggestion');
    
    if (!suggestionEl) {
        suggestionEl = document.createElement('small');
        suggestionEl.className = 'email-suggestion text-muted';
        suggestionEl.style.cursor = 'pointer';
        input.parentElement.appendChild(suggestionEl);
    }
    
    suggestionEl.innerHTML = `Ý bạn là <strong>${suggestion}</strong>?`;
    suggestionEl.style.display = 'block';
    
    suggestionEl.onclick = function() {
        input.value = suggestion;
        this.style.display = 'none';
        validateEmail(input);
    };
}

// ==========================================
// 15. ANIMATED ALERTS
// ==========================================
function showAnimatedAlert(message, type = 'info', duration = 5000) {
    const alertClass = {
        'success': 'alert-success',
        'error': 'alert-danger',
        'warning': 'alert-warning',
        'info': 'alert-info'
    }[type];
    
    const alertHTML = `
        <div class="alert ${alertClass} alert-dismissible fade show position-fixed top-0 start-50 translate-middle-x mt-3" 
             role="alert" style="z-index: 9999; min-width: 300px; animation: slideDown 0.3s ease-out;">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        </div>
    `;
    
    const tempDiv = document.createElement('div');
    tempDiv.innerHTML = alertHTML;
    const alert = tempDiv.firstElementChild;
    document.body.appendChild(alert);
    
    // Auto-dismiss
    setTimeout(() => {
        alert.style.animation = 'slideUp 0.3s ease-out';
        setTimeout(() => alert.remove(), 300);
    }, duration);
}

// ==========================================
// 16. INITIALIZE ALL FEATURES
// ==========================================
document.addEventListener('DOMContentLoaded', function() {
    console.log('🔐 Authentication JS Loaded');
    
    // Check account lockout status
    if (window.location.pathname.includes('/Login')) {
        checkAccountLockout();
    }
    
    // Initialize all features
    setupFormSubmission();
    setupRealtimeValidation();
    setupCapsLockDetection();
    setupRememberMe();
    setupKeyboardShortcuts();
    setupFocusManagement();
    preventPasswordPaste();
    setupEmailSuggestions();
    detectAutofill();
    
    // Setup session timeout for logged-in users
    const userId = sessionStorage.getItem('UserId');
    if (userId) {
        setupSessionTimeout(30);
    }
    
    // Handle timeout parameter from URL
    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get('timeout') === 'true') {
        showAnimatedAlert('Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.', 'warning');
    }
});

// Add CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideDown {
        from {
            opacity: 0;
            transform: translateX(-50%) translateY(-20px);
        }
        to {
            opacity: 1;
            transform: translateX(-50%) translateY(0);
        }
    }
    
    @keyframes slideUp {
        from {
            opacity: 1;
            transform: translateX(-50%) translateY(0);
        }
        to {
            opacity: 0;
            transform: translateX(-50%) translateY(-20px);
        }
    }
    
    @keyframes onAutoFillStart {
        from { }
        to { }
    }
    
    input:-webkit-autofill {
        animation-name: onAutoFillStart;
        transition: background-color 50000s ease-in-out 0s;
    }
    
    .caps-lock-warning {
        display: block;
        margin-top: 5px;
        font-size: 0.875rem;
    }
    
    .email-suggestion {
        display: block;
        margin-top: 5px;
    }
    
    .email-suggestion:hover {
        text-decoration: underline;
    }
    
    .spinner-border-sm {
        width: 1rem;
        height: 1rem;
        border-width: 0.15em;
    }
`;
document.head.appendChild(style);

// ==========================================
// 17. EXPORT PUBLIC API
// ==========================================
window.AuthJS = {
    togglePassword,
    acceptTerms,
    trackLoginAttempt,
    showAnimatedAlert,
    validateEmail,
    validatePhoneNumber
};