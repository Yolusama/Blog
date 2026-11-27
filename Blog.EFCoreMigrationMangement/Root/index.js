const apiClient = axios.create({
    timeout: 1000*60*3,
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
const loadDataRow = document.getElementById("loadDataRow");
const removeLatestBtn = document.getElementById("removeLatestBtn");
const generateAllBtn = document.getElementById("generateAllBtn");
const migrationModal = new bootstrap.Modal(modalElement);

let editingId = null;
let rows = [];
let totalCount = 0;
let currentPage = 1;
let pageSize = Number(pageSizeSelect.value);
let searchKey = "";
const rowElements = [];
const paginationElements = [];

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

/**
 * 将字节数组下载为文件：先构造 Blob，再通过临时 <a> 标签触发下载
 */
function downloadBytes(bytes, fileName, contentType) {
    const blob = new Blob([bytes], { type: contentType || "application/octet-stream" });
    const url = URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = url;
    link.download = fileName || "output.sql";
    link.style.display = "none";
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    // 释放内存（延迟到下载已开始后再 revoke）
    setTimeout(() => URL.revokeObjectURL(url), 1000);
}

/**
 * 从 Content-Disposition 中解析服务端给出的文件名，缺失时回退到 fallback
 * 后端现在返回真实文件名：
 *   单个迁移：{id}-{name}-{时间戳}-Migration.sql
 *   全量脚本：Migration-{时间戳}.sql
 */
function resolveFileNameFromHeaders(response, fallback) {
    const headers = response?.headers ?? {};
    const disposition = headers["content-disposition"] ?? headers["Content-Disposition"];
    if (!disposition) return fallback;

    // RFC 5987 形式：filename*=UTF-8''%E4%B8%AD%E6%96%87.sql（中文/特殊字符走这里）
    const utf8Match = disposition.match(/filename\*\s*=\s*UTF-8''([^;]+)/i);
    if (utf8Match?.[1]) {
        try {
            const name = decodeURIComponent(utf8Match[1].trim());
            if (name) return name;
        } catch {
            // 非法的 URL 编码，继续尝试普通 filename
        }
    }

    // 普通形式：filename="name.sql"
    const match = disposition.match(/filename\s*=\s*"?([^";]+)"?/i);
    const name = match?.[1]?.trim();
    return name || fallback;
}

/**
 * 下载接口出错时，响应体是 ArrayBuffer，这里尝试解析出可读的错误信息
 */
function readDownloadError(error, fallback) {
    const data = error?.response?.data;
    if (data instanceof ArrayBuffer) {
        try {
            const text = new TextDecoder("utf-8").decode(new Uint8Array(data));
            try {
                const parsed = JSON.parse(text);
                return parsed?.message || parsed?.title || parsed?.detail || fallback;
            } catch {
                // 非 JSON（例如 HTML 错误页），过滤掉 HTML 标签后截断展示
                const plain = text.replace(/<[^>]+>/g, " ").replace(/\s+/g, " ").trim();
                return plain && plain.length <= 300 ? plain : fallback;
            }
        } catch {
            return fallback;
        }
    }
    return data?.message || error?.message || fallback;
}

/**
 * 生成单个迁移对应的 SQL 脚本（对应后端 GET /api/migrations/generate/{id}）
 * 注意：脚本内容是「上一条迁移 → 本条迁移」的增量，只包含该迁移自身的变更，
 * 而不是从空库累积到该迁移（dotnet ef migrations script <to> 的默认行为）
 */
async function GenerateMigrationScript(item) {
    if (!item) return;

    const confirmed = await MessageBox.confirm(
        `将生成迁移脚本，仅包含迁移「${item.name}」自身的变更，是否继续？`,
        { title: "生成迁移脚本", type: "info", confirmText: "生成" }
    );
    if (!confirmed) return;

    const loading = Loading.show({
        text: "正在生成迁移脚本…",
        subText: `dotnet ef 需要先编译项目，请勿关闭或刷新页面：${item.name}`
    });

    try {
        const response = await apiClient.get(`${apiUrl}/generate/${encodeURIComponent(item.id)}`, {
            params: { name: item.name ?? "" },
            responseType: "arraybuffer"
        });
        const fileName = resolveFileNameFromHeaders(response, `${item.name || "migration"}.sql`);
        downloadBytes(response.data, fileName, response.headers?.["content-type"]);
        Message.success(`迁移脚本「${fileName}」已开始下载`);
    } catch (error) {
        console.error(error);
        Message.error(readDownloadError(error, "生成迁移脚本失败，请稍后重试。"));
    } finally {
        loading.close();
    }
}

/**
 * 生成从迁移历史底到最新的全量 SQL 脚本（对应后端 GET /api/migrations/fullyGenerate）
 */
async function generateAllMigrationScript() {
    const confirmed = await MessageBox.confirm(
        "将从迁移历史从头到尾到生成 SQL 迁移脚本，是否继续？",
        { title: "全迁移", type: "info", confirmText: "生成" }
    );
    if (!confirmed) return;

    const loading = Loading.show({
        text: "正在生成全量迁移脚本…",
        subText: "dotnet ef 需要先编译项目，请勿关闭或刷新页面"
    });

    try {
        const response = await apiClient.get(`${apiUrl}/fullyGenerate`, {
            responseType: "arraybuffer"
        });
        const stamp = new Date().toISOString().replace(/[:.]/g, "-").slice(0, 19);
        const fileName = resolveFileNameFromHeaders(response, `migration-full-${stamp}.sql`);
        downloadBytes(response.data, fileName, response.headers?.["content-type"]);
        Message.success(`全量迁移脚本「${fileName}」已开始下载`);
    } catch (error) {
        console.error(error);
        Message.error(readDownloadError(error, "生成全量迁移脚本失败，请稍后重试。"));
    } finally {
        loading.close();
    }
}

function renderRows() {
    loadDataRow.style.display = "none";
    if (!rows.length) {
        const emptyRow = document.createElement("tr");
        const emptyTd = document.createElement("td");
        emptyTd.colSpan = 6;
        emptyTd.className = "text-center py-4 text-muted";
        emptyTd.textContent = "暂无数据";
        emptyRow.appendChild(emptyTd);
        tableBody.appendChild(emptyRow);
        rowElements.push(emptyRow);
        return;
    }

      rows.forEach(item => {
          const row = document.createElement("tr");
          const idTd = document.createElement("td");
          idTd.textContent = item.id;
          const nameTd = document.createElement("td");
          nameTd.textContent = item.name;
          nameTd.className = "text-overflow-ellipse";
          nameTd.style.maxWidth = "150px";
          nameTd.title = item.name;
          const createTimeTd = document.createElement("td");
          createTimeTd.textContent = formatDateTime(item.createTime);
          const fileNameTd = document.createElement("td");
          fileNameTd.textContent = item.fileName;
          fileNameTd.style.maxWidth = "200px";
          fileNameTd.title = item.fileName;
          fileNameTd.className = "text-overflow-ellipse";
          const applyTimeTd = document.createElement("td");
          applyTimeTd.textContent = formatDateTime(item.applyTime);
          const actionTd = document.createElement("td");
          actionTd.className = "text-center action-buttons";
          const applyBtn = document.createElement("button");
          applyBtn.type = "button";
          applyBtn.className = "btn btn-sm btn-outline-primary me-2 js-edit";
          applyBtn.dataset.id = item.id;
          applyBtn.textContent = "应用";
          applyBtn.addEventListener("click", () => applyMigrate(item));
          const deleteBtn = document.createElement("button");
          deleteBtn.type = "button";
          deleteBtn.className = "btn btn-sm btn-outline-danger js-delete";
          deleteBtn.dataset.id = item.id;
          deleteBtn.dataset.name = item.name;
          deleteBtn.textContent = "删除";
          deleteBtn.style.display = "none";
          const generateScriptBtn = document.createElement("button");
          generateScriptBtn.type = "button";
          generateScriptBtn.className = "btn btn-sm btn-outline-success js-info";
          generateScriptBtn.dataset.id = item.id;
          generateScriptBtn.dataset.name = item.name;
          generateScriptBtn.textContent = "迁移";
          generateScriptBtn.title = "生成该迁移的增量 SQL 脚本（仅包含这一条迁移的变更）";
          generateScriptBtn.addEventListener("click", () => GenerateMigrationScript(item));
          actionTd.append(applyBtn, deleteBtn, generateScriptBtn);
          row.append(idTd, nameTd, createTimeTd, fileNameTd, applyTimeTd, actionTd);
          tableBody.appendChild(row);
          rowElements.push(row);
      });
}

function createPaginationItem(label, page, disabled, active,callback) {
    const liClass = ["page-item"];
    if (disabled) liClass.push("disabled");
    if (active) liClass.push("active");
    const liItem = document.createElement("li");
    liClass.forEach(cls => liItem.classList.add(cls));
    const button = document.createElement("button");
    button.type = "button";
    button.className = "page-link js-page";
    button.dataset.page = page;
    button.disabled = disabled;
    button.textContent = label;
    button.addEventListener("click", callback);
    liItem.appendChild(button);
    return liItem;
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
    items.push(createPaginationItem("上一页", currentPage - 1, currentPage === 1, false, async () => {
        if (currentPage === 1) return;
        currentPage--;
        await loadMigrations();
    }));

    const maxButtons = 5;
    let begin = Math.max(1, currentPage - 2);
    let end = Math.min(totalPages, begin + maxButtons - 1);
    begin = Math.max(1, end - maxButtons + 1);

    for (let i = begin; i <= end; i += 1) {
        items.push(createPaginationItem(i, i, false, i === currentPage, async () => {
            if (i === currentPage) return;
            currentPage = i;
            await loadMigrations();
        }));
    }

    items.push(createPaginationItem("下一页", currentPage + 1, currentPage === totalPages, false, async () => {
        if (currentPage === totalPages) return;
        currentPage++;
        await loadMigrations();
    }));
    items.forEach(item => paginationElement.appendChild(item));
    paginationElements.push(...items);
}

function renderTable() {
    renderRows();
    renderPagination();
}

async function loadMigrations() {
    try {
        rows.splice(0, rows.length);
        rowElements.forEach(row => tableBody.removeChild(row));
        rowElements.splice(0, rowElements.length);
        paginationElements.forEach(item => paginationElement.removeChild(item));
        paginationElements.splice(0, paginationElements.length);
        loadDataRow.style.display = "block";
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
        tableBody.innerHTML = "<tr><td colspan=\"6\" class=\"text-center py-4 text-danger\">加载失败</td></tr>";
        pageInfo.textContent = "加载失败";
        paginationElement.innerHTML = "";
        Message.error("加载迁移列表失败，请稍后重试。");
    }
}

/**
 * 应用迁移（对应 ef database update）
 * MessageBox 返回 Promise，可直接 await 拿到用户选择：
 *   confirm  → true / false
 *   alert    → true / false
 *   prompt   → string / null
 */
async function applyMigrate(row) {
    if (!row) return;

    // ① await 拿到布尔结果：点“应用”为 true，取消/关闭遮罩/Esc 为 false
    const confirmed = await MessageBox.confirm(
        `确定要将迁移「${row.name}」应用到数据库吗？该操作可能修改数据库结构。`,
        { title: "应用迁移", type: "warning", confirmText: "应用" }
    );
    if (!confirmed) return; // 用户取消，什么都不做

    const loading = Loading.show({
        text: "正在应用迁移…",
        subText: `执行 database update：${row.name}，请勿关闭页面`
    });

    try {
        const result = await updateMigration({ id: row.id, name: row.name });
        loading.close();
        if (!showResultMessage(result, "应用成功")) return;

        await loadMigrations();
    } catch (error) {
        console.error(error);
        loading.close();
        Message.error("应用迁移失败，请稍后重试。");
    }
}

async function removeMigrate(row) {
    if(!row) return;

    const confirmed = await MessageBox.confirm(
        `确定要删除迁移「${row.name}」吗？对应的迁移文件将被一并移除。`,
        { title: "删除迁移", type: "error", confirmText: "删除", danger: true }
    );
    if (!confirmed) return;

    const loading = Loading.show({
        text: "正在删除迁移…",
        subText: `移除迁移文件：${row.name}`
    });

    try {
        const result = await deleteMigration(row.id, row.name);
        loading.close();
        if (!showResultMessage(result, "删除成功")) return;

        await loadMigrations();
    } catch (error) {
        console.error(error);
        loading.close();
        Message.error("删除迁移失败，请稍后重试。");
    }
}

/**
 * 应用迁移（对应 ef database update）
 * 后端路由：[HttpPut("apply")] → PUT /api/migrations/apply
 */
async function updateMigration(payload) {
    const result = await apiClient.put(`${apiUrl}/apply`, {
        id: payload?.id ?? "",
        name: payload?.name ?? ""
    });
    return result.data;
}

// 其它几种调用示例（按需使用，当前流程未调用）：
// await MessageBox.alert("操作已完成");
// await MessageBox.success("保存成功", { title: "提示" });
// const name = await MessageBox.prompt("请输入迁移名称", { inputValue: row.name });
// if (name === null) return;   // null 表示取消
// MessageBox.close();          // 主动关闭��前弹窗，按取消处理

function openCreateModal() {
    editingId = null;
    modalTitle.textContent = "添加迁移";
    migrationForm.reset();
    migrationModal.show();
}

async function createMigration(payload) {
    const result = await apiClient.post(apiUrl, payload);
    return result.data;
}


async function deleteMigration(id, name) {
    const result = await apiClient.delete(`${apiUrl}/${encodeURIComponent(id)}`, {
        params: { name: name ?? "" }
    });
    return result.data;
}

addBtn.addEventListener("click", openCreateModal);

if (generateAllBtn) {
    generateAllBtn.addEventListener("click", generateAllMigrationScript);
}

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

    const submitBtn = migrationForm.querySelector('button[type="submit"]');
    const loading = Loading.show({
        text: "正在生成迁移文件…",
        subText: "dotnet ef 需要先编译项目，请勿关闭或刷新页面"
    });
    if (submitBtn) submitBtn.disabled = true;

    try {
        const result = await createMigration(payload);
        loading.close();
        if (!showResultMessage(result, "添加成功")) return;

        migrationModal.hide();
        await loadMigrations();
    } catch (error) {
        console.error(error);
        loading.close();
        Message.error("保存失败，请稍后重试。");
    } finally {
        if (submitBtn) submitBtn.disabled = false;
    }
});

removeLatestBtn.addEventListener('click',async ()=>{
    try 
    {
        const confirmed = await MessageBox.confirm(
            `此操作将移除最新未应用迁移，是否继续？`,
            { title: "删除最近迁移", type: "warning", confirmText: "确定"}
        );
        if(!confirmed) return;

        const loading = Loading.show({
            text: "正在移除最新迁移…",
            subText: "dotnet ef 需要先编译项目，请勿关闭或刷新页面"
        });

        let res;
        try {
            const response = await apiClient.delete(`${apiUrl}/RemoveLatest`);
            res = response.data;
        } finally {
            loading.close();
        }

        if(!res?.success) {
            Message.error(res?.message || "移除失败，请稍后重试。");
            return;
        }
        Message.success(res.message);
        await loadMigrations();
    }
    catch(error) {
        console.error(error);
        Message.error(error);
    }
});

window.onload = (async () => {
    await loadMigrations();
    window.onload = null;
});