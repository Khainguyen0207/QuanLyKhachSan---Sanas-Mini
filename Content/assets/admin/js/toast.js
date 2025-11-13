function showError(message, toastID) {
    return `
         <div class="toast align-items-center text-bg-danger border-0 show fade-in" role="alert" aria-live="assertive"
             aria-atomic="true" id="error-${toastID}" style="position: fixed; z-index: 9999;  right: 0; flex-basis: auto; margin-right: 10px;">
            <div class="d-flex">
                <div class="toast-body w-100">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
                        aria-label="Close" style="margin-right: 10px !important;"></button>
            </div>
        </div>
        <script>
            setTimeout(function () {
                $('#error-${toastID}').remove()
            }, 4000)
        </script>
    `
}

function showSuccess(message, toastID) {
    return `
         <div class="toast align-items-center text-bg-success border-0 show fade-in" role="alert" aria-live="assertive"
             aria-atomic="true" id="success-${toastID}" style="position: fixed; right: 0;z-index: 9999; flex-basis: auto; margin-right: 10px;">
            <div class="d-flex">
                <div class="toast-body w-100">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"
                        aria-label="Close" style="margin-right: 10px !important;"></button>
            </div>
        </div>
        <script>
            setTimeout(function () {
                $('#success-${toastID}').remove()
            }, 4000)
        </script>
    `
}

