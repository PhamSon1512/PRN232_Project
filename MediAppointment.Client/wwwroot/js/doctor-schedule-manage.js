/**
 * Doctor Schedule Management JavaScript
 * Handles all client-side functionality for doctor schedule management
 */

class DoctorScheduleManager {
    constructor() {
        this.availableTimeSlots = [];
        this.currentDate = '';
        this.currentPeriod = '';
        this.allRoomsData = [];
        this.isUpdatingSchedule = false; // Flag to prevent race conditions
        
        // Initialize when DOM is ready
        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', () => this.init());
        } else {
            this.init();
        }
    }

    init() {
        this.loadTimeSlots();
        this.setupEventListeners();
        this.initializeRoomsData();
        this.initializeDepartmentDefaults();
        
        // Sync schedule data after a short delay to ensure everything is loaded
        setTimeout(() => {
            this.syncScheduleDataOnLoad();
        }, 100);
    }

    /**
     * Initialize rooms data from existing select options
     */
    initializeRoomsData() {
        const roomSelect = document.getElementById('roomSelect');
        if (roomSelect) {
            const initialRoomOptions = Array.from(roomSelect.querySelectorAll('option[data-department]'));
            this.allRoomsData = initialRoomOptions.map(option => ({
                id: option.value,
                name: option.textContent,
                departmentId: option.getAttribute('data-department')
            }));
            console.log('Stored initial rooms data:', this.allRoomsData);
        }
    }

    /**
     * Set up all event listeners
     */
    setupEventListeners() {
        // Department change to filter rooms
        const departmentSelect = document.getElementById('departmentSelect');
        if (departmentSelect) {
            departmentSelect.addEventListener('change', (e) => {
                const departmentId = e.target.value;
                console.log('Department changed to:', departmentId);
                
                if (departmentId) {
                    this.loadRoomsByDepartment(departmentId);
                } else {
                    const roomSelect = document.getElementById('roomSelect');
                    if (roomSelect) {
                        roomSelect.innerHTML = '<option value="">-- Chọn phòng --</option>';
                    }
                }
            });
        }

        // Quick week navigation
        const quickWeekBtn = document.getElementById('quickWeekBtn');
        if (quickWeekBtn) {
            quickWeekBtn.addEventListener('click', () => {
                const now = new Date();
                const currentYear = now.getFullYear();
                const currentWeek = this.getWeekOfYear(now);
                
                document.getElementById('yearSelect').value = currentYear;
                document.getElementById('weekSelect').value = currentWeek;
                
                if (document.getElementById('departmentSelect').value && document.getElementById('roomSelect').value) {
                    document.getElementById('filterForm').submit();
                }
            });
        }

        // Bulk register button
        const bulkRegisterBtn = document.getElementById('bulkRegisterBtn');
        if (bulkRegisterBtn) {
            bulkRegisterBtn.addEventListener('click', () => {
                console.log('Bulk register button clicked');
                new bootstrap.Modal(document.getElementById('bulkRegisterModal')).show();
            });
        }

        // Bulk cancel button
        const bulkCancelBtn = document.getElementById('bulkCancelBtn');
        if (bulkCancelBtn) {
            bulkCancelBtn.addEventListener('click', () => {
                console.log('Bulk cancel button clicked');
                this.loadRegisteredSchedules();
                new bootstrap.Modal(document.getElementById('bulkCancelModal')).show();
            });
        }

        // Bulk confirm buttons
        const confirmBulkRegister = document.getElementById('confirmBulkRegister');
        if (confirmBulkRegister) {
            confirmBulkRegister.addEventListener('click', () => {
                console.log('Confirm bulk register clicked');
                this.bulkRegisterSchedules();
            });
        }

        const confirmBulkCancel = document.getElementById('confirmBulkCancel');
        if (confirmBulkCancel) {
            confirmBulkCancel.addEventListener('click', () => {
                console.log('Confirm bulk cancel clicked');
                this.bulkCancelSchedules();
            });
        }

        // Add schedule button click (event delegation)
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('add-schedule-btn') || e.target.closest('.add-schedule-btn')) {
                console.log('Add schedule button clicked');
                const btn = e.target.classList.contains('add-schedule-btn') ? e.target : e.target.closest('.add-schedule-btn');
                this.currentDate = btn.getAttribute('data-date');
                this.currentPeriod = btn.getAttribute('data-period');
                
                console.log('Date:', this.currentDate, 'Period:', this.currentPeriod);
                
                // Directly register for the entire shift without showing modal
                if (confirm(`Bạn có muốn đăng ký ${this.currentPeriod === 'morning' ? 'ca sáng' : 'ca chiều'} ngày ${new Date(this.currentDate).toLocaleDateString('vi-VN')}?`)) {
                    this.registerWholeShift(this.currentDate, this.currentPeriod);
                }
            }
        });

        // Remove schedule button click (event delegation)
        document.addEventListener('click', (e) => {
            if (e.target.classList.contains('remove-schedule-btn') || e.target.closest('.remove-schedule-btn')) {
                console.log('Remove schedule button clicked');
                e.preventDefault();
                const btn = e.target.classList.contains('remove-schedule-btn') ? e.target : e.target.closest('.remove-schedule-btn');
                const date = btn.getAttribute('data-date');
                const period = btn.getAttribute('data-period');
                
                console.log('Remove date:', date, 'Period:', period);
                
                if (confirm('Bạn có chắc chắn muốn hủy lịch làm việc này?')) {
                    this.removeSchedule(date, period);
                }
            }
        });

        // Confirm time slots button
        const confirmTimeSlots = document.getElementById('confirmTimeSlots');
        if (confirmTimeSlots) {
            confirmTimeSlots.addEventListener('click', () => {
                console.log('Confirm time slots clicked');
                const selectedTimeSlots = Array.from(document.querySelectorAll('.time-slot-checkbox:checked')).map(cb => cb.value);
                
                if (selectedTimeSlots.length === 0) {
                    this.showAlert('Vui lòng chọn ít nhất một khung giờ', 'warning');
                    return;
                }
                
                this.createSchedule(this.currentDate, this.currentPeriod, selectedTimeSlots);
                bootstrap.Modal.getInstance(document.getElementById('timeSlotModal')).hide();
            });
        }
        
        console.log('All event listeners set up successfully');
    }

    /**
     * Initialize department defaults
     */
    initializeDepartmentDefaults() {
        const departmentSelect = document.getElementById('departmentSelect');
        let currentDeptId = departmentSelect.value;
        
        // Set first department as default if no department is selected
        if (!currentDeptId) {
            const firstDepartmentOption = departmentSelect.querySelector('option[value]:not([value=""])');
            if (firstDepartmentOption) {
                currentDeptId = firstDepartmentOption.value;
                departmentSelect.value = currentDeptId;
                console.log('Auto-selected first department:', firstDepartmentOption.textContent);
                
                // Trigger change event to notify other parts of the application
                departmentSelect.dispatchEvent(new Event('change'));
            }
        }
        
        // Load rooms for the selected department
        if (currentDeptId) {
            this.loadRoomsByDepartment(currentDeptId);
        }
    }

    /**
     * Load available time slots from server
     */
    loadTimeSlots() {
        fetch('/DoctorSchedule/GetTimeSlots')
            .then(response => {
                console.log('GetTimeSlots response status:', response.status);
                return response.json();
            })
            .then(data => {
                console.log('GetTimeSlots response data:', data);
                if (data.success) {
                    this.availableTimeSlots = data.data;
                    console.log('Loaded time slots:', this.availableTimeSlots);
                    
                    // Now that time slots are loaded, sync the schedule data
                    this.syncScheduleDataOnLoad();
                } else {
                    console.error('Failed to load time slots:', data.message);
                    this.showAlert('Không thể tải danh sách khung giờ: ' + data.message, 'warning');
                }
            })
            .catch(error => {
                console.error('Error loading time slots:', error);
                this.showAlert('Lỗi khi tải danh sách khung giờ', 'danger');
            });
    }

    /**
     * Load rooms by department
     */
    loadRoomsByDepartment(departmentId) {
        console.log('loadRoomsByDepartment called with:', departmentId);
        
        const roomSelect = document.getElementById('roomSelect');
        
        // Clear current options
        roomSelect.innerHTML = '<option value="">-- Chọn phòng --</option>';
        
        // Filter rooms from stored data
        const filteredRooms = this.allRoomsData.filter(room => room.departmentId === departmentId);
        console.log(`Found ${filteredRooms.length} rooms for department ${departmentId} from stored data`);
        
        if (filteredRooms.length > 0) {
            // Add filtered rooms to select
            filteredRooms.forEach(room => {
                const option = document.createElement('option');
                option.value = room.id;
                option.textContent = room.name;
                option.setAttribute('data-department', room.departmentId);
                roomSelect.appendChild(option);
            });
            
            // Set first room as default if no room is currently selected
            if (!roomSelect.value && filteredRooms.length > 0) {
                roomSelect.value = filteredRooms[0].id;
                console.log('Auto-selected first room:', filteredRooms[0].name);
                
                // Trigger change event to notify other parts of the application
                roomSelect.dispatchEvent(new Event('change'));
            }
        } else {
            // If no rooms found in stored data, try API call
            console.log('No rooms found in stored data, trying API call...');
            fetch(`/DoctorSchedule/GetRoomsByDepartment?departmentId=${departmentId}`)
                .then(response => {
                    console.log('API response status:', response.status);
                    return response.json();
                })
                .then(data => {
                    console.log('API response data:', data);
                    
                    if (data.success && data.data) {
                        data.data.forEach(room => {
                            const option = document.createElement('option');
                            option.value = room.id;
                            option.textContent = room.name;
                            option.setAttribute('data-department', room.departmentId);
                            roomSelect.appendChild(option);
                            
                            // Also add to stored data
                            this.allRoomsData.push({
                                id: room.id,
                                name: room.name,
                                departmentId: room.departmentId
                            });
                        });
                        console.log(`Loaded ${data.data.length} rooms from API for department ${departmentId}`);
                        
                        // Set first room as default if no room is currently selected
                        if (!roomSelect.value && data.data.length > 0) {
                            roomSelect.value = data.data[0].id;
                            console.log('Auto-selected first room from API:', data.data[0].name);
                            
                            // Trigger change event to notify other parts of the application
                            roomSelect.dispatchEvent(new Event('change'));
                        }
                    } else {
                        console.error('Failed to load rooms from API:', data.message);
                        this.showAlert('Không thể tải danh sách phòng: ' + (data.message || 'Lỗi không xác định'), 'warning');
                    }
                })
                .catch(error => {
                    console.error('Error loading rooms from API:', error);
                    this.showAlert('Không thể tải danh sách phòng', 'danger');
                });
        }
    }

    /**
     * Sync schedule data on page load
     */
    syncScheduleDataOnLoad() {
        // Skip if currently updating schedule to avoid race conditions
        if (this.isUpdatingSchedule) {
            console.log('Skipping schedule sync - update in progress');
            return;
        }
        
        // Only sync if we have the required parameters
        const roomId = document.getElementById('roomSelect')?.value;
        const year = document.getElementById('yearSelect')?.value;
        const week = document.getElementById('weekSelect')?.value;
        
        if (!roomId || !year || !week) {
            console.log('Missing parameters for schedule sync, skipping...');
            return;
        }
        
        console.log('Syncing schedule data on load...', { roomId, year, week });
        
        fetch(`/DoctorSchedule/GetWeeklyScheduleDetails?roomId=${roomId}&year=${year}&week=${week}`)
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Schedule sync response:', data);
                
                if (data.success && data.data) {
                    // Update UI for each date and period
                    Object.keys(data.data).forEach(dateKey => {
                        const dayData = data.data[dateKey];
                        
                        // Check morning shift
                        if (dayData.morning && dayData.morning.length > 0) {
                            this.updateScheduleCell(dateKey, 'morning', true, []);
                        } else {
                            this.updateScheduleCell(dateKey, 'morning', false);
                        }
                        
                        // Check afternoon shift
                        if (dayData.afternoon && dayData.afternoon.length > 0) {
                            this.updateScheduleCell(dateKey, 'afternoon', true, []);
                        } else {
                            this.updateScheduleCell(dateKey, 'afternoon', false);
                        }
                    });
                    
                    console.log('Schedule data synchronized successfully');
                } else {
                    console.log('No schedule data found or API error:', data.message);
                }
            })
            .catch(error => {
                console.error('Error syncing schedule data:', error);
            });
    }

    /**
     * Load time slot options for modal
     */
    loadTimeSlotOptions() {
        const container = document.getElementById('timeSlotContainer');
        container.innerHTML = '';
        
        const periodSlots = this.availableTimeSlots.filter(slot => slot.period === this.currentPeriod);
        
        periodSlots.forEach(slot => {
            const colDiv = document.createElement('div');
            colDiv.className = 'col-md-6 time-slot-option';
            
            colDiv.innerHTML = `
                <div class="form-check">
                    <input class="form-check-input time-slot-checkbox" type="checkbox" value="${slot.id}" id="slot_${slot.id}">
                    <label class="form-check-label w-100 p-2 border rounded" for="slot_${slot.id}">
                        ${slot.timeRange}
                    </label>
                </div>
            `;
            
            container.appendChild(colDiv);
        });
    }

    /**
     * Load registered schedules for bulk cancel modal
     */
    loadRegisteredSchedules() {
        const roomId = document.getElementById('roomSelect').value;
        const year = document.getElementById('yearSelect').value;
        const week = document.getElementById('weekSelect').value;
        
        fetch(`/DoctorSchedule/GetWeeklyScheduleDetails?roomId=${roomId}&year=${year}&week=${week}`)
            .then(response => response.json())
            .then(data => {
                const container = document.getElementById('registeredSchedulesList');
                container.innerHTML = '';
                
                if (data.success && data.data) {
                    Object.keys(data.data).forEach(dateKey => {
                        const dayData = data.data[dateKey];
                        const hasSchedules = dayData.morning.length > 0 || dayData.afternoon.length > 0;
                        
                        if (hasSchedules) {
                            const dayDiv = document.createElement('div');
                            dayDiv.className = 'mb-2';
                            dayDiv.innerHTML = `
                                <div class="row">
                                    <div class="col-md-3">
                                        <strong>${dayData.dayOfWeek} ${dayData.date}</strong>
                                    </div>
                                    <div class="col-md-9">
                                        ${dayData.morning.length > 0 ? `
                                            <div class="form-check form-check-inline">
                                                <input class="form-check-input bulk-cancel-check" type="checkbox" 
                                                       data-date="${dateKey}" data-period="morning" 
                                                       id="cancel_${dateKey.replace(/-/g, '')}_morning">
                                                <label class="form-check-label" for="cancel_${dateKey.replace(/-/g, '')}_morning">
                                                    Ca sáng (${dayData.morning.length} slot)
                                                </label>
                                            </div>
                                        ` : ''}
                                        ${dayData.afternoon.length > 0 ? `
                                            <div class="form-check form-check-inline">
                                                <input class="form-check-input bulk-cancel-check" type="checkbox" 
                                                       data-date="${dateKey}" data-period="afternoon" 
                                                       id="cancel_${dateKey.replace(/-/g, '')}_afternoon">
                                                <label class="form-check-label" for="cancel_${dateKey.replace(/-/g, '')}_afternoon">
                                                    Ca chiều (${dayData.afternoon.length} slot)
                                                </label>
                                            </div>
                                        ` : ''}
                                    </div>
                                </div>
                            `;
                            container.appendChild(dayDiv);
                        }
                    });
                    
                    if (container.children.length === 0) {
                        container.innerHTML = '<p class="text-muted">Không có ca làm việc nào để hủy.</p>';
                    }
                } else {
                    container.innerHTML = '<p class="text-muted">Không thể tải danh sách ca làm việc.</p>';
                }
            })
            .catch(error => {
                console.error('Error loading registered schedules:', error);
            });
    }

    /**
     * Bulk register schedules
     */
    bulkRegisterSchedules() {
        try {
            const selectedSchedules = Array.from(document.querySelectorAll('.bulk-schedule-check:checked'));
            const roomId = document.getElementById('roomSelect').value;
            
            if (selectedSchedules.length === 0) {
                this.showAlert('Vui lòng chọn ít nhất một ca để đăng ký', 'warning');
                return;
            }

            if (!roomId) {
                this.showAlert('Vui lòng chọn phòng trước khi đăng ký', 'warning');
                return;
            }

            // Check if time slots are available
            if (!this.availableTimeSlots || this.availableTimeSlots.length === 0) {
                this.showAlert('Đang tải danh sách khung giờ. Vui lòng đợi một chút và thử lại!', 'warning');
                // Try to reload time slots
                this.loadTimeSlots();
                return;
            }

            const schedules = [];
            let hasError = false;
            
            for (const checkbox of selectedSchedules) {
                const dateStr = checkbox.getAttribute('data-date');
                const period = checkbox.getAttribute('data-period');
                const timeSlotIds = this.getDefaultTimeSlotIds(period);
                
                if (!timeSlotIds || timeSlotIds.length === 0) {
                    this.showAlert(`Không tìm thấy khung giờ cho ca ${period}`, 'danger');
                    hasError = true;
                    break;
                }

                schedules.push({
                    roomId: roomId,
                    date: dateStr + 'T00:00:00', // Add time part for proper DateTime parsing
                    period: period,
                    timeSlotIds: timeSlotIds
                });
            }
            
            if (hasError) {
                return;
            }

            console.log('Bulk register data to send:', { schedules: schedules });

            const headers = {
                'Content-Type': 'application/json'
            };

            fetch('/DoctorSchedule/BulkCreateSchedule', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify({ schedules: schedules })
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                console.log('Bulk register response:', data);
                
                if (data.successCount > 0) {
                    this.showAlert(`Đăng ký thành công ${data.successCount} ca`, 'success');
                    
                    // Update UI for successful registrations
                    if (data.details) {
                        data.details.forEach(detail => {
                            if (detail.success) {
                                // Convert date from dd/MM/yyyy to yyyy-MM-dd format
                                const dateParts = detail.date.split('/');
                                const formattedDate = `${dateParts[2]}-${dateParts[1].padStart(2, '0')}-${dateParts[0].padStart(2, '0')}`;
                                
                                const timeSlots = this.getDefaultTimeSlotsByPeriod(detail.period);
                                this.updateScheduleCell(formattedDate, detail.period, true, timeSlots);
                            }
                        });
                    } else {
                        // If no details, update UI for all selected schedules
                        const selectedSchedules = Array.from(document.querySelectorAll('.bulk-schedule-check:checked'));
                        selectedSchedules.forEach(checkbox => {
                            const date = checkbox.getAttribute('data-date');
                            const period = checkbox.getAttribute('data-period');
                            const timeSlots = this.getDefaultTimeSlotsByPeriod(period);
                            this.updateScheduleCell(date, period, true, timeSlots);
                        });
                    }
                }
                if (data.errorCount > 0) {
                    this.showAlert(`${data.errorCount} ca không thể đăng ký`, 'warning');
                    if (data.details) {
                        console.log('Failed schedules:', data.details.filter(d => !d.success));
                    }
                }
                bootstrap.Modal.getInstance(document.getElementById('bulkRegisterModal')).hide();
            })
            .catch(error => {
                console.error('Error bulk registering:', error);
                this.showAlert('Có lỗi xảy ra khi đăng ký: ' + error.message, 'danger');
            });
        } catch (error) {
            console.error('Unexpected error in bulkRegisterSchedules:', error);
            this.showAlert('Có lỗi không mong muốn xảy ra: ' + error.message, 'danger');
        }
    }

    /**
     * Bulk cancel schedules
     */
    bulkCancelSchedules() {
        const selectedSchedules = Array.from(document.querySelectorAll('.bulk-cancel-check:checked'));
        const roomId = document.getElementById('roomSelect').value;
        
        if (selectedSchedules.length === 0) {
            this.showAlert('Vui lòng chọn ít nhất một ca để hủy', 'warning');
            return;
        }

        if (!roomId) {
            this.showAlert('Vui lòng chọn phòng trước khi hủy lịch', 'warning');
            return;
        }

        const schedules = selectedSchedules.map(checkbox => ({
            roomId: roomId,
            date: checkbox.getAttribute('data-date') + 'T00:00:00', // Add time part for proper DateTime parsing
            period: checkbox.getAttribute('data-period')
        }));

        const headers = {
            'Content-Type': 'application/json'
        };

        fetch('/DoctorSchedule/BulkDeleteSchedule', {
            method: 'POST',
            headers: headers,
            body: JSON.stringify({ schedules: schedules })
        })
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            console.log('Bulk cancel response:', data);
            
            if (data.successCount > 0) {
                this.showAlert(`Hủy thành công ${data.successCount} ca`, 'success');
                
                // Update UI for successful cancellations
                if (data.details) {
                    data.details.forEach(detail => {
                        if (detail.success) {
                            // Convert date from dd/MM/yyyy to yyyy-MM-dd format
                            const dateParts = detail.date.split('/');
                            const formattedDate = `${dateParts[2]}-${dateParts[1].padStart(2, '0')}-${dateParts[0].padStart(2, '0')}`;
                            
                            this.updateScheduleCell(formattedDate, detail.period, false);
                        }
                    });
                } else {
                    // If no details, update UI for all selected schedules
                    const selectedSchedules = Array.from(document.querySelectorAll('.bulk-cancel-check:checked'));
                    selectedSchedules.forEach(checkbox => {
                        const date = checkbox.getAttribute('data-date');
                        const period = checkbox.getAttribute('data-period');
                        this.updateScheduleCell(date, period, false);
                    });
                }
            }
            if (data.errorCount > 0) {
                this.showAlert(`${data.errorCount} ca không thể hủy`, 'warning');
                if (data.details) {
                    console.log('Failed cancellations:', data.details.filter(d => !d.success));
                }
            }
            bootstrap.Modal.getInstance(document.getElementById('bulkCancelModal')).hide();
        })
        .catch(error => {
            console.error('Error bulk canceling:', error);
            this.showAlert('Có lỗi xảy ra khi hủy lịch: ' + error.message, 'danger');
        });
    }

    /**
     * Register whole shift
     */
    registerWholeShift(date, period) {
        try {
            const roomId = document.getElementById('roomSelect').value;
            
            if (!roomId) {
                this.showAlert('Vui lòng chọn phòng trước khi đăng ký', 'warning');
                return;
            }

            // Set flag to prevent race conditions
            this.isUpdatingSchedule = true;

            // Get all time slots for the period
            const timeSlotIds = this.getDefaultTimeSlotIds(period);
            
            if (!timeSlotIds || timeSlotIds.length === 0) {
                this.showAlert('Không tìm thấy khung giờ cho ca này', 'danger');
                this.isUpdatingSchedule = false;
                return;
            }
            
            const requests = [{
                roomId: roomId,
                date: date + 'T00:00:00',
                period: period,
                timeSlotIds: timeSlotIds
            }];
            
            const headers = {
                'Content-Type': 'application/json'
            };
            
            fetch('/DoctorSchedule/CreateSchedule', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(requests)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    this.showAlert(`Đăng ký ${period === 'morning' ? 'ca sáng' : 'ca chiều'} thành công!`, 'success');
                    
                    // Update UI immediately
                    const timeSlots = this.getDefaultTimeSlotsByPeriod(period);
                    this.updateScheduleCell(date, period, true, timeSlots);
                } else {
                    this.showAlert(data.message || 'Không thể đăng ký lịch làm việc', 'danger');
                }
            })
            .catch(error => {
                console.error('Error registering shift:', error);
                this.showAlert('Có lỗi xảy ra khi đăng ký ca làm việc: ' + error.message, 'danger');
            })
            .finally(() => {
                this.isUpdatingSchedule = false;
            });
        } catch (error) {
            console.error('Unexpected error in registerWholeShift:', error);
            this.showAlert('Có lỗi không mong muốn xảy ra: ' + error.message, 'danger');
            this.isUpdatingSchedule = false;
        }
    }

    /**
     * Create schedule
     */
    createSchedule(date, period, timeSlotIds) {
        try {
            const roomId = document.getElementById('roomSelect').value;
            
            if (!roomId) {
                this.showAlert('Vui lòng chọn phòng trước khi đăng ký', 'warning');
                return;
            }

            if (!timeSlotIds || timeSlotIds.length === 0) {
                this.showAlert('Vui lòng chọn khung giờ', 'warning');
                return;
            }
            
            const requests = [{
                roomId: roomId,
                date: date + 'T00:00:00', // Add time part for proper DateTime parsing
                period: period,
                timeSlotIds: timeSlotIds
            }];
            
            const headers = {
                'Content-Type': 'application/json'
            };
            
            fetch('/DoctorSchedule/CreateSchedule', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(requests)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    this.showAlert(data.message, 'success');
                    
                    // Update UI immediately
                    const timeSlots = this.getDefaultTimeSlotsByPeriod(period);
                    this.updateScheduleCell(date, period, true, timeSlots);
                } else {
                    this.showAlert(data.message, 'danger');
                }
            })
            .catch(error => {
                console.error('Error creating schedule:', error);
                this.showAlert('Có lỗi xảy ra khi tạo lịch: ' + error.message, 'danger');
            });
        } catch (error) {
            console.error('Unexpected error in createSchedule:', error);
            this.showAlert('Có lỗi không mong muốn xảy ra: ' + error.message, 'danger');
        }
    }

    /**
     * Remove schedule
     */
    removeSchedule(date, period) {
        try {
            const roomId = document.getElementById('roomSelect').value;
            
            if (!roomId) {
                this.showAlert('Vui lòng chọn phòng trước khi hủy lịch', 'warning');
                return;
            }
            
            // Set flag to prevent race conditions
            this.isUpdatingSchedule = true;
            
            // Show loading state
            const cell = document.querySelector(`[data-date="${date}"][data-period="${period}"]`);
            if (cell) {
                cell.style.opacity = '0.5';
                cell.style.pointerEvents = 'none';
            }
            
            const request = {
                roomId: roomId,
                date: date + 'T00:00:00', // Add time part for proper DateTime parsing
                period: period
            };
            
            const headers = {
                'Content-Type': 'application/json'
            };
            
            fetch('/DoctorSchedule/DeleteSchedule', {
                method: 'POST',
                headers: headers,
                body: JSON.stringify(request)
            })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (data.success) {
                    this.showAlert(data.message, 'success');
                    
                    // Update UI immediately to show "Not registered" state
                    this.updateScheduleCell(date, period, false);
                    
                    console.log(`Schedule removed and UI updated for ${date}, ${period}`);
                } else {
                    this.showAlert(data.message, 'danger');
                }
            })
            .catch(error => {
                console.error('Error removing schedule:', error);
                this.showAlert('Có lỗi xảy ra khi hủy lịch: ' + error.message, 'danger');
            })
            .finally(() => {
                // Remove loading state and flag
                if (cell) {
                    cell.style.opacity = '1';
                    cell.style.pointerEvents = 'auto';
                }
                this.isUpdatingSchedule = false;
            });
        } catch (error) {
            console.error('Unexpected error in removeSchedule:', error);
            this.showAlert('Có lỗi không mong muốn xảy ra: ' + error.message, 'danger');
            this.isUpdatingSchedule = false;
        }
    }

    /**
     * Helper functions
     */
    getDefaultTimeSlotIds(period) {
        try {
            if (!this.availableTimeSlots || this.availableTimeSlots.length === 0) {
                console.error('Available time slots not loaded');
                this.showAlert('Chưa tải được danh sách khung giờ. Vui lòng tải lại trang!', 'warning');
                return [];
            }
            
            console.log('All available time slots:', this.availableTimeSlots);
            const filteredSlots = this.availableTimeSlots.filter(slot => slot.period === period);
            console.log(`Found ${filteredSlots.length} time slots for period ${period}`);
            
            if (filteredSlots.length === 0) {
                console.error(`No time slots found for period: ${period}`);
                this.showAlert(`Không tìm thấy khung giờ cho ca ${period === 'morning' ? 'sáng' : 'chiều'}`, 'danger');
                return [];
            }
            
            return filteredSlots.map(slot => slot.id);
        } catch (error) {
            console.error('Error in getDefaultTimeSlotIds:', error);
            return [];
        }
    }

    getDefaultTimeSlotsByPeriod(period) {
        if (!this.availableTimeSlots || this.availableTimeSlots.length === 0) {
            return [];
        }
        
        return this.availableTimeSlots.filter(slot => slot.period === period).map(slot => ({
            id: slot.id,
            timeRange: slot.timeRange
        }));
    }

    getWeekOfYear(date) {
        const start = new Date(date.getFullYear(), 0, 1);
        const days = Math.floor((date - start) / (24 * 60 * 60 * 1000)) + 1;
        return Math.ceil(days / 7);
    }

    /**
     * UI Helper methods
     */
    createRegisteredSlotHTML(period, timeSlots) {
        const periodText = period === 'morning' ? 'sáng' : 'chiều';
        
        return `
            <div class="registered-slots">
                <div class="text-center mb-3">
                    <span class="badge bg-success" style="font-size: 1rem; padding: 8px 12px;">
                        <i class="bi bi-check-circle-fill"></i> Đã đăng ký
                    </span>
                </div>
                <button type="button" 
                        class="btn btn-sm btn-outline-danger remove-schedule-btn w-100"
                        data-date="" 
                        data-period="${period}">
                    <i class="bi bi-trash"></i> Hủy đăng ký
                </button>
            </div>
        `;
    }

    createEmptySlotHTML(period) {
        const periodText = period === 'morning' ? 'sáng' : 'chiều';
        return `
            <div class="empty-slot">
                <div class="text-center mb-2">
                    <i class="bi bi-calendar-plus text-muted" style="font-size: 1.5rem;"></i>
                    <div class="text-muted small">Chưa đăng ký</div>
                </div>
                <button type="button" 
                        class="btn btn-sm btn-success add-schedule-btn w-100"
                        data-date="" 
                        data-period="${period}">
                    <i class="bi bi-plus-circle"></i> Đăng ký ca ${periodText}
                </button>
            </div>
        `;
    }

    updateScheduleCell(date, period, isRegistered, timeSlots = []) {
        const cell = document.querySelector(`[data-date="${date}"][data-period="${period}"]`);
        if (!cell) {
            console.warn('Cell not found for date:', date, 'period:', period, '- might not be in current view');
            return;
        }

        console.log(`Updating schedule cell for date: ${date}, period: ${period}, isRegistered: ${isRegistered}`);

        // Create the appropriate HTML content
        let newContent = '';
        if (isRegistered) {
            newContent = this.createRegisteredSlotHTML(period, timeSlots);
        } else {
            newContent = this.createEmptySlotHTML(period);
        }
        
        // Update the cell content
        cell.innerHTML = newContent;
        
        // Update data attributes for buttons
        const buttons = cell.querySelectorAll('button');
        buttons.forEach(btn => {
            btn.setAttribute('data-date', date);
            btn.setAttribute('data-period', period);
        });
        
        // Add visual feedback for successful update
        cell.style.transition = 'background-color 0.3s ease';
        cell.style.backgroundColor = isRegistered ? '#d4edda' : '#f8f9fa';
        setTimeout(() => {
            cell.style.backgroundColor = '';
        }, 1000);
        
        console.log(`Successfully updated schedule cell for date: ${date}, period: ${period}, state: ${isRegistered ? 'registered' : 'not registered'}`);
    }

    showAlert(message, type) {
        const alertContainer = document.getElementById('alertContainer');
        const alertId = 'alert_' + Date.now();
        
        const alertHtml = `
            <div id="${alertId}" class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `;
        
        alertContainer.innerHTML = alertHtml;
        
        setTimeout(() => {
            const alertElement = document.getElementById(alertId);
            if (alertElement) {
                bootstrap.Alert.getInstance(alertElement)?.close();
            }
        }, 5000);
    }

    validateScheduleConflict(date, period, callback) {
        const roomId = document.getElementById('roomSelect').value;
        
        if (!roomId) {
            this.showAlert('Vui lòng chọn phòng trước', 'warning');
            return;
        }

        fetch(`/DoctorSchedule/ValidateScheduleConflict?roomId=${roomId}&date=${date}&period=${period}`)
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    if (data.hasConflict) {
                        this.showAlert('Đã có lịch làm việc cho ca này. Vui lòng chọn ca khác.', 'warning');
                    } else {
                        callback();
                    }
                } else {
                    this.showAlert('Không thể kiểm tra lịch trùng lặp', 'danger');
                }
            })
            .catch(error => {
                console.error('Error validating conflict:', error);
                // Still allow to proceed if validation fails
                callback();
            });
    }
}

// Initialize the manager when script loads
const scheduleManager = new DoctorScheduleManager();
