// Multi-Step Registration Form
let currentStep = 1;
const totalSteps = 4;

document.addEventListener('DOMContentLoaded', function() {
    updateStepDisplay();
    setupFormValidation();
    
    // Next button
    document.getElementById('nextBtn')?.addEventListener('click', function() {
        if (validateCurrentStep()) {
            if (currentStep < totalSteps) {
                currentStep++;
                updateStepDisplay();
            }
        }
    });
    
    // Previous button
    document.getElementById('prevBtn')?.addEventListener('click', function() {
        if (currentStep > 1) {
            currentStep--;
            updateStepDisplay();
        }
    });
    
    // Submit button
    document.getElementById('submitBtn')?.addEventListener('click', function(e) {
        if (!validateCurrentStep()) {
            e.preventDefault();
        }
    });
});

function updateStepDisplay() {
    // Update step indicators
    document.querySelectorAll('.step-item').forEach((item, index) => {
        const stepNum = index + 1;
        item.classList.remove('active', 'completed');
        
        if (stepNum < currentStep) {
            item.classList.add('completed');
        } else if (stepNum === currentStep) {
            item.classList.add('active');
        }
    });
    
    // Update form steps
    document.querySelectorAll('.form-step').forEach((step, index) => {
        step.classList.remove('active');
        if (index + 1 === currentStep) {
            step.classList.add('active');
        }
    });
    
    // Update navigation buttons
    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const submitBtn = document.getElementById('submitBtn');
    
    if (prevBtn) prevBtn.style.display = currentStep === 1 ? 'none' : 'block';
    if (nextBtn) nextBtn.style.display = currentStep === totalSteps ? 'none' : 'block';
    if (submitBtn) submitBtn.style.display = currentStep === totalSteps ? 'block' : 'none';
    
    // Update hidden field
    const currentStepInput = document.getElementById('currentStep');
    if (currentStepInput) currentStepInput.value = currentStep;
}

function validateCurrentStep() {
    const currentStepElement = document.querySelector(`.form-step[data-step="${currentStep}"]`);
    if (!currentStepElement) return true;
    
    const inputs = currentStepElement.querySelectorAll('input[required], select[required], textarea[required]');
    let isValid = true;
    
    inputs.forEach(input => {
        if (!input.value.trim()) {
            isValid = false;
            input.classList.add('is-invalid');
        } else {
            input.classList.remove('is-invalid');
        }
    });
    
    // Additional validation for specific steps
    if (currentStep === 1) {
        isValid = validateStep1() && isValid;
    } else if (currentStep === 3) {
        isValid = validateStep3() && isValid;
    } else if (currentStep === 4) {
        isValid = validateStep4() && isValid;
    }
    
    return isValid;
}

function validateStep1() {
    const email = document.getElementById('Email');
    const phone = document.getElementById('PhoneNumber');
    let isValid = true;
    
    // Email validation
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (email && email.value && !emailRegex.test(email.value)) {
        email.classList.add('is-invalid');
        showError(email, 'Email không hợp lệ');
        isValid = false;
    }
    
    // Phone validation (Vietnamese format)
    const phoneRegex = /^(\+84|0)[1-9]\d{8}$/;
    if (phone && phone.value && !phoneRegex.test(phone.value)) {
        phone.classList.add('is-invalid');
        showError(phone, 'Số điện thoại không hợp lệ (10 số, bắt đầu bằng 0)');
        isValid = false;
    }
    
    return isValid;
}

function validateStep3() {
    const username = document.getElementById('Username');
    const password = document.getElementById('Password');
    const confirmPassword = document.getElementById('ConfirmPassword');
    let isValid = true;
    
    // Username validation
    const usernameRegex = /^[a-zA-Z0-9_]{3,50}$/;
    if (username && username.value && !usernameRegex.test(username.value)) {
        username.classList.add('is-invalid');
        showError(username, 'Tên đăng nhập chỉ được chứa chữ, số và dấu gạch dưới (3-50 ký tự)');
        isValid = false;
    }
    
    // Password validation (SIMPLIFIED - No complexity requirements)
    if (password && password.value && password.value.length < 3) {
        password.classList.add('is-invalid');
        showError(password, 'Mật khẩu phải có ít nhất 3 ký tự');
        isValid = false;
    }
    
    // Confirm password validation
    if (password && confirmPassword && password.value !== confirmPassword.value) {
        confirmPassword.classList.add('is-invalid');
        showError(confirmPassword, 'Mật khẩu xác nhận không khớp');
        isValid = false;
    }
    
    return isValid;
}

function validateStep4() {
    const agreeToTerms = document.getElementById('AgreeToTerms');
    
    if (agreeToTerms && !agreeToTerms.checked) {
        agreeToTerms.classList.add('is-invalid');
        showError(agreeToTerms, 'Bạn phải đồng ý với điều khoản sử dụng');
        return false;
    }
    
    return true;
}

function showError(element, message) {
    let errorElement = element.nextElementSibling;
    if (!errorElement || !errorElement.classList.contains('text-danger')) {
        errorElement = document.createElement('span');
        errorElement.className = 'text-danger small';
        element.parentNode.insertBefore(errorElement, element.nextSibling);
    }
    errorElement.textContent = message;
    errorElement.style.display = 'block';
}

function updateSummary() {
    const fullNameEl = document.getElementById('summaryFullName');
    const emailEl = document.getElementById('summaryEmail');
    const phoneEl = document.getElementById('summaryPhone');
    const usernameEl = document.getElementById('summaryUsername');
    
    if (fullNameEl) fullNameEl.textContent = document.getElementById('FullName')?.value || '';
    if (emailEl) emailEl.textContent = document.getElementById('Email')?.value || '';
    if (phoneEl) phoneEl.textContent = document.getElementById('PhoneNumber')?.value || '';
    if (usernameEl) usernameEl.textContent = document.getElementById('Username')?.value || '';
}

function setupFormValidation() {
    // Real-time validation for inputs
    const inputs = document.querySelectorAll('.form-control, .form-select');
    
    inputs.forEach(input => {
        input.addEventListener('blur', function() {
            if (this.hasAttribute('required') && !this.value.trim()) {
                this.classList.add('is-invalid');
            } else {
                this.classList.remove('is-invalid');
            }
        });
        
        input.addEventListener('input', function() {
            this.classList.remove('is-invalid');
            const errorElement = this.nextElementSibling;
            if (errorElement && errorElement.classList.contains('text-danger')) {
                errorElement.style.display = 'none';
            }
        });
    });
}

function acceptTerms() {
    const termsCheckbox = document.getElementById('AgreeToTerms');
    if (termsCheckbox) {
        termsCheckbox.checked = true;
        termsCheckbox.classList.remove('is-invalid');
    }
}