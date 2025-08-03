// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Auto-hide alerts after 5 seconds
document.addEventListener('DOMContentLoaded', function() {
    const alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });
});

// Confirm delete actions
function confirmDelete(message) {
    return confirm(message || 'هل أنت متأكد من الحذف؟');
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('ar-SA', {
        style: 'currency',
        currency: 'SAR'
    }).format(amount);
}

// Phone number formatting
function formatPhoneNumber(phoneNumber) {
    // Remove all non-digit characters
    const cleaned = phoneNumber.replace(/\D/g, '');
    
    // Format as +966 XX XXX XXXX
    if (cleaned.length === 10 && cleaned.startsWith('0')) {
        return '+966 ' + cleaned.substring(1, 3) + ' ' + cleaned.substring(3, 6) + ' ' + cleaned.substring(6);
    } else if (cleaned.length === 9) {
        return '+966 ' + cleaned.substring(0, 2) + ' ' + cleaned.substring(2, 5) + ' ' + cleaned.substring(5);
    }
    
    return phoneNumber;
}

// Real-time search functionality
function setupSearch(inputId, tableId) {
    const searchInput = document.getElementById(inputId);
    const table = document.getElementById(tableId);
    
    if (searchInput && table) {
        searchInput.addEventListener('keyup', function() {
            const filter = this.value.toLowerCase();
            const rows = table.getElementsByTagName('tr');
            
            for (let i = 1; i < rows.length; i++) {
                const row = rows[i];
                const cells = row.getElementsByTagName('td');
                let found = false;
                
                for (let j = 0; j < cells.length; j++) {
                    if (cells[j].textContent.toLowerCase().includes(filter)) {
                        found = true;
                        break;
                    }
                }
                
                row.style.display = found ? '' : 'none';
            }
        });
    }
}

// Loading button state
function setLoadingState(button, loading) {
    if (loading) {
        button.disabled = true;
        button.innerHTML = '<span class="loading"></span> جاري التحميل...';
    } else {
        button.disabled = false;
        button.innerHTML = button.getAttribute('data-original-text') || 'إرسال';
    }
}

// Form validation helpers
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (form) {
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;
        
        inputs.forEach(function(input) {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
            }
        });
        
        return isValid;
    }
    return false;
}

