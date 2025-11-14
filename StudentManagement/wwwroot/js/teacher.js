// ==========================================
// TEACHER DASHBOARD JAVASCRIPT
// ==========================================

$(document).ready(function () {
    console.log('👨‍🏫 Teacher JS Loaded');

    // ==========================================
    // 1. SIDEBAR COLLAPSE FUNCTIONALITY
    // ==========================================
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('collapsed');
        $('#content').toggleClass('expanded');
        
        // Store collapse state in localStorage
        const isCollapsed = $('#sidebar').hasClass('collapsed');
        localStorage.setItem('sidebarCollapsed', isCollapsed);
        
        // Change icon
        $(this).find('i').toggleClass('fa-bars fa-times');
    });

    // Restore sidebar state from localStorage
    const sidebarCollapsed = localStorage.getItem('sidebarCollapsed');
    if (sidebarCollapsed === 'true') {
        $('#sidebar').addClass('collapsed');
        $('#content').addClass('expanded');
        $('#sidebarCollapse i').removeClass('fa-bars').addClass('fa-times');
    }

    // ==========================================
    // 2. LOGOUT FUNCTIONALITY
    // ==========================================
    $('.logout-btn').on('click', function (e) {
        e.preventDefault();
        
        // Show confirmation dialog
        if (confirm('Bạn có chắc chắn muốn đăng xuất?')) {
            // Show loading state
            const originalText = $(this).html();
            $(this).html('<i class="fas fa-spinner fa-spin"></i> Đang đăng xuất...');
            $(this).css('pointer-events', 'none');
            
            // Clear any stored data
            localStorage.removeItem('sidebarCollapsed');
            sessionStorage.clear();
            
            // Redirect to logout action
            setTimeout(function () {
                window.location.href = '/Account/Logout';
            }, 500);
        }
    });

    // ==========================================
    // 3. TOOLTIPS INITIALIZATION
    // ==========================================
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // ==========================================
    // 4. SCORE INPUT VALIDATION
    // ==========================================
    $('.score-input').on('input', function () {
        const value = parseFloat($(this).val());
        if (value < 0 || value > 10) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // ==========================================
    // 5. SAVE SCORES
    // ==========================================
    $('#saveScores').on('click', function () {
        let isValid = true;
        $('.score-input').each(function () {
            const value = parseFloat($(this).val());
            if (isNaN(value) || value < 0 || value > 10) {
                isValid = false;
                $(this).addClass('is-invalid');
            }
        });

        if (isValid) {
            alert('Lưu điểm thành công!');
        } else {
            alert('Vui lòng kiểm tra lại điểm số (0-10)');
        }
    });

    // ==========================================
    // 6. ATTENDANCE MARKING
    // ==========================================
    $('.attendance-checkbox').on('change', function () {
        const row = $(this).closest('tr');
        if ($(this).is(':checked')) {
            row.addClass('table-success');
        } else {
            row.removeClass('table-success');
        }
    });

    // ==========================================
    // 7. AUTO-DISMISS ALERTS
    // ==========================================
    $('.alert').not('.alert-permanent').delay(5000).fadeOut('slow');
});