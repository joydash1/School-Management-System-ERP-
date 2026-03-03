$('select.form-select').each(function () {
    var $select = $(this);
    var firstZeroOption = $select.find('option[value="0"]').first();
    var placeholderText = firstZeroOption.length ? firstZeroOption.text() : 'Select an option';

    $select.select2({
        theme: 'bootstrap-5',
        width: '100%',
        placeholder: placeholderText,
        allowClear: true
    });
    if (firstZeroOption.length) firstZeroOption.hide();
});

$(document).on('focus', 'select.form-select', function () {
    if (!$(this).hasClass('select2-hidden-accessible')) {
        $(this).select2({
            theme: 'bootstrap-5',
            width: '100%',
            placeholder: 'Select an option',
            allowClear: true
        });
    }
});

// ==================== SWEETALERT2 HELPER FUNCTIONS ====================

// Success Popup
window.showSuccess = function (message, title = 'Success!', reloadAfter = 0) {
    Swal.fire({
        icon: 'success',
        title: title,
        text: message,
        confirmButtonColor: '#10b981',
        confirmButtonText: '<i class="fa-solid fa-check me-2"></i>OK',
        timer: reloadAfter > 0 ? 3000 : undefined,
        timerProgressBar: true,
        showClass: {
            popup: 'animate__animated animate__fadeInDown'
        },
        hideClass: {
            popup: 'animate__animated animate__fadeOutUp'
        }
    }).then(() => {
        if (reloadAfter > 0) {
            setTimeout(() => {
                window.location.reload();
            }, reloadAfter);
        }
    });
};

// Error Popup with single message
window.showError = function (message, title = 'Error!') {
    Swal.fire({
        icon: 'error',
        title: title,
        html: `
                <div class="text-center">
                    <i class="fa-solid fa-circle-exclamation fa-3x text-danger mb-3"></i>
                    <p class="text-danger">${message}</p>
                </div>
            `,
        confirmButtonColor: '#dc2626',
        confirmButtonText: '<i class="fa-solid fa-times me-2"></i>Close',
        showClass: {
            popup: 'animate__animated animate__shake'
        }
    });
};

// Enhanced Error Popup with multiple errors
window.showErrors = function (errors, title = 'Validation Failed!') {
    let errorHtml = '<div class="error-list">';

    if (Array.isArray(errors)) {
        errors.forEach(error => {
            errorHtml += `<div class="error-item">
                <i class="fa-solid fa-exclamation-circle"></i> ${error}
            </div>`;
        });
    } else if (typeof errors === 'string') {
        errorHtml += `<div class="error-item">
            <i class="fa-solid fa-exclamation-circle"></i> ${errors}
        </div>`;
    } else if (errors.errors && Array.isArray(errors.errors)) {
        // Handle nested errors object
        errors.errors.forEach(error => {
            errorHtml += `<div class="error-item">
                <i class="fa-solid fa-exclamation-circle"></i> ${error}
            </div>`;
        });
    } else {
        errorHtml += `<div class="error-item">
            <i class="fa-solid fa-exclamation-circle"></i> Something went wrong
        </div>`;
    }

    errorHtml += '</div>';

    Swal.fire({
        icon: 'error',
        title: title,
        html: `
            <div class="text-center">
                <i class="fa-solid fa-circle-exclamation fa-3x text-danger mb-3"></i>
                ${errorHtml}
            </div>
        `,
        confirmButtonColor: '#dc2626',
        confirmButtonText: '<i class="fa-solid fa-times me-2"></i>Close',
        width: '500px',
        showClass: {
            popup: 'animate__animated animate__shake'
        }
    });
};
// Warning Popup
window.showWarning = function (message, title = 'Warning!') {
    Swal.fire({
        icon: 'warning',
        title: title,
        html: `
                <div class="text-center">
                    <i class="fa-solid fa-triangle-exclamation fa-3x text-warning mb-3"></i>
                    <p class="text-warning">${message}</p>
                </div>
            `,
        confirmButtonColor: '#f59e0b',
        confirmButtonText: '<i class="fa-solid fa-check me-2"></i>OK',
        showClass: {
            popup: 'animate__animated animate__pulse'
        }
    });
};

// Info Popup
window.showInfo = function (message, title = 'Information') {
    Swal.fire({
        icon: 'info',
        title: title,
        text: message,
        confirmButtonColor: '#2563eb',
        confirmButtonText: '<i class="fa-solid fa-check me-2"></i>Got it',
        showClass: {
            popup: 'animate__animated animate__fadeIn'
        }
    });
};

// Confirmation Dialog
window.showConfirm = function (message, callback, title = 'Are you sure?') {
    Swal.fire({
        title: title,
        text: message,
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#10b981',
        cancelButtonColor: '#dc2626',
        confirmButtonText: '<i class="fa-solid fa-check me-2"></i>Yes',
        cancelButtonText: '<i class="fa-solid fa-times me-2"></i>No',
        reverseButtons: true,
        showClass: {
            popup: 'animate__animated animate__fadeIn'
        }
    }).then((result) => {
        if (result.isConfirmed && callback) {
            callback();
        }
    });
};

// Loading Popup
window.showLoading = function (message = 'Processing...') {
    Swal.fire({
        title: message,
        html: '<div class="custom-loader"></div>',
        showConfirmButton: false,
        allowOutsideClick: false,
        allowEscapeKey: false,
        showClass: {
            popup: 'animate__animated animate__fadeIn'
        }
    });
};

// Close Loading Popup
window.closeLoading = function () {
    Swal.close();
};

// Parse API Response and Show Appropriate Message
window.handleApiResponse = function (response) {
    if (response.isSuccess) {
        showSuccess(response.message || 'Operation completed successfully');
    } else {
        if (response.errors && response.errors.length > 0) {
            showErrors(response.errors);
        } else {
            showError(response.message || 'Something went wrong');
        }
    }
};

// Handle AJAX Errors
window.handleAjaxError = function (xhr, status, error) {
    if (xhr.status === 400 && xhr.responseJSON) {
        // Handle validation errors
        if (xhr.responseJSON.errors) {
            showErrors(xhr.responseJSON.errors);
        } else if (xhr.responseJSON.message) {
            showError(xhr.responseJSON.message);
        } else {
            showError('Validation failed. Please check your input.');
        }
    } else if (xhr.status === 404) {
        showError('Resource not found');
    } else if (xhr.status === 401) {
        showError('Unauthorized access');
    } else if (xhr.status === 403) {
        showError('Access forbidden');
    } else if (xhr.status === 500) {
        showError('Server error. Please try again later.');
    } else {
        showError(xhr.responseText || 'Something went wrong');
    }
};