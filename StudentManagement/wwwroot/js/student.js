$(document).ready(function () {
    // Sidebar collapse toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Mark notification as read
    $('.notification-item').on('click', function () {
        $(this).addClass('read').css('opacity', '0.6');
    });

    // Filter classes by status
    $('#classFilter').on('change', function () {
        var status = $(this).val();
        if (status === '') {
            $('.class-card').show();
        } else {
            $('.class-card').hide();
            $('.class-card').filter(function () {
                return $(this).find('.badge').text().trim() === status;
            }).show();
        }
    });

    // Calculate average score
    function calculateAverage() {
        var total = 0;
        var count = 0;
        $('.score-value').each(function () {
            var score = parseFloat($(this).text());
            if (!isNaN(score)) {
                total += score;
                count++;
            }
        });
        if (count > 0) {
            var average = total / count;
            $('.overall-average').text(average.toFixed(2));
        }
    }

    calculateAverage();

    // Print schedule
    $('#printSchedule').on('click', function () {
        window.print();
    });

    // Export to PDF (requires jsPDF library)
    $('#exportPDF').on('click', function () {
        alert('Chức năng xuất PDF sẽ được triển khai với thư viện jsPDF');
    });
});