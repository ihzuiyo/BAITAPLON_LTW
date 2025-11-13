$(document).ready(function () {
    // Sidebar collapse toggle
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('#content').toggleClass('active');
    });

    // Select all checkbox
    $('#selectAll').on('change', function () {
        $('.row-checkbox').prop('checked', this.checked);
    });

    // Search functionality
    $('#searchStudent').on('keyup', function () {
        var value = $(this).val().toLowerCase();
        $(".data-table tbody tr").filter(function () {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });

    // Filter by status
    $('#filterStatus').on('change', function () {
        var status = $(this).val();
        if (status === '') {
            $('.data-table tbody tr').show();
        } else {
            $('.data-table tbody tr').hide();
            $('.data-table tbody tr').filter(function () {
                return $(this).find('.badge').text().trim() === status;
            }).show();
        }
    });

    // Chart.js for enrollment chart (if Chart.js is included)
    if (typeof Chart !== 'undefined' && document.getElementById('enrollmentChart')) {
        var ctx = document.getElementById('enrollmentChart').getContext('2d');
        var enrollmentChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['Tháng 1', 'Tháng 2', 'Tháng 3', 'Tháng 4', 'Tháng 5', 'Tháng 6'],
                datasets: [{
                    label: 'Số lượng ghi danh',
                    data: [12, 19, 15, 25, 22, 30],
                    backgroundColor: 'rgba(52, 152, 219, 0.2)',
                    borderColor: 'rgba(52, 152, 219, 1)',
                    borderWidth: 2,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: true,
                        position: 'top',
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

    // Confirm delete
    $('.btn-danger').on('click', function (e) {
        if (!confirm('Bạn có chắc chắn muốn xóa?')) {
            e.preventDefault();
        }
    });
});