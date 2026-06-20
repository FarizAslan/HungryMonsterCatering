// Hungry Monster - Catering Ledger frontend
// Talks to the ASP.NET Core minimal API endpoints defined in Program.cs.

const API = {
  contractors: "/api/contractors",
  data: "/api/data",
  analysis: "/api/analysis",
};

let currentContractors = [];

// ---------------------------------------------------------------
// Tab navigation
// ---------------------------------------------------------------
function initTabs() {
  const steps = document.querySelectorAll(".stepper__step");
  steps.forEach((step) => {
    step.addEventListener("click", () => activateTab(step.dataset.tab));
  });
}

function activateTab(tabName) {
  document.querySelectorAll(".stepper__step").forEach((s) => {
    s.classList.toggle("is-active", s.dataset.tab === tabName);
  });
  document.querySelectorAll(".panel").forEach((p) => {
    p.classList.toggle("is-active", p.id === `tab-${tabName}`);
  });

  if (tabName === "entry") buildEntryForm();
  if (tabName === "ledger") loadLedger();
  if (tabName === "verdict") resetVerdict();
}

// ---------------------------------------------------------------
// Tab 1: Partners
// ---------------------------------------------------------------
async function loadContractors() {
  const res = await fetch(API.contractors);
  currentContractors = await res.json();
  renderPartnerChips();
}

function renderPartnerChips() {
  const container = document.getElementById("partner-chips");
  const emptyNote = document.getElementById("partner-empty-note");

  container.querySelectorAll(".chip").forEach((c) => c.remove());

  if (currentContractors.length === 0) {
    emptyNote.style.display = "block";
    return;
  }

  emptyNote.style.display = "none";
  currentContractors.forEach((name) => {
    const chip = document.createElement("span");
    chip.className = "chip";
    chip.textContent = name;
    container.appendChild(chip);
  });
}

function showMessage(elementId, text, isError) {
  const el = document.getElementById(elementId);
  el.textContent = text;
  el.classList.toggle("is-error", !!isError);
  el.classList.toggle("is-success", !isError);
}

document.addEventListener("DOMContentLoaded", () => {
  initTabs();
  loadContractors();

  document.getElementById("form-add-partner").addEventListener("submit", async (e) => {
    e.preventDefault();
    const input = document.getElementById("input-partner-name");
    const name = input.value.trim();
    if (!name) return;

    const res = await fetch(API.contractors, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ name }),
    });

    if (res.ok) {
      currentContractors = await res.json();
      renderPartnerChips();
      showMessage("partner-message", `"${name}" added.`, false);
      input.value = "";
      input.focus();
    } else {
      const err = await res.json();
      showMessage("partner-message", err.message ?? "Could not add partner.", true);
    }
  });

  document.getElementById("form-entry").addEventListener("submit", async (e) => {
    e.preventDefault();
    const year = parseInt(document.getElementById("input-year").value, 10);
    const meals = {};

    currentContractors.forEach((name) => {
      const input = document.getElementById(`meals-${slug(name)}`);
      meals[name] = input ? parseInt(input.value || "0", 10) : 0;
    });

    const res = await fetch(API.data, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ year, meals }),
    });

    if (res.ok) {
      showMessage("entry-message", `Saved data for ${year}.`, false);
    } else {
      const err = await res.json();
      showMessage("entry-message", err.message ?? "Could not save data.", true);
    }
  });

  document.getElementById("btn-run-analysis").addEventListener("click", runAnalysis);
});

// ---------------------------------------------------------------
// Tab 2: Yearly entry
// ---------------------------------------------------------------
function slug(name) {
  return name.toLowerCase().replace(/[^a-z0-9]+/g, "-");
}

function buildEntryForm() {
  const noPartners = document.getElementById("entry-no-partners");
  const form = document.getElementById("form-entry");
  const grid = document.getElementById("entry-grid");

  if (currentContractors.length === 0) {
    noPartners.style.display = "block";
    form.style.display = "none";
    return;
  }

  noPartners.style.display = "none";
  form.style.display = "block";
  grid.innerHTML = "";

  currentContractors.forEach((name) => {
    const wrap = document.createElement("div");
    wrap.className = "entry-field";

    const label = document.createElement("label");
    label.setAttribute("for", `meals-${slug(name)}`);
    label.textContent = name;

    const input = document.createElement("input");
    input.type = "number";
    input.min = "0";
    input.value = "0";
    input.id = `meals-${slug(name)}`;

    wrap.appendChild(label);
    wrap.appendChild(input);
    grid.appendChild(wrap);
  });
}

// ---------------------------------------------------------------
// Tab 3: Ledger
// ---------------------------------------------------------------
async function loadLedger() {
  const res = await fetch(API.data);
  const data = await res.json();

  const empty = document.getElementById("ledger-empty");
  const table = document.getElementById("ledger-table");
  const head = document.getElementById("ledger-head");
  const body = document.getElementById("ledger-body");

  if (!data.rows || data.rows.length === 0) {
    empty.style.display = "block";
    table.style.display = "none";
    return;
  }

  empty.style.display = "none";
  table.style.display = "table";

  head.innerHTML = "<th>Year</th>" +
    data.contractors.map((c) => `<th>${c}</th>`).join("") +
    "<th>Active partners</th>";

  body.innerHTML = data.rows
    .map((row) => {
      const cells = data.contractors.map((c) => `<td>${row.meals[c] ?? 0}</td>`).join("");
      return `<tr><td>${row.year}</td>${cells}<td class="active-count">${row.activeContractors}</td></tr>`;
    })
    .join("");
}

// ---------------------------------------------------------------
// Tab 4: Verdict (analysis)
// ---------------------------------------------------------------
function resetVerdict() {
  document.getElementById("verdict-result").style.display = "none";
  document.getElementById("verdict-empty").style.display = "none";
}

async function runAnalysis() {
  const res = await fetch(API.analysis);
  const emptyEl = document.getElementById("verdict-empty");
  const resultEl = document.getElementById("verdict-result");

  if (!res.ok) {
    const err = await res.json();
    emptyEl.textContent = err.message ?? "No data to analyze yet.";
    emptyEl.style.display = "block";
    resultEl.style.display = "none";
    return;
  }

  const result = await res.json();
  emptyEl.style.display = "none";
  resultEl.style.display = "flex";

  document.getElementById("stamp-year").textContent = result.years.join(" & ");
  const word = result.activeContractorCount === 1 ? "partner" : "partners";
  document.getElementById("stamp-detail").textContent =
    `${result.activeContractorCount} active ${word}` +
    (result.years.length > 1 ? " (tie)" : "");
}
