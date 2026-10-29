---
version: 1
slug: "dashboard-index"
primary_target: "Dashboard/Index"
related_targets: []
---

## Surface strategy

Scope: the authenticated dormitory workspace, with role-aware administrator and student views. Mode: Operate.
Audience and job: staff need to resolve applications, room allocation, contracts, maintenance, billing, and reports; students need to request a room and follow their stay. Actions: submit, review, assign, sign, check in/out, request service, and settle invoices. Proof: linked records and status history in SQL Server. Constraints: preserve privacy, distinguish synthetic demo data, and keep staff actions traceable.

## Direction contract

THESIS: A student’s stay moves through connected stages. Keep familiar navigation and operational tables; avoid a dashboard made only from generic metric cards.
OWN-WORLD: Midnight navy #14243D is the route board, warm off-white #F4F0E6 is the working surface, and red, blue, amber, and green distinguish lifecycle routes. White station nodes mark handoffs; statuses always include text. Tables and forms remain standard, legible web controls.
STORY: Staff see what waits, who owns the next step, and can continue into the matching record. Students see their own application, room, contract, services, and balance, then can take the next allowed action.
FIRST VIEWPORT: A compact left navigation frames a broad work area. The upper area shows a route from application through approval, contract, occupancy, and payment with current counts; below it, a pending-work table anchors the page. Primary action sits beside the page title. Student landing page uses the same route for their own stay and a short task list.
FORM: Transit Network, winning challenger fused with dormitory operations, ranked above resident register and wayfinding; seed:a561cfb8415f6804. Signature interaction: selecting a route station filters the matching work list without changing primary navigation.
FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance
