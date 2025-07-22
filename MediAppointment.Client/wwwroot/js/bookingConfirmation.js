document.addEventListener("DOMContentLoaded", function () {
    const submitBtn = document.getElementById("submitBtn");
    const modalElement = document.getElementById("confirmBookingModal");
    const confirmModalYes = document.getElementById("confirmBookingSubmit");

    // Khởi tạo toast container
    const toastContainer = document.createElement("div");
    toastContainer.setAttribute("class", "toast-container position-fixed top-0 end-0 p-3");
    document.body.appendChild(toastContainer);

    if (!submitBtn || !modalElement || !confirmModalYes) {
        console.log("One or more elements not found");
        return;
    }

    submitBtn.addEventListener("click", function () {
        console.log("Button clicked");
        // Truyền dữ liệu từ form vào modal nếu cần (optional)
        const departmentText = document.getElementById("departmentSelect")?.selectedOptions[0]?.textContent.trim();
        const appointmentDate = document.getElementById("appointmentDate")?.value;
        const timeSlotBtn = document.querySelector(".time-slot-btn.btn-success");
        const timeRange = timeSlotBtn?.dataset.timeRange;
        const note = document.getElementById("Note")?.value;

        document.querySelector("#confirmBookingModal dd:nth-of-type(1)").textContent = departmentText || "";
        document.querySelector("#confirmBookingModal dd:nth-of-type(2)").textContent = formatDate(appointmentDate);
        document.querySelector("#confirmBookingModal dd:nth-of-type(3)").textContent = timeRange || "";
        document.querySelector("#confirmBookingModal dd:nth-of-type(4)").textContent = note || "";

        // Mở modal
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    });

    confirmModalYes.addEventListener("click", async function () { // Thêm async
        const depositAmount = 200000;
        
        try {
            const response = await fetch('/Ewallet/check-balance-payment', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ amount: depositAmount })
            });

            if (!response.ok) {
                throw new Error(`Lỗi HTTP: ${response.status} - ${response.statusText}`);
            }

            const data = await response.json();

            if (data.success) {
                alert(data.message);
                document.getElementById("bookingForm").submit();
            } else {
                const toastElement = document.createElement("div");
                toastElement.setAttribute("class", "toast align-items-center text-white bg-danger border-0");
                toastElement.setAttribute("role", "alert");
                toastElement.setAttribute("aria-live", "assertive");
                toastElement.setAttribute("aria-atomic", "true");
                toastElement.innerHTML = `
                    <div class="d-flex">
                        <div class="toast-body">
                            Số dư không đủ! <a href="/EWallet" class="text-white text-decoration-underline">Nạp tiền ngay</a>
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                `;
                toastContainer.appendChild(toastElement);

                const toast = new bootstrap.Toast(toastElement);
                toast.show();

                // Xóa toast sau khi ẩn
                toastElement.addEventListener('hidden.bs.toast', function () {
                    toastContainer.removeChild(toastElement);
                });
            }
        } catch (error) {
            alert(`Lỗi: ${error.message}`);
        }
    });

    function formatDate(dateStr) {
        if (!dateStr) return "";
        const [yyyy, mm, dd] = dateStr.split("-");
        return `${dd}/${mm}/${yyyy}`;
    }
});