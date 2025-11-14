// ==========================================
// ADMIN DASHBOARD JAVASCRIPT
// ==========================================

$(document).ready(function () {
    console.log('🔧 Admin JS Loaded');

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
    // 3. SELECT ALL CHECKBOX
    // ==========================================
    $('#selectAll').on('change', function () {
        $('.row-checkbox').prop('checked', this.checked);
    });

    // ==========================================
    // 4. SEARCH FUNCTIONALITY
    // ==========================================
    $('#searchStudent').on('keyup', function () {
        const value = $(this).val().toLowerCase();
        $(".data-table tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // ==========================================
    // 5. FILTER BY STATUS
    // ==========================================
    $('#filterStatus').on('change', function () {
        const status = $(this).val();
        if (status === '') {
            $('.data-table tbody tr').show();
        } else {
            $('.data-table tbody tr').hide();
            $('.data-table tbody tr').filter(function () {
                return $(this).find('.badge').text().trim() === status;
            }).show();
        }
    });

    // ==========================================
    // 6. ENROLLMENT CHART
    // ==========================================
    if (typeof Chart !== 'undefined' && document.getElementById('enrollmentChart')) {
        const ctx = document.getElementById('enrollmentChart').getContext('2d');
        const enrollmentChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6'],
                datasets: [{
                    label: 'Số lượng ghi danh',
                    data: [12, 19, 15, 25, 22, 30],
                    backgroundColor: 'rgba(52, 152, 219, 0.2)',
                    borderColor: 'rgba(52, 152, 219, 1)',
                    borderWidth: 2,
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
                    },
                    tooltip: {
                        mode: 'index',
                        intersect: false,
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    // ==========================================
    // 7. CONFIRM DELETE
    // ==========================================
    $('.btn-danger').on('click', function (e) {
        if (!$(this).hasClass('confirm-delete')) {
            if (!confirm('Bạn có chắc chắn muốn xóa?')) {
                e.preventDefault();
            }
        }
    });

    // ==========================================
    // 8. AUTO-DISMISS ALERTS
    // ==========================================
    $('.alert').not('.alert-permanent').delay(5000).fadeOut('slow');

    // ==========================================
    // 9. TOOLTIPS INITIALIZATION
    // ==========================================
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
});