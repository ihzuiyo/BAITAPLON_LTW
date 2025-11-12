$(document).ready(function () {
    // Sidebar collapse toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Score input validation
    $('.score-input').on('input', function () {
        var value = parseFloat($(this).val());
        if (value < 0 || value > 10) {
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });

    // Save scores
    $('#saveScores').on('click', function () {
        var isValid = true;
        $('.score-input').each(function () {
            var value = parseFloat($(this).val());
            if (isNaN(value) || value < 0 || value > 10) {
                isValid = false;
                $(this).addClass('is-invalid');
            }
        });

        if (isValid) {
            // Submit form or make AJAX request
            alert('Lưu điểm thành công!');
        } else {
            alert('Vui lòng kiểm tra lại điểm số (0-10)');
        }
    });

    // Attendance marking
    $('.attendance-checkbox').on('change', function () {
        var row = $(this).closest('tr');
        if ($(this).is(':checked')) {
            row.addClass('table-success');
        } else {
            row.removeClass('table-success');
        }
    });
});