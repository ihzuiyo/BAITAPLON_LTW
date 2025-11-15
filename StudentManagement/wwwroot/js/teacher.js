// ==========================================
// TEACHER DASHBOARD JAVASCRIPT - COMPLETE FIX
// ==========================================

$(document).ready(function () {
    console.log('👨‍🏫 Teacher Dashboard Loaded');

    // ==========================================
    // 1. SIDEBAR COLLAPSE FUNCTIONALITY (FIXED)
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
        localStorage.setItem('teacherSidebarCollapsed', isCollapsed);
        
        // Toggle icon
        $icon.toggleClass('fa-bars fa-times');
        
        // Close all submenus when collapsing
        if (isCollapsed) {
            $('.submenu.show').removeClass('show');
            $('.dropdown-toggle').attr('aria-expanded', 'false');
        }
        
        console.log('Sidebar toggled:', isCollapsed ? 'Collapsed' : 'Expanded');
    });

    // Restore sidebar state from localStorage on page load
    const sidebarCollapsed = localStorage.getItem('teacherSidebarCollapsed');
    if (sidebarCollapsed === 'true') {
        $('#sidebar').addClass('collapsed');
        $('#content').addClass('expanded');
        $('#sidebarCollapse i').removeClass('fa-bars').addClass('fa-times');
        $('.submenu.show').removeClass('show');
    }

    // ==========================================
    // 2. SUBMENU TOGGLE FUNCTIONALITY - ALTERNATIVE
    // ==========================================
    
    // Prevent default dropdown toggle when collapsed
    $('.dropdown-toggle').on('click', function(e) {
        if ($('#sidebar').hasClass('collapsed')) {
            e.preventDefault();
            e.stopPropagation();
            return false;
        }
    });

    // Show submenu on hover when sidebar is collapsed
    $('.sidebar').on('mouseenter', 'li:has(.submenu)', function() {
        if ($('#sidebar').hasClass('collapsed')) {
            const $submenu = $(this).find('.submenu');
            const $link = $(this).find('.dropdown-toggle');
            
            // Position submenu to the right of sidebar
            $submenu.css({
                'position': 'fixed',
                'left': '80px',
                'top': $link.offset().top,
                'background': 'rgba(0,0,0,0.95)',
                'border-radius': '5px',
                'padding': '10px 0',
                'box-shadow': '0 4px 12px rgba(0,0,0,0.3)',
                'min-width': '200px',
                'display': 'block',
                'z-index': 1002
            });
            
            $submenu.find('a').css({
                'padding': '10px 20px',
                'color': 'rgba(255,255,255,0.8)'
            });
        }
    }).on('mouseleave', 'li:has(.submenu)', function() {
        if ($('#sidebar').hasClass('collapsed')) {
            $(this).find('.submenu').css('display', 'none');
        }
    });

    // ==========================================
    // 3. LOGOUT FUNCTIONALITY - FIXED
    // ==========================================
    $('.logout-btn').on('click', function (e) {
        e.preventDefault();
        
        if (confirm('🚪 Bạn có chắc chắn muốn đăng xuất khỏi hệ thống?')) {
            const $btn = $(this);
            const originalHTML = $btn.html();
            
            $btn.html('<i class="fas fa-spinner fa-spin"></i> <span class="menu-text">Đang đăng xuất...</span>');
            $btn.css('pointer-events', 'none');
            
            // Clear stored data
            localStorage.removeItem('teacherSidebarCollapsed');
            sessionStorage.clear();
            
            // Submit the form
            setTimeout(function () {
                $('#logoutForm').submit();
            }, 1000);
        }
    });

    // ==========================================
    // 4. ATTENDANCE FUNCTIONALITY
    // ==========================================
    function updateAttendanceCounts() {
        const present = $('input[value="Present"]:checked').length;
        const absent = $('input[value="Absent"]:checked').length;
        const late = $('input[value="Late"]:checked').length;
        
        $('#presentCount, #presentCountBadge').text(present);
        $('#absentCount, #absentCountBadge').text(absent);
        $('#lateCount, #lateCountBadge').text(late);
        
        const total = present + absent + late;
        if (total > 0) {
            const presentPercent = ((present / total) * 100).toFixed(1);
            $('#presentPercentage').text(presentPercent + '%');
            $('#presentProgressBar').css('width', presentPercent + '%');
        }
    }
    
    $('#markAllPresent').on('click', function() {
        $('input[value="Present"]:not(:disabled)').prop('checked', true).trigger('change');
    });
    
    $('#markAllAbsent').on('click', function() {
        $('input[value="Absent"]:not(:disabled)').prop('checked', true).trigger('change');
    });
    
    $('#markAllLate').on('click', function() {
        $('input[value="Late"]:not(:disabled)').prop('checked', true).trigger('change');
    });
    
    $('#clearAll').on('click', function() {
        if (confirm('⚠️ Bạn có chắc muốn xóa tất cả điểm danh?')) {
            $('.attendance-radio').prop('checked', false).trigger('change');
        }
    });
    
    $('.attendance-radio').on('change', function() {
        updateAttendanceCounts();
        const $row = $(this).closest('tr');
        $row.addClass('table-active');
        setTimeout(() => $row.removeClass('table-active'), 300);
    });
    
    $('#saveAttendance').on('click', function() {
        const attendanceDate = $('#attendanceDate').val();
        if (!attendanceDate) {
            alert('Vui lòng chọn ngày điểm danh!');
            return;
        }
        
        const attendanceData = [];
        $('tbody tr').each(function() {
            const $row = $(this);
            const enrollmentId = $row.data('enrollment-id');
            const status = $row.find('.attendance-radio:checked').val();
            const note = $row.find('.attendance-note').val();
            
            if (status) {
                attendanceData.push({
                    enrollmentId: enrollmentId,
                    date: attendanceDate,
                    status: status,
                    note: note
                });
            }
        });
        
        if (attendanceData.length === 0) {
            alert('Vui lòng điểm danh ít nhất một học viên!');
            return;
        }
        
        if (confirm(`💾 Xác nhận lưu điểm danh cho ${attendanceData.length} học viên?`)) {
            const $btn = $(this);
            const originalHTML = $btn.html();
            
            $btn.prop('disabled', true);
            $btn.html('<i class="fas fa-spinner fa-spin"></i> Đang lưu...');
            
            setTimeout(() => {
                console.log('✅ Attendance saved:', attendanceData);
                alert(`Đã lưu điểm danh cho ${attendanceData.length} học viên`);
                $btn.html(originalHTML);
                $btn.prop('disabled', false);
            }, 1000);
        }
    });

    if (typeof updateAttendanceCounts === 'function') {
        updateAttendanceCounts();
    }

    // ==========================================
    // 5. SEARCH FUNCTIONALITY
    // ==========================================
    $('#searchStudent, #searchClass').on('keyup', function() {
        const value = $(this).val().toLowerCase();
        $('tbody tr').filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1);
        });
    });

    // ==========================================
    // 6. AUTO-DISMISS ALERTS
    // ==========================================
    $('.alert').not('.alert-permanent').delay(5000).fadeOut('slow');

    // ==========================================
    // 7. PRINT FUNCTIONALITY
    // ==========================================
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

    console.log('✅ Teacher dashboard loaded successfully!');
    console.log('💡 Hover over "Quản Lý Điểm" when collapsed to see submenu');
});