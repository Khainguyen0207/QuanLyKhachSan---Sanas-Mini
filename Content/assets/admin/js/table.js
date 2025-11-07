$(document).ready(function () {
    new DataTable('#customer-table', {
        order: [[1, 'desc']],
    });
    new DataTable('#room-table', {
        order: [[1, 'desc']],
    });
    new DataTable('#resort-table', {
        order: [[1, 'desc']],
    });
})
