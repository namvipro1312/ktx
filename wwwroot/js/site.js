// Tabler Dashboard interactions for QLKTX

document.addEventListener("DOMContentLoaded", function () {
    const searchInputs = document.querySelectorAll("[data-table-search]");
    const statusSelects = document.querySelectorAll("[data-table-status]");

    function filterRows() {
        const query = (document.querySelector("[data-table-search]")?.value ?? "").trim().toLocaleLowerCase("vi-VN");
        const status = document.querySelector("[data-table-status]")?.value ?? "";
        const filterRows = document.querySelectorAll("[data-filter-row]");
        const visibleCount = document.querySelector("[data-visible-count]");
        const filterEmpty = document.querySelector("[data-filter-empty]");
        let visible = 0;

        filterRows.forEach(row => {
            const matchesText = row.textContent.toLocaleLowerCase("vi-VN").includes(query);
            const matchesStatus = !status || row.dataset.rowState === status;
            const show = matchesText && matchesStatus;
            row.hidden = !show;
            if (show) visible++;
        });

        if (visibleCount) visibleCount.textContent = visible.toString();
        if (filterEmpty) filterEmpty.hidden = visible !== 0;
    }

    searchInputs.forEach(input => input.addEventListener("input", filterRows));
    statusSelects.forEach(select => select.addEventListener("change", filterRows));
});
