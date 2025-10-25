let isLoggedIn = false;
let isFileLoaded = false;

// Button references
const btnImportExcel = document.getElementById('btnImportExcel');
const btnPlaceOrder = document.getElementById('btnPlaceOrder');
const btnCancelAllOrders = document.getElementById('btnCancelAllOrders');
const txtExcelFileName = document.getElementById('txtExcelFileName');
const fileInput = document.getElementById('fileInput');
const excelDataDiv = document.getElementById('excelData');
const btnLogin = document.getElementById('btnLogin');
const loginDropdown = document.getElementById('loginDropdown');
const btnEnterToken = document.getElementById('btnEnterToken');
const txtRequestToken = document.getElementById('txtRequestToken');
const btnGenerateToken = document.getElementById('btnGenerateToken');

// ----- Restore saved state on page load -----
window.addEventListener('DOMContentLoaded', () => {
    //localStorage.clear();
    const savedTable = localStorage.getItem('excelTableHTML');
    const savedFileName = localStorage.getItem('excelFileName');

    if (savedTable) {
        excelDataDiv.innerHTML = savedTable;
        isFileLoaded = true;
    }
    if (savedFileName) {
        txtExcelFileName.textContent = savedFileName;
    }

    btnEnterToken.disabled = true;
    btnCancelAllOrders.disabled = true;
    btnPlaceOrder.disabled = true;
    refreshStatus();
});

// ----- Import Excel -----
btnImportExcel.addEventListener('click', () => fileInput.click());

btnCancelAllOrders.addEventListener('click', () => {
     CancelOrders();
});

async function CancelOrders() {
    try {
         const response = await fetch('https://altra-bc21.onrender.com/api/Altra/CancelOrder', {
            method: 'GET'
        });
        if (!response.ok) {
            btnLogin.textContent = '❌ Error';
            btnLogin.disabled = true;
            return;
        }

        const data = await response.json();
        alert(data.message);
    }
    catch{
        alert("Server is down");
    }
}

btnLogin.addEventListener('click', () => {
    if (!isLoggedIn) {
        loginDropdown.parentElement.classList.toggle('show');
    }
    else {
        isLoggedIn = false;
        localStorage.clear();
    }
});

// Generate token action
btnGenerateToken.addEventListener('click', () => {
    GetLoginURL();
});
btnEnterToken.addEventListener('click', () => {
    Login();
});

async function GetLoginURL() {
    try {
        const response = await fetch('https://altra-bc21.onrender.com/api/Altra/GetLoginURL', {
            method: 'GET'
        });
        if (!response.ok) {
            btnLogin.textContent = '❌ Error';
            btnLogin.disabled = true;
            return;
        }

        const data = await response.json();
        const loginURL = data.message;
        const newTab = window.open('about:blank', '_blank');
        newTab.location.href = loginURL;
        btnEnterToken.disabled = false;
        console.log(loginURL);
    } catch (error) {
        alert("Server is down");
        console.error('Error refreshing:', error);
    }
}

async function refreshStatus() {
    try {
        const accessToken = localStorage.getItem('accessToken');
        if (accessToken == null) {
            isLoggedIn = false;
            btnLogin.textContent = '🔑 Login ▼';
            return;
        }
        const response = await fetch(`https://altra-bc21.onrender.com/api/Altra/Refresh?accessToken=${accessToken}`, {
            method: 'GET'
        });

        if (!response.ok) {
            return;
        }

        const data = await response.json();
        const msg = data.message;
        if (msg === "LoggedIn") {
            isLoggedIn = true;
            btnLogin.textContent = '🚪Logout';
            btnCancelAllOrders.disabled = false;
            if (isFileLoaded) {
                btnPlaceOrder.disabled = false;
            }
        }
        else {
            isLoggedIn = false;
            btnLogin.textContent = '🔑 Login ▼';
        }
        console.log(msg);
    } catch (error) {
        console.error('Error refreshing:', error);
    }
}

async function Login() {
    try {
        const requestToken = txtRequestToken.value;
        loginDropdown.parentElement.classList.remove('show');

        const response = await fetch(`https://altra-bc21.onrender.com/api/Altra/Login?requestToken=${requestToken}`, {
            method: 'GET'
        });

        if (!response.ok) {
            btnLogin.textContent = '❌ Error';
            btnLogin.disabled = true;
            return;
        }

        const data = await response.json();
        const accessToken = data.message;
        if (accessToken !== "") {
            isLoggedIn = true;
            localStorage.setItem('accessToken', accessToken);
            btnLogin.textContent = '🚪Logout';
            btnCancelAllOrders.disabled = false;
              if (isFileLoaded) {
                btnPlaceOrder.disabled = false;
            }
        }
        else {
            isLoggedIn = false;
            btnLogin.textContent = '🔑 Login ▼';
        }
    } catch (error) {
        alert("Server is down");
        console.error('Error refreshing:', error);
    }
}

fileInput.addEventListener('change', (e) => {
    const file = e.target.files[0];
    if (!file) return;

    txtExcelFileName.textContent = file.name;
    btnPlaceOrder.disabled = !isLoggedIn;

    const reader = new FileReader();
    reader.onload = (event) => {
        const data = new Uint8Array(event.target.result);
        const workbook = XLSX.read(data, { type: 'array' });

        const firstSheetName = workbook.SheetNames[0];
        const worksheet = workbook.Sheets[firstSheetName];

        const jsonData = XLSX.utils.sheet_to_json(worksheet, { header: 1, raw: false });
        const maxCols = Math.max(...jsonData.map(r => r.length));

        // Add "S.No." and "Status" columns
        let table = '<table id="excelTable">';
        jsonData.forEach((row, index) => {
            table += '<tr>';
            if (index === 0) {
                table += '<th onclick="sortTable(0)">S.No.</th>';
                for (let i = 0; i < maxCols; i++) {
                    const cell = row[i] !== undefined ? row[i] : 0;
                    table += `<th onclick="sortTable(${i + 1})">${cell}</th>`;
                }
                table += `<th onclick="sortTable(${maxCols + 1})">Status</th>`;
            } else {
                table += `<td>${index}</td>`;
                for (let i = 0; i < maxCols; i++) {
                    const cell = row[i] !== undefined ? row[i] : 0;
                    table += `<td>${cell}</td>`;
                }
                table += `<td></td>`;
            }
            table += '</tr>';
        });
        table += '</table>';
        excelDataDiv.innerHTML = table;
        isFileLoaded = true;
        // Save to localStorage
        localStorage.setItem('excelTableHTML', table);
        localStorage.setItem('excelFileName', file.name);
    };
    reader.readAsArrayBuffer(file);
});

btnPlaceOrder.addEventListener('click', () => {

    placeBulkOrder();
});
async function placeBulkOrder() {
    const table = document.querySelector('#excelData table'); // find table inside div
    const rows = table.querySelectorAll('tbody tr');
    for (const [index, row] of rows.entries()) {
        const cells = row.querySelectorAll('td');
        const symbol = cells[1]?.textContent.trim() || '';
        const qty = cells[2]?.textContent.trim() || '';
        const buy = cells[3]?.textContent.trim() || '';
        const sell = cells[4]?.textContent.trim() || '';

        if (symbol != "") {
            await placeOrder(symbol, qty, buy, sell, cells[5]);
        }
    }
}

async function placeOrder(symbol, quantity, buy, sell, cell) {
    try {
        // Build the URL with query parameters
        const url = `https://altra-bc21.onrender.com/api/Altra/PlaceOrder?symbol=${encodeURIComponent(symbol)}&quantity=${quantity}&buy=${buy}&sell=${sell}`;

        // Fetch GET request asynchronously
        const response = await fetch(url);

        if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);

        const data = await response.json();

        // Update UI (example)
        cell.innerText = data.message;
        if (data.message == "success") {
            cell.style.backgroundColor = 'lightgreen';
        }
        else {
            cell.style.backgroundColor = 'red';
        }

    } catch (err) {
        cell.innerText = 'Error placing order';
    }
}

// ----- Sort function -----
function sortTable(colIndex) {
    const table = document.getElementById("excelTable");
    let rows = Array.from(table.rows).slice(1);
    let ascending = table.getAttribute("data-sort-col") != colIndex || table.getAttribute("data-sort-dir") === "desc";

    rows.sort((a, b) => {
        let aText = a.cells[colIndex].innerText;
        let bText = b.cells[colIndex].innerText;
        let aNum = parseFloat(aText.replace(/,/g, ''));
        let bNum = parseFloat(bText.replace(/,/g, ''));
        if (!isNaN(aNum) && !isNaN(bNum)) return ascending ? aNum - bNum : bNum - aNum;
        return ascending ? aText.localeCompare(bText) : bText.localeCompare(aText);
    });

    rows.forEach(row => table.appendChild(row));
    table.setAttribute("data-sort-col", colIndex);
    table.setAttribute("data-sort-dir", ascending ? "asc" : "desc");

    // Save table after sorting
    localStorage.setItem('excelTableHTML', table.outerHTML);
}

// ----- Handle page refresh -----
window.addEventListener('beforeunload', () => {
    // Save table, file name, and button states
    const table = document.getElementById('excelTable');
    if (table) localStorage.setItem('excelTableHTML', table.outerHTML);
    localStorage.setItem('excelFileName', txtExcelFileName.textContent);
    saveButtonStates();
});

// Close dropdown if clicked outside
window.addEventListener('click', (event) => {
    if (!event.target.matches('#btnLogin') && !event.target.closest('.dropdown-content')) {
        loginDropdown.parentElement.classList.remove('show');
    }
});