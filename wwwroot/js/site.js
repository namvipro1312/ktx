// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

const searchField = document.querySelector("[data-table-search]");
const statusField = document.querySelector("[data-table-status]");
const filterRows = [...document.querySelectorAll("[data-filter-row]")];
const visibleCount = document.querySelector("[data-visible-count]");
const filterEmpty = document.querySelector("[data-filter-empty]");

function filterWorkspaceRows() {
    const query = (searchField?.value ?? "").trim().toLocaleLowerCase("vi-VN");
    const status = statusField?.value ?? "";
    let visible = 0;

    for (const row of filterRows) {
        const matchesText = row.textContent.toLocaleLowerCase("vi-VN").includes(query);
        const matchesStatus = !status || row.dataset.rowState === status;
        const show = matchesText && matchesStatus;
        row.hidden = !show;
        if (show) visible++;
    }

    if (visibleCount) visibleCount.textContent = visible.toString();
    if (filterEmpty) filterEmpty.hidden = visible !== 0;
}

searchField?.addEventListener("input", filterWorkspaceRows);
statusField?.addEventListener("change", filterWorkspaceRows);

const menuToggle = document.querySelector("#menuToggle");
const sidebar = document.querySelector("#sidebar");
const scrim = document.querySelector("#scrim");

function setMenuOpen(open) {
    sidebar?.classList.toggle("is-open", open);
    scrim?.classList.toggle("is-visible", open);
    menuToggle?.setAttribute("aria-expanded", String(open));
    menuToggle?.setAttribute("aria-label", open ? "Đóng điều hướng" : "Mở điều hướng");
    document.body.classList.toggle("menu-open", open);
}

menuToggle?.addEventListener("click", () => {
    setMenuOpen(menuToggle.getAttribute("aria-expanded") !== "true");
});
scrim?.addEventListener("click", () => setMenuOpen(false));
sidebar?.querySelectorAll("a").forEach(link => link.addEventListener("click", () => setMenuOpen(false)));
document.addEventListener("keydown", event => {
    if (event.key === "Escape") setMenuOpen(false);
});
document.querySelectorAll(".notice-close").forEach(button => {
    button.addEventListener("click", () => button.closest(".notice")?.remove());
});
