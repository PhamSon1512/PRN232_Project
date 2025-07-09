// Common functionality for schedule management
class ScheduleManager {
    constructor() {
        this.selectedSlots = [];
        this.init();
    }

    init() {
        this.bindEvents();
        this.updateFilterDependencies();
    }

    bindEvents() {
        // Auto-submit filter forms
        $('.filter-select').on('change', (e) => {
            this.handleFilterChange(e.target);
        });

        // Handle slot selection
        $(document).on('click', '.slot-btn', (e) => {
            this.handleSlotSelection(e.target);
        });

        // Handle period selection (morning/afternoon)
        $(document).on('click', '.select-period-btn', (e) => {
            this.handlePeriodSelection(e.target);
        });

        // Clear all selections
        $(document).on('click', '.clear-selection-btn', () => {
            this.clearAllSelections();
        });

        // Department change updates rooms and doctors
        $('#departmentSelect').on('change', (e) => {
            this.updateRoomsAndDoctors($(e.target).val());
        });
    }

    handleFilterChange(element) {
        const $form = $(element).closest('form');
        if ($form.length > 0) {
            $form.submit();
        }
    }

    handleSlotSelection(element) {
        const $btn = $(element);
        
        if ($btn.prop('disabled')) return;
        
        const slotId = $btn.data('slot-id');
        const date = $btn.data('date');
        const period = $btn.data('period');
        
        if ($btn.hasClass('btn-success')) {
            // Deselect
            $btn.removeClass('btn-success').addClass('btn-outline-primary');
            this.removeFromSelection(slotId);
        } else {
            // Select
            $btn.removeClass('btn-outline-primary').addClass('btn-success');
            this.addToSelection({
                id: slotId,
                date: date,
                period: period,
                element: $btn
            });
        }
        
        this.updateSelectionDisplay();
    }

    handlePeriodSelection(element) {
        const $btn = $(element);
        const date = $btn.data('date');
        const period = $btn.data('period');
        const roomId = $('#roomSelect').val();
        
        if (!roomId) {
            this.showAlert('Vui lòng chọn phòng trước', 'warning');
            return;
        }

        // Mark the entire period as selected
        $btn.parent().html(`<span class="badge bg-success">Đã chọn ca ${period === 'morning' ? 'sáng' : 'chiều'}</span>`);
        
        this.addToSelection({
            roomId: roomId,
            date: date,
            period: period,
            type: 'period'
        });
        
        this.updateSelectionDisplay();
    }

    addToSelection(slot) {
        const exists = this.selectedSlots.find(s => 
            s.id === slot.id || (s.date === slot.date && s.period === slot.period)
        );
        
        if (!exists) {
            this.selectedSlots.push(slot);
        }
    }

    removeFromSelection(slotId) {
        this.selectedSlots = this.selectedSlots.filter(s => s.id !== slotId);
    }

    clearAllSelections() {
        $('.slot-btn.btn-success').removeClass('btn-success').addClass('btn-outline-primary');
        this.selectedSlots = [];
        this.updateSelectionDisplay();
    }

    updateSelectionDisplay() {
        const count = this.selectedSlots.length;
        $('.selection-count').text(count);
        $('.save-btn, .clear-btn').prop('disabled', count === 0);
    }

    async updateRoomsAndDoctors(departmentId) {
        if (!departmentId) {
            $('#roomSelect, #doctorSelect').empty().append('<option value="">-- Chọn --</option>');
            return;
        }

        try {
            // Show loading
            $('#roomSelect').html('<option value="">Đang tải...</option>');
            $('#doctorSelect').html('<option value="">Đang tải...</option>');

            // Load rooms and doctors for selected department
            const [roomsResponse, doctorsResponse] = await Promise.all([
                fetch(`/api/Room/department/${departmentId}`),
                fetch(`/api/Doctor/department/${departmentId}`)
            ]);

            if (roomsResponse.ok && doctorsResponse.ok) {
                const rooms = await roomsResponse.json();
                const doctors = await doctorsResponse.json();

                this.populateSelect('#roomSelect', rooms, 'Id', 'Name', '-- Chọn phòng --');
                this.populateSelect('#doctorSelect', doctors, 'Id', 'Name', '-- Chọn bác sĩ --');
            } else {
                throw new Error('Failed to load data');
            }
        } catch (error) {
            console.error('Error loading departments data:', error);
            this.showAlert('Không thể tải dữ liệu. Vui lòng thử lại.', 'error');
            
            // Reset selects
            $('#roomSelect, #doctorSelect').empty().append('<option value="">-- Chọn --</option>');
        }
    }

    populateSelect(selector, data, valueField, textField, placeholder) {
        const $select = $(selector);
        $select.empty();
        
        if (placeholder) {
            $select.append(`<option value="">${placeholder}</option>`);
        }
        
        data.forEach(item => {
            $select.append(`<option value="${item[valueField]}">${item[textField]}</option>`);
        });
    }

    updateFilterDependencies() {
        const selectedDept = $('#departmentSelect').val();
        
        if (selectedDept) {
            // Show/hide rooms and doctors based on department
            $('#roomSelect option, #doctorSelect option').each(function() {
                const $option = $(this);
                const itemDept = $option.data('department');
                
                if (!itemDept || itemDept === selectedDept || $option.val() === '') {
                    $option.show();
                } else {
                    $option.hide();
                }
            });
        } else {
            $('#roomSelect option, #doctorSelect option').show();
        }
    }

    showAlert(message, type = 'info') {
        const alertClass = {
            'success': 'alert-success',
            'warning': 'alert-warning',
            'error': 'alert-danger',
            'info': 'alert-info'
        }[type] || 'alert-info';

        const alertHtml = `
            <div class="alert ${alertClass} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;

        // Find the best container for the alert
        const $container = $('.alert-container').first() || $('.card-body').first() || $('body');
        $container.prepend(alertHtml);

        // Auto-dismiss after 5 seconds
        setTimeout(() => {
            $container.find('.alert').first().fadeOut();
        }, 5000);
    }

    async saveSchedule() {
        if (this.selectedSlots.length === 0) {
            this.showAlert('Vui lòng chọn ít nhất một khung giờ', 'warning');
            return;
        }

        try {
            const response = await fetch('/DoctorSchedule/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: JSON.stringify(this.selectedSlots)
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert(result.message || 'Lưu lịch thành công!', 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                throw new Error(result.message || 'Lưu lịch thất bại');
            }
        } catch (error) {
            console.error('Error saving schedule:', error);
            this.showAlert('Có lỗi xảy ra: ' + error.message, 'error');
        }
    }

    async deleteSchedule() {
        if (!confirm('Bạn có chắc chắn muốn xóa toàn bộ lịch làm việc tuần này?')) {
            return;
        }

        const roomId = $('#roomSelect').val();
        if (!roomId) {
            this.showAlert('Vui lòng chọn phòng', 'warning');
            return;
        }

        try {
            const response = await fetch('/DoctorSchedule/Delete', {
                method: 'DELETE',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val()
                },
                body: new URLSearchParams({
                    RoomId: roomId,
                    // Add date range parameters as needed
                })
            });

            const result = await response.json();

            if (result.success) {
                this.showAlert(result.message || 'Xóa lịch thành công!', 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                throw new Error(result.message || 'Xóa lịch thất bại');
            }
        } catch (error) {
            console.error('Error deleting schedule:', error);
            this.showAlert('Có lỗi xảy ra: ' + error.message, 'error');
        }
    }
}

// Initialize when document is ready
$(document).ready(function() {
    window.scheduleManager = new ScheduleManager();
    
    // Bind specific action buttons
    $('#saveScheduleBtn').on('click', () => window.scheduleManager.saveSchedule());
    $('#deleteScheduleBtn').on('click', () => window.scheduleManager.deleteSchedule());
    $('#clearSelectionBtn').on('click', () => window.scheduleManager.clearAllSelections());
});
