// ==========================================
// STUDENT DASHBOARD JAVASCRIPT - ENHANCED
// ==========================================

$(document).ready(function () {
    console.log('🎓 Student Dashboard Loaded');

    // ==========================================
    // 1. SIDEBAR COLLAPSE FUNCTIONALITY (ENHANCED)
    // ==========================================
    $('#sidebarCollapse').on('click', function () {
        const $sidebar = $('#sidebar');
        const $content = $('#content');
        const $icon = $(this).find('i');
        
        // Toggle collapsed state
        $sidebar.toggleClass('collapsed');
        $content.toggleClass('expanded');
        
        // Store state in localStorage
        const isCollapsed = $sidebar.hasClass('collapsed');
        localStorage.setItem('studentSidebarCollapsed', isCollapsed);
        
        // Toggle icon
        $icon.toggleClass('fa-bars fa-times');
        
        console.log('Sidebar toggled:', isCollapsed ? 'Collapsed' : 'Expanded');
    });

    // Restore sidebar state from localStorage on page load
    const sidebarCollapsed = localStorage.getItem('studentSidebarCollapsed');
    if (sidebarCollapsed === 'true') {
        $('#sidebar').addClass('collapsed');
        $('#content').addClass('expanded');
        $('#sidebarCollapse i').removeClass('fa-bars').addClass('fa-times');
    }

    // ==========================================
    // 2. LOGOUT FUNCTIONALITY - ENHANCED
    // ==========================================
    $('.logout-btn').on('click', function (e) {
        e.preventDefault();
        
        // Show confirmation dialog
        if (confirm('🚪 Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?')) {
            const $btn = $(this);
            const originalHTML = $btn.html();
            
            // Show loading state
            $btn.html('<i class="fas fa-spinner fa-spin"></i> Đang đăng xuất...');
            $btn.css('pointer-events', 'none');
            
            // Clear stored data
            localStorage.removeItem('studentSidebarCollapsed');
            sessionStorage.clear();
            
            // Redirect to logout action
            setTimeout(function () {
                // If you have a logout form, submit it
                if ($('#logoutForm').length) {
                    $('#logoutForm').submit();
                } else {
                    // Otherwise redirect to logout URL
                    window.location.href = '/Account/Logout';
                }
            }, 500);
        }
    });

    // ==========================================
    // 3. MARK NOTIFICATION AS READ
    // ==========================================
    $('.notification-item').on('click', function () {
        $(this).addClass('read').css('opacity', '0.6');
        
        // Update notification badge
        const unreadCount = $('.notification-item').not('.read').length;
        if (unreadCount > 0) {
            $('.notification-badge').text(unreadCount).show();
        } else {
            $('.notification-badge').hide();
        }
    });

    // ==========================================
    // 4. FILTER CLASSES BY STATUS
    // ==========================================
    $('#classFilter, #filterStatus').on('change', function () {
        const status = $(this).val();
        if (status === '') {
            $('.class-card, .class-card-item').show();
        } else {
            $('.class-card, .class-card-item').hide();
            $('.class-card, .class-card-item').filter(function () {
                return $(this).find('.badge').text().trim().toLowerCase().indexOf(status.toLowerCase()) > -1;
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
            
            // Update visual feedback
            if (average >= 8.5) {
                $('.overall-average').addClass('text-success');
            } else if (average >= 7.0) {
                $('.overall-average').addClass('text-info');
            } else if (average >= 5.5) {
                $('.overall-average').addClass('text-warning');
            } else {
                $('.overall-average').addClass('text-danger');
            }
        }
    }

    if ($('.score-value').length > 0) {
        calculateAverage();
    }

    // ==========================================
    // 6. SEARCH FUNCTIONALITY
    // ==========================================
    $('#searchClass, #searchStudent, #searchNotification').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        const targetSelector = $(this).attr('id') === 'searchNotification' 
            ? '.notification-card' 
            : 'tbody tr, .class-card, .class-card-item';
        
        $(targetSelector).filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // ==========================================
    // 7. PRINT FUNCTIONALITY
    // ==========================================
    $('#printSchedule').on('click', function () {
        window.print();
    });

    // Print event handlers
    window.onbeforeprint = function() {
        $('#sidebar, .top-navbar, .header-actions').hide();
        $('#content').css('margin-left', '0');
    };
    
    window.onafterprint = function() {
        $('#sidebar, .top-navbar, .header-actions').show();
        if ($('#sidebar').hasClass('collapsed')) {
            $('#content').css('margin-left', '80px');
        } else {
            $('#content').css('margin-left', '260px');
        }
    };

    // ==========================================
    // 8. EXPORT TO EXCEL/PDF
    // ==========================================
    $('#exportExcel, #exportPDF').on('click', function () {
        const type = $(this).attr('id') === 'exportExcel' ? 'Excel' : 'PDF';
        console.log(`Exporting to ${type}...`);
        // This will be implemented when you integrate export libraries
        alert(`Chức năng xuất ${type} sẽ được triển khai với thư viện tương ứng`);
    });

    // ==========================================
    // 9. AUTO-DISMISS ALERTS
    // ==========================================
    $('.alert').not('.alert-permanent').delay(5000).fadeOut('slow');

    // ==========================================
    // 10. TOOLTIPS INITIALIZATION
    // ==========================================
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // ==========================================
    // 11. PAYMENT FUNCTIONALITY
    // ==========================================
    $('.payment-btn').on('click', function(e) {
        e.preventDefault();
        const amount = $(this).data('amount');
        const className = $(this).data('class');
        
        if (confirm(`Xác nhận thanh toán ${amount} VNĐ cho lớp ${className}?`)) {
            // TODO: Integrate payment gateway
            console.log('Processing payment:', { amount, className });
            alert('Chức năng thanh toán sẽ được tích hợp với cổng thanh toán');
        }
    });

    // ==========================================
    // 12. SCHEDULE VIEW TOGGLE
    // ==========================================
    $('#toggleView, #toggleViewMode').on('click', function() {
        $('.schedule-view').toggleClass('d-none');
    });

    // ==========================================
    // 13. COPY TO CLIPBOARD
    // ==========================================
    $('.copy-btn').on('click', function() {
        const text = $(this).data('copy-text');
        navigator.clipboard.writeText(text).then(function() {
            alert('Đã sao chép: ' + text);
        }).catch(function() {
            console.error('Failed to copy text');
        });
    });

    // ==========================================
    // 14. CARD HOVER EFFECTS
    // ==========================================
    $('.class-card, .stat-card, .notification-card').hover(
        function() {
            $(this).addClass('shadow-lg');
        },
        function() {
            $(this).removeClass('shadow-lg');
        }
    );

    // ==========================================
    // 15. SMOOTH SCROLL
    // ==========================================
    $('a[href^="#"]').on('click', function(e) {
        const target = $(this.hash);
        if (target.length) {
            e.preventDefault();
            $('html, body').animate({
                scrollTop: target.offset().top - 100
            }, 500);
        }
    });

    console.log('✅ Student dashboard loaded successfully!');
});

// ==========================================
// UTILITY FUNCTIONS
// ==========================================

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Format date
function formatDate(date) {
    return new Date(date).toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
}

// Show toast notification (if using a toast library)
function showToast(title, message, type = 'info') {
    // This is a placeholder - implement with your preferred toast library
    console.log(`[${type.toUpperCase()}] ${title}: ${message}`);
}