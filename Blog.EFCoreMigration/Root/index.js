const apiClient = axios.create({
    baseURL: "http://localhost:5576"
});

const apiUrl = "/api/migrations";

const tableBody = document.getElementById("migrationTableBody");
const addBtn = document.getElementById("addMigrationBtn");
const modalElement = document.getElementById("migrationModal");
const modalTitle = document.getElementById("modalTitle");
const migrationForm = document.getElementById("migrationForm");
const migrationNameInput = document.getElementById("migrationName");
const pageInfo = document.getElementById("pageInfo");
const paginationElement = document.getElementById("pagination");
const pageSizeSelect = document.getElementById("pageSizeSelect");
const searchInput = document.getElementById("searchInput");
const searchBtn = document.getElementById("searchBtn");
const migrationModal = new bootstrap.Modal(modalElement);

let editingId = null;
let rows = [];
let totalCount = 0;
let currentPage = 1;
let pageSize = Number(pageSizeSelect.value);
let searchKey = "";

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

/**
 * 统一处理非查询接口的响应结果 { success, message, data }
 * 成功：优先展示服务端返回的 message，缺失时用 fallback
 * 失败：展示服务端返回的错误信息
 * 返回是否成功
 */
function showResultMessage(result, fallback) {
    if (result?.success) {
        Message.success(result.message || fallback || "操作成功");
        return true;
    }
    Message.error(result?.message || "操作失败，请稍后重试。");
    return false;
}

function getTotalPages() {
    return Math.max(1, Math.ceil(totalCount / pageSize));
}

function renderRows() {
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
                <button type="button" class="btn btn-sm btn-outline-danger js-delete" data-id="${escapeHtml(item.id)}" data-name="${escapeHtml(item.name)}">删除</button>
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
    const totalPages = getTotalPages();
    const startIndex = totalCount === 0 ? 0 : (currentPage - 1) * pageSize + 1;
    const endIndex = Math.min(currentPage * pageSize, totalCount);
    pageInfo.textContent = `共 ${totalCount} 条，第 ${currentPage}/${totalPages} 页（${startIndex}-${endIndex}）`;

    if (totalCount === 0) {
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
        const response = await apiClient.get(apiUrl, {
            params: {
                search: searchKey,
                page: currentPage,
                pageSize: pageSize
            }
        });
        const result = response.data?.data ?? {};
        rows = Array.isArray(result.data) ? result.data : [];
        totalCount = Number(result.total ?? 0);

        const totalPages = getTotalPages();
        if (currentPage > totalPages) {
            currentPage = totalPages;
            return loadMigrations();
        }

        renderTable();
    } catch (error) {
        console.error(error);
        tableBody.innerHTML = "<tr><td colspan=\"5\" class=\"text-center py-4 text-danger\">加载失败</td></tr>";
        pageInfo.textContent = "加载失败";
        paginationElement.innerHTML = "";
        Message.error("加载迁移列表失败，请稍后重试。");
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
    migrationModal.show();
}

async function createMigration(payload) {
    const result = await apiClient.post(apiUrl, payload);
    return result.data;
}

async function updateMigration(id, payload) {
    const result = await apiClient.put(`${apiUrl}/${encodeURIComponent(id)}`, payload);
    return result.data;
}

async function deleteMigration(id, name) {
    const result = await apiClient.delete(`${apiUrl}/${encodeURIComponent(id)}`, {
        params: { name: name ?? "" }
    });
    return result.data;
}

addBtn.addEventListener("click", openCreateModal);

pageSizeSelect.addEventListener("change", async () => {
    pageSize = Number(pageSizeSelect.value);
    currentPage = 1;
    await loadMigrations();
});

async function applySearch() {
    searchKey = searchInput.value.trim();
    currentPage = 1;
    await loadMigrations();
}

searchBtn.addEventListener("click", applySearch);

searchInput.addEventListener("keydown", async (event) => {
    if (event.key === "Enter") {
        event.preventDefault();
        await applySearch();
    }
});

migrationForm.addEventListener("submit", async (event) => {
    event.preventDefault();
    const payload = {
        id: editingId ?? "",
        name : migrationNameInput.value
    };

    if (!payload.name) {
        Message.warning("请填写迁移名称。");
        return;
    }

    try {
        const isEdit = Boolean(editingId);
        const result = isEdit
            ? await updateMigration(editingId, payload)
            : await createMigration(payload);

        if (!showResultMessage(result, isEdit ? "更新成功" : "添加成功")) return;

        migrationModal.hide();
        await loadMigrations();
    } catch (error) {
        console.error(error);
        Message.error("保存失败，请稍后重试。");
    }
});

document.addEventListener("click", async (event) => {
    const target = event.target;
    if (!(target instanceof HTMLElement)) return;

    if (target.classList.contains("js-page")) {
        const page = Number(target.dataset.page);
        const totalPages = getTotalPages();
        if (!Number.isInteger(page) || page < 1 || page > totalPages || page === currentPage) return;
        currentPage = page;
        await loadMigrations();
        return;
    }

    const id = target.dataset.id;
    if (!id) return;

    if (target.classList.contains("js-edit")) {
        const row = rows.find(item => item.id === id);
        if (!row) return;
        openEditModal(row);
        return;
    }

    if (!target.classList.contains("js-delete")) return;
    if (!confirm("确认删除这条迁移记录吗？")) return;

    try {
        const result = await deleteMigration(id, target.dataset.name);
        if (!showResultMessage(result, "删除成功")) return;

        await loadMigrations();
    } catch (error) {

        console.error(error);
        Message.error("删除失败，请稍后重试。");
    }
});

window.onload = (async () => {
    await loadMigrations();
    window.onload = null;
});