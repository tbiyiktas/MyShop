(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        const root = document.getElementById("gridDesignerRoot");
        if (!root) {
            console.error("gridDesignerRoot not found.");
            return;
        }

        const metaUrl = root.dataset.metaUrl;
        const searchUrl = root.dataset.searchUrl;

        if (!metaUrl || !searchUrl) {
            console.error("Metadata or search URLs are missing on gridDesignerRoot.");
            return;
        }

        let gridMetadata = null;

        const FilterLogicalOperator = {
            And: 0,
            Or: 1
        };

        const FilterOperation = {
            Equals: 0,
            NotEquals: 1,
            Contains: 2,
            StartsWith: 3,
            EndsWith: 4,
            GreaterThan: 5,
            GreaterThanOrEqual: 6,
            LessThan: 7,
            LessThanOrEqual: 8
        };

        const FilterOperationNames = {
            [FilterOperation.Equals]: "Equals",
            [FilterOperation.NotEquals]: "NotEquals",
            [FilterOperation.Contains]: "Contains",
            [FilterOperation.StartsWith]: "StartsWith",
            [FilterOperation.EndsWith]: "EndsWith",
            [FilterOperation.GreaterThan]: "GreaterThan",
            [FilterOperation.GreaterThanOrEqual]: "GreaterThanOrEqual",
            [FilterOperation.LessThan]: "LessThan",
            [FilterOperation.LessThanOrEqual]: "LessThanOrEqual"
        };

        const GridColumnType = {
            String: 0,
            Number: 1,
            Boolean: 2,
            DateTime: 3
        };

        const groupsContainer = document.getElementById("groupsContainer");
        const sortsContainer = document.getElementById("sortsContainer");

        const addGroupBtn = document.getElementById("addGroupBtn");
        const addSortBtn = document.getElementById("addSortBtn");
        const runSearchBtn = document.getElementById("runSearchBtn");

        if (!groupsContainer || !sortsContainer || !addGroupBtn || !addSortBtn || !runSearchBtn) {
            console.error("One or more required elements are missing.");
            return;
        }

        addGroupBtn.addEventListener("click", addGroup);
        addSortBtn.addEventListener("click", function () { addSortRow(); });
        runSearchBtn.addEventListener("click", runSearch);

        const rawJsonEl = document.getElementById("rawJson");

        // ------------- METADATA -------------

        async function loadMetadata() {
            const response = await fetch(metaUrl, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error("Failed to load grid metadata: HTTP " + response.status);
            }

            const apiResponse = await response.json(); // ApiResponse<GridMetadataDto>

            if (!apiResponse || !apiResponse.success) {
                const errText = apiResponse && apiResponse.errors
                    ? apiResponse.errors.join(" | ")
                    : "unknown error";
                throw new Error("Metadata API error: " + errText);
            }

            gridMetadata = apiResponse.data;
        }

        function getColumn(propertyPath) {
            if (!gridMetadata || !Array.isArray(gridMetadata.columns)) return null;
            return gridMetadata.columns.find(c => c.propertyPath === propertyPath) || null;
        }

        // ------------- DROPDOWN HELPERS -------------

        function fillPropertyOptions(selectElement) {
            selectElement.innerHTML = "";

            if (!gridMetadata || !Array.isArray(gridMetadata.columns)) {
                return;
            }

            gridMetadata.columns
                .filter(c => c.filterable)
                .forEach(col => {
                    const opt = document.createElement("option");
                    opt.value = col.propertyPath;
                    opt.textContent = col.displayName;
                    selectElement.appendChild(opt);
                });
        }

        function fillOperationOptions(selectElement, propertyPath) {
            selectElement.innerHTML = "";

            const col = getColumn(propertyPath);
            let allowedOps = col && Array.isArray(col.allowedOperations)
                ? col.allowedOperations
                : Object.values(FilterOperation);

            allowedOps.forEach(opValue => {
                const opt = document.createElement("option");
                opt.value = opValue;
                opt.textContent = FilterOperationNames[opValue] || ("Op(" + opValue + ")");
                selectElement.appendChild(opt);
            });
        }

        function fillSortPropertyOptions(selectElement) {
            selectElement.innerHTML = "";

            if (!gridMetadata || !Array.isArray(gridMetadata.columns)) {
                return;
            }

            gridMetadata.columns
                .filter(c => c.sortable)
                .forEach(col => {
                    const opt = document.createElement("option");
                    opt.value = col.propertyPath;
                    opt.textContent = col.displayName;
                    selectElement.appendChild(opt);
                });
        }

        // ------------- VALUE INPUT (TYPE-BASED) -------------

        function renderValueInput(conditionDiv, columnMeta) {
            const valueContainer = conditionDiv.querySelector(".value-container");
            const caseInsensitiveWrapper = conditionDiv.querySelector(".ci-wrapper");
            if (!valueContainer) return;

            valueContainer.innerHTML = "";

            if (!columnMeta) {
                const input = document.createElement("input");
                input.type = "text";
                input.className = "value-input value-text";
                valueContainer.appendChild(input);
                if (caseInsensitiveWrapper) {
                    caseInsensitiveWrapper.style.display = "";
                }
                conditionDiv.dataset.columnType = String(GridColumnType.String);
                return;
            }

            const colType = columnMeta.columnType;
            conditionDiv.dataset.columnType = String(colType);

            switch (colType) {
                case GridColumnType.Number: {
                    const input = document.createElement("input");
                    input.type = "number";
                    input.step = "0.01";
                    input.className = "value-input value-number";
                    valueContainer.appendChild(input);

                    if (caseInsensitiveWrapper) {
                        caseInsensitiveWrapper.style.display = "none";
                        const ciCheckbox = caseInsensitiveWrapper.querySelector(".case-insensitive");
                        if (ciCheckbox) ciCheckbox.checked = false;
                    }
                    break;
                }
                case GridColumnType.Boolean: {
                    const select = document.createElement("select");
                    select.className = "value-input value-boolean";
                    const optTrue = document.createElement("option");
                    optTrue.value = "true";
                    optTrue.textContent = "True";
                    const optFalse = document.createElement("option");
                    optFalse.value = "false";
                    optFalse.textContent = "False";
                    select.appendChild(optTrue);
                    select.appendChild(optFalse);
                    valueContainer.appendChild(select);

                    if (caseInsensitiveWrapper) {
                        caseInsensitiveWrapper.style.display = "none";
                        const ciCheckbox = caseInsensitiveWrapper.querySelector(".case-insensitive");
                        if (ciCheckbox) ciCheckbox.checked = false;
                    }
                    break;
                }
                case GridColumnType.DateTime: {
                    const input = document.createElement("input");
                    input.type = "date";
                    input.className = "value-input value-datetime";
                    valueContainer.appendChild(input);

                    if (caseInsensitiveWrapper) {
                        caseInsensitiveWrapper.style.display = "none";
                        const ciCheckbox = caseInsensitiveWrapper.querySelector(".case-insensitive");
                        if (ciCheckbox) ciCheckbox.checked = false;
                    }
                    break;
                }
                case GridColumnType.String:
                default: {
                    const input = document.createElement("input");
                    input.type = "text";
                    input.className = "value-input value-text";
                    valueContainer.appendChild(input);

                    if (caseInsensitiveWrapper) {
                        caseInsensitiveWrapper.style.display = "";
                    }
                    break;
                }
            }
        }

        // ------------- GROUP & CONDITION -------------

        function addGroup() {
            const groupIndex = groupsContainer.children.length;

            const groupDiv = document.createElement("div");
            groupDiv.className = "filter-group";
            groupDiv.style.border = "1px solid #ccc";
            groupDiv.style.margin = "8px 0";
            groupDiv.style.padding = "8px";

            groupDiv.innerHTML = `
                <div style="overflow:hidden;">
                    <strong>Group #${groupIndex + 1}</strong>
                    <button type="button" class="remove-group-btn" style="float:right;">x</button>
                </div>
                <div>
                    <label>Group Operator (inside group):</label>
                    <select class="group-operator">
                        <option value="0">AND</option>
                        <option value="1">OR</option>
                    </select>
                </div>
                <div class="conditionsContainer"></div>
                <button type="button" class="add-condition-btn">+ Add Condition</button>
            `;

            groupsContainer.appendChild(groupDiv);

            const addConditionBtn = groupDiv.querySelector(".add-condition-btn");
            const removeGroupBtn = groupDiv.querySelector(".remove-group-btn");
            const conditionsContainer = groupDiv.querySelector(".conditionsContainer");

            if (addConditionBtn && conditionsContainer) {
                addConditionBtn.addEventListener("click", function () {
                    addCondition(conditionsContainer);
                });
            }

            if (removeGroupBtn) {
                removeGroupBtn.addEventListener("click", function () {
                    groupDiv.remove();
                });
            }
        }

        function addCondition(container) {
            const conditionDiv = document.createElement("div");
            conditionDiv.className = "filter-condition";
            conditionDiv.style.margin = "4px 0";
            conditionDiv.style.padding = "4px";
            conditionDiv.style.border = "1px dashed #aaa";

            conditionDiv.innerHTML = `
                <label>Property:</label>
                <select class="property-path"></select>

                <label>Operation:</label>
                <select class="operation"></select>

                <span class="value-label">Value:</span>
                <span class="value-container"></span>

                <span class="ci-wrapper">
                    <label>Case Insensitive:</label>
                    <input type="checkbox" class="case-insensitive" />
                </span>

                <button type="button" class="remove-condition-btn">x</button>
            `;

            container.appendChild(conditionDiv);

            const propertySelect = conditionDiv.querySelector(".property-path");
            const operationSelect = conditionDiv.querySelector(".operation");
            const removeConditionBtn = conditionDiv.querySelector(".remove-condition-btn");

            fillPropertyOptions(propertySelect);

            function onPropertyChanged() {
                const propertyPath = propertySelect.value;
                fillOperationOptions(operationSelect, propertyPath);

                const col = getColumn(propertyPath);
                renderValueInput(conditionDiv, col);
            }

            propertySelect.addEventListener("change", onPropertyChanged);

            if (removeConditionBtn) {
                removeConditionBtn.addEventListener("click", function () {
                    conditionDiv.remove();
                });
            }

            // Başlangıçta tetikle
            propertySelect.dispatchEvent(new Event("change"));
        }

        // ------------- SORT -------------

        function addSortRow(columnMeta) {
            const rowDiv = document.createElement("div");
            rowDiv.className = "sort-row";
            rowDiv.style.margin = "4px 0";

            rowDiv.innerHTML = `
                <label>Property:</label>
                <select class="sort-property"></select>

                <label>Direction:</label>
                <select class="sort-direction">
                    <option value="false">ASC</option>
                    <option value="true">DESC</option>
                </select>

                <button type="button" class="remove-sort-btn">x</button>
            `;

            sortsContainer.appendChild(rowDiv);

            const propertySelect = rowDiv.querySelector(".sort-property");
            const directionSelect = rowDiv.querySelector(".sort-direction");
            const removeSortBtn = rowDiv.querySelector(".remove-sort-btn");

            fillSortPropertyOptions(propertySelect);

            if (columnMeta) {
                propertySelect.value = columnMeta.propertyPath;
                directionSelect.value = columnMeta.defaultSortDescending ? "true" : "false";
            }

            if (removeSortBtn) {
                removeSortBtn.addEventListener("click", function () {
                    rowDiv.remove();
                });
            }
        }

        // ------------- SEARCH -------------

        async function runSearch() {
            const groupOperatorSelect = document.getElementById("groupOperator");
            const pageIndexInput = document.getElementById("pageIndex");
            const pageSizeInput = document.getElementById("pageSize");

            const groupOperatorValue = parseInt(groupOperatorSelect.value, 10);

            const request = {
                groups: [],
                groupOperator: groupOperatorValue,
                sorts: [],
                pageIndex: parseInt(pageIndexInput.value, 10) || 1,
                pageSize: parseInt(pageSizeInput.value, 10) || 20
            };

            // Gruplar
            const groupDivs = groupsContainer.querySelectorAll(".filter-group");
            groupDivs.forEach(function (groupDiv) {
                const groupOperatorSelect = groupDiv.querySelector(".group-operator");
                const groupOperator = parseInt(groupOperatorSelect.value, 10);

                const conditions = [];
                const conditionDivs = groupDiv.querySelectorAll(".filter-condition");
                conditionDivs.forEach(function (condDiv) {
                    const propertySelect = condDiv.querySelector(".property-path");
                    const operationSelect = condDiv.querySelector(".operation");

                    const propertyPath = propertySelect.value;
                    const operation = parseInt(operationSelect.value, 10);
                    const columnType = parseInt(condDiv.dataset.columnType || GridColumnType.String, 10);

                    let value = "";
                    if (columnType === GridColumnType.Number) {
                        const input = condDiv.querySelector(".value-number");
                        value = input ? input.value : "";
                    } else if (columnType === GridColumnType.Boolean) {
                        const select = condDiv.querySelector(".value-boolean");
                        value = select ? select.value : "";
                    } else if (columnType === GridColumnType.DateTime) {
                        const input = condDiv.querySelector(".value-datetime");
                        value = input ? input.value : "";
                    } else {
                        const input = condDiv.querySelector(".value-text");
                        value = input ? input.value : "";
                    }

                    const ciCheckbox = condDiv.querySelector(".case-insensitive");
                    const caseInsensitive = !!(ciCheckbox && ciCheckbox.checked);

                    if (propertyPath && value !== "") {
                        conditions.push({
                            propertyPath: propertyPath,
                            operation: operation,
                            value: value,
                            caseInsensitive: caseInsensitive
                        });
                    }
                });

                if (conditions.length > 0) {
                    request.groups.push({
                        operator: groupOperator,
                        conditions: conditions
                    });
                }
            });

            // Sort'lar
            const sortRows = sortsContainer.querySelectorAll(".sort-row");
            sortRows.forEach(function (row) {
                const propertySelect = row.querySelector(".sort-property");
                const directionSelect = row.querySelector(".sort-direction");

                const propertyPath = propertySelect.value;
                const descending = directionSelect.value === "true";

                if (propertyPath) {
                    request.sorts.push({
                        propertyPath: propertyPath,
                        descending: descending
                    });
                }
            });

            if (rawJsonEl) {
                rawJsonEl.textContent = "REQUEST:\n" + JSON.stringify(request, null, 2);
            }

            try {
                const response = await fetch(searchUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        "Accept": "application/json"
                    },
                    body: JSON.stringify(request)
                });

                if (!response.ok) {
                    if (rawJsonEl) {
                        rawJsonEl.textContent += "\n\nHTTP Error: " + response.status;
                    }
                    return;
                }

                const apiResponse = await response.json(); // ApiResponse<PaginatedResponse<ProductDto>>

                if (rawJsonEl) {
                    rawJsonEl.textContent += "\n\nRESPONSE:\n" + JSON.stringify(apiResponse, null, 2);
                }

                renderResults(apiResponse);
            } catch (err) {
                if (rawJsonEl) {
                    rawJsonEl.textContent += "\n\nEXCEPTION:\n" + err;
                }
            }
        }

        function renderResults(apiResponse) {
            const tbody = document.querySelector("#resultsTable tbody");
            if (!tbody) return;
            tbody.innerHTML = "";

            if (!apiResponse || !apiResponse.success || !apiResponse.data) {
                return;
            }

            const page = apiResponse.data;
            const items = page.items || [];

            items.forEach(function (item) {
                const tr = document.createElement("tr");
                tr.innerHTML = `
                    <td>${item.id}</td>
                    <td>${item.name}</td>
                    <td>${item.price}</td>
                    <td>${item.stockQuantity}</td>
                    <td>${item.categoryName}</td>
                `;
                tbody.appendChild(tr);
            });
        }

        // ------------- INIT -------------

        (async function init() {
            try {
                await loadMetadata();

                // Default sort'lar
                if (gridMetadata && Array.isArray(gridMetadata.columns)) {
                    const defaultSortCols = gridMetadata.columns
                        .filter(function (c) { return c.sortable && c.isDefaultSort; });

                    if (defaultSortCols.length > 0) {
                        defaultSortCols.forEach(function (col) {
                            addSortRow(col);
                        });
                    }
                }

                // İlk grup + condition
                addGroup();
                const firstContainer = document.querySelector(".filter-group .conditionsContainer");
                if (firstContainer) {
                    addCondition(firstContainer);
                }
            } catch (err) {
                if (rawJsonEl) {
                    rawJsonEl.textContent =
                        "Failed to initialize grid designer:\n" + err;
                }
            }
        })();
    });
})();
