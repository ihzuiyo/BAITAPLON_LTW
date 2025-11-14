// ==========================================
// STUDENT DASHBOARD JAVASCRIPT
// ==========================================

$(document).ready(function () {
    console.log('🎓 Student JS Loaded');

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
    // 3. MARK NOTIFICATION AS READ
    // ==========================================
    $('.notification-item').on('click', function () {
        $(this).addClass('read').css('opacity', '0.6');
    });

    // ==========================================
    // 4. FILTER CLASSES BY STATUS
    // ==========================================
    $('#classFilter').on('change', function () {
        const status = $(this).val();
        if (status === '') {
            $('.class-card').show();
        } else {
            $('.class-card').hide();
            $('.class-card').filter(function () {
                return $(this).find('.badge').text().trim() === status;
            }).show();
        }
    });

    // ==========================================
    // 5. CALCULATE AVERAGE SCORE
    // ==========================================
    function calculateAverage() {
        let total = 0;
        let count = 0;
        $('.score-value').each(function () {
            const score = parseFloat($(this).text());
            if (!isNaN(score)) {
                total += score;
                count++;
            }
        });
        if (count > 0) {
            const average = total / count;
            $('.overall-average').text(average.toFixed(2));
        }
    }

    calculateAverage();

    // ==========================================
    // 6. PRINT SCHEDULE
    // ==========================================
    $('#printSchedule').on('click', function () {
        window.print();
    });

    // ==========================================
    // 7. EXPORT TO PDF
    // ==========================================
    $('#exportPDF').on('click', function () {
        alert('Chức năng xuất PDF sẽ được triển khai với thư viện jsPDF');
    });

    // ==========================================
    // 8. AUTO-DISMISS ALERTS
    // ==========================================
    $('.alert').not('.alert-permanent').delay(5000).fadeOut('slow');
});