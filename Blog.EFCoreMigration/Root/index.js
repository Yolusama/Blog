const apiClient = axios.create({
    baseURL: "/api/migrations"
});

const tableBody = document.getElementById("migrationTableBody");
const addBtn = document.getElementById("addMigrationBtn");
const modalElement = document.getElementById("migrationModal");
const modalTitle = document.getElementById("modalTitle");
const migrationForm = document.getElementById("migrationForm");
const migrationNameInput = document.getElementById("migrationName");
const migrationFileNameInput = document.getElementById("migrationFileName");
const pageInfo = document.getElementById("pageInfo");
const paginationElement = document.getElementById("pagination");
const pageSizeSelect = document.getElementById("pageSizeSelect");
const migrationModal = new bootstrap.Modal(modalElement);

let editingId = null;
let allMigrations = [];
let currentPage = 1;
let pageSize = Number(pageSizeSelect.value);

function escapeHtml(value) {
    return String(value ?? "")
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll("\"", "&quot;")
        .replaceAll("'", "&#39;");
}

function formatDateTime(value) {
    if (!value) return "-";
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return value;
    return date.toLocaleString();
}

function getTotalPages() {
    return Math.max(1, Math.ceil(allMigrations.length / pageSize));
}

function getPagedRows() {
    const start = (currentPage - 1) * pageSize;
    const end = start + pageSize;
    return allMigrations.slice(start, end);
}

function renderRows() {
    const rows = getPagedRows();
    if (!rows.length) {
        tableBody.innerHTML = "<tr><td colspan=\"5\" class=\"text-center py-4 text-muted\">暂无数据</td></tr>";
        return;
    }

    tableBody.innerHTML = rows.map(item => `
        <tr>
            <td>${escapeHtml(item.id)}</td>
            <td>${escapeHtml(item.name)}</td>
            <td>${escapeHtml(formatDateTime(item.createTime))}</td>
            <td>${escapeHtml(item.fileName)}</td>
            <td class="text-center action-buttons">
                <button type="button" class="btn btn-sm btn-outline-primary me-2 js-edit" data-id="${escapeHtml(item.id)}">更新</button>
                <button type="button" class="btn btn-sm btn-outline-danger js-delete" data-id="${escapeHtml(item.id)}">删除</button>
            </td>
        </tr>
    `).join("");
}

function createPaginationItem(label, page, disabled, active) {
    const liClass = ["page-item"];
    if (disabled) liClass.push("disabled");
    if (active) liClass.push("active");

    return `
        <li class="${liClass.join(" ")}">
            <button class="page-link js-page" data-page="${page}" ${disabled ? "disabled" : ""}>${label}</button>
        </li>
    `;
}

function renderPagination() {
    const totalItems = allMigrations.length;
    const totalPages = getTotalPages();
    if (currentPage > totalPages) {
        currentPage = totalPages;
    }

    const startIndex = totalItems === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const endIndex = Math.min(currentPage * pageSize, totalItems);
    pageInfo.textContent = `共 ${totalItems} 条，第 ${currentPage}/${totalPages} 页（${startIndex}-${endIndex}）`;

    if (totalItems === 0) {
        paginationElement.innerHTML = "";
        return;
    }

    const items = [];
    items.push(createPaginationItem("上一页", currentPage - 1, currentPage === 1, false));

    const maxButtons = 5;
    let begin = Math.max(1, currentPage - 2);
    let end = Math.min(totalPages, begin + maxButtons - 1);
    begin = Math.max(1, end - maxButtons + 1);

    for (let i = begin; i <= end; i += 1) {
        items.push(createPaginationItem(i, i, false, i === currentPage));
    }

    items.push(createPaginationItem("下一页", currentPage + 1, currentPage === totalPages, false));
    paginationElement.innerHTML = items.join("");
}

function renderTable() {
    renderRows();
    renderPagination();
}

async function loadMigrations() {
    try {
        const response = await apiClient.get("/");
        allMigrations = Array.isArray(response.data) ? response.data : [];
        currentPage = 1;
        renderTable();
    } catch (error) {
        console.error(error);
        tableBody.innerHTML = "<tr><td colspan=\"5\" class=\"text-center py-4 text-danger\">加载失败</td></tr>";
        pageInfo.textContent = "加载失败";
        paginationElement.innerHTML = "";
    }
}

function openCreateModal() {
    editingId = null;
    modalTitle.textContent = "添加迁移";
    migrationForm.reset();
    migrationModal.show();
}

function openEditModal(row) {
    editingId = row.id;
    modalTitle.textContent = "更新迁移";
    migrationNameInput.value = row.name ?? "";
    migrationFileNameInput.value = row.fileName ?? "";
    migrationModal.show();
}

async function createMigration(payload) {
    await apiClient.post("/", payload);
}

async function updateMigration(id, payload) {
    await apiClient.put(`/${encodeURIComponent(id)}`, payload);
}

async function deleteMigration(id) {
    await apiClient.delete(`/${encodeURIComponent(id)}`);
}

addBtn.addEventListener("click", openCreateModal);

pageSizeSelect.addEventListener("change", () => {
    pageSize = Number(pageSizeSelect.value);
    currentPage = 1;
    renderTable();
});

migrationForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const payload = {
        name: migrationNameInput.value.trim(),
        fileName: migrationFileNameInput.value.trim()
    };

    if (!payload.name || !payload.fileName) {
        alert("请填写完整信息。");
        return;
    }

    try {
        if (editingId) {
            await updateMigration(editingId, payload);
        } else {
            await createMigration(payload);
        }
        migrationModal.hide();
        await loadMigrations();
    } catch (error) {
        console.error(error);
        alert("保存失败，请稍后重试。");
    }
});

document.addEventListener("click", async (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) return;

    if (target.classList.contains("js-page")) {
        const page = Number(target.dataset.page);
        const totalPages = getTotalPages();
        if (!Number.isInteger(page) || page < 1 || page > totalPages) return;
        currentPage = page;
        renderTable();
        return;
    }

    const id = target.dataset.id;
    if (!id) return;

    if (target.classList.contains("js-edit")) {
        const row = allMigrations.find(item => item.id === id);
        if (!row) return;
        openEditModal(row);
        return;
    }

    if (!target.classList.contains("js-delete")) return;
    if (!confirm("确认删除这条迁移记录吗？")) return;

    try {
        await deleteMigration(id);
        await loadMigrations();
    } catch (error) {
        console.error(error);
        alert("删除失败，请稍后重试。");
    }
});

loadMigrations();
