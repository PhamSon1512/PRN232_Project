/**
 * Simplified Doctor Schedule Management JavaScript
 * Only handles client-side validation and UX enhancements
 * Most functionality now handled server-side
 */

document.addEventListener('DOMContentLoaded', function() {
    // Auto-hide alerts after 5 seconds
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            if (alert.parentNode) {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '0';
                setTimeout(() => {
                    alert.remove();
                }, 500);
            }
        }, 5000);
    });

    // Store all rooms data for filtering
    const roomSelect = document.getElementById('roomSelect');
    const departmentSelect = document.getElementById('departmentSelect');
    let allRooms = [];
    
    // Store all room options on page load
    if (roomSelect) {
        const roomOptions = Array.from(roomSelect.querySelectorAll('option[data-department]'));
        allRooms = roomOptions.map(option => ({
            id: option.value,
            name: option.textContent,
            departmentId: option.getAttribute('data-department')
        }));
        
        // Also get rooms from hidden options that might be filtered out
        const hiddenRoomsData = document.getElementById('allRoomsData');
        if (hiddenRoomsData) {
            try {
                const additionalRooms = JSON.parse(hiddenRoomsData.textContent);
                allRooms = allRooms.concat(additionalRooms);
            } catch (e) {
                console.log('No additional rooms data found');
            }
        }
    }

    // Handle department change to filter rooms
    if (departmentSelect) {
        departmentSelect.addEventListener('change', function() {
            const selectedDepartmentId = this.value;
            
            // Clear room options
            roomSelect.innerHTML = '<option value="">-- Chọn phòng --</option>';
            
            if (selectedDepartmentId) {
                // Filter and add rooms for selected department
                const departmentRooms = allRooms.filter(room => room.departmentId === selectedDepartmentId);
                
                departmentRooms.forEach(room => {
                    const option = document.createElement('option');
                    option.value = room.id;
                    option.textContent = room.name;
                    option.setAttribute('data-department', room.departmentId);
                    roomSelect.appendChild(option);
                });
                
                // Auto-select first room if available
                if (departmentRooms.length > 0) {
                    roomSelect.value = departmentRooms[0].id;
                }
            }
        });
    }

    // Bulk registration form validation
    const bulkRegisterForm = document.getElementById('bulkRegisterForm');
    if (bulkRegisterForm) {
        bulkRegisterForm.addEventListener('submit', function(e) {
            const selectedDates = bulkRegisterForm.querySelectorAll('input[name="selectedDates"]:checked');
            const selectedPeriods = bulkRegisterForm.querySelectorAll('input[name="periods"]:checked');
            
            if (selectedDates.length === 0) {
                e.preventDefault();
                alert('Vui lòng chọn ít nhất một ngày!');
                return false;
            }
            
            if (selectedPeriods.length === 0) {
                e.preventDefault();
                alert('Vui lòng chọn ít nhất một ca làm việc!');
                return false;
            }
            
            // Show loading state
            const submitBtn = bulkRegisterForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
            }
            
            return true;
        });
    }

    // Bulk cancel form validation
    const bulkCancelForm = document.getElementById('bulkCancelForm');
    if (bulkCancelForm) {
        bulkCancelForm.addEventListener('submit', function(e) {
            const submitBtn = bulkCancelForm.querySelector('button[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
            }
        });
    }

    // Add loading states to all form submissions
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function() {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn && !submitBtn.disabled) {
                const originalText = submitBtn.innerHTML;
                submitBtn.disabled = true;
                
                // Add spinner only if not already present
                if (!originalText.includes('hourglass-split')) {
                    submitBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
                }
                
                // Re-enable after 10 seconds (safety measure)
                setTimeout(() => {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }, 10000);
            }
        });
    });

    // Smooth scroll to alerts
    const alertContainer = document.getElementById('alertContainer');
    if (alertContainer && alertContainer.children.length > 0) {
        alertContainer.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    // Enhanced form validation feedback
    const requiredSelects = document.querySelectorAll('select[required]');
    requiredSelects.forEach(select => {
        select.addEventListener('change', function() {
            if (this.value) {
                this.classList.remove('is-invalid');
                this.classList.add('is-valid');
            } else {
                this.classList.remove('is-valid');
                this.classList.add('is-invalid');
            }
        });
    });

    // Keyboard shortcuts
    document.addEventListener('keydown', function(e) {
        // Ctrl + Enter to submit current form
        if (e.ctrlKey && e.key === 'Enter') {
            const focusedElement = document.activeElement;
            const form = focusedElement.closest('form');
            if (form) {
                const submitBtn = form.querySelector('button[type="submit"]');
                if (submitBtn && !submitBtn.disabled) {
                    submitBtn.click();
                }
            }
        }
    });

    console.log('Simplified schedule management initialized - server-side mode');
});

// Utility function for showing temporary messages
function showTemporaryMessage(message, type = 'info') {
    const alertContainer = document.getElementById('alertContainer');
    if (alertContainer) {
        const alertId = 'temp_alert_' + Date.now();
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        alertContainer.insertAdjacentHTML('beforeend', alertHtml);
        
        // Auto-remove after 5 seconds
        setTimeout(() => {
            const alert = document.getElementById(alertId);
            if (alert) {
                alert.style.transition = 'opacity 0.5s ease';
                alert.style.opacity = '0';
                setTimeout(() => {
                    alert.remove();
                }, 500);
            }
        }, 5000);
    }
}
