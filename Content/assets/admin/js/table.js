$(document).ready(function () {
    new DataTable('#customer-table', {
        columnDefs: [
            {
                orderable: false,
                targets: [0]
            } // cột nào muốn cấm sort
        ],
        order: [
            [
                1, 'desc'
            ]
        ]
    });
    new DataTable('#room-table', {
        order: [[1, 'desc']],
    });
    new DataTable('#resort-table', {
        order: [[1, 'desc']],
    });
})
