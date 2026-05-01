const STATUS_OPCOES = [
    { value: "Recebido", label: "Recebido" },
    { value: "EmAndamento", label: "Em Andamento" },
    { value: "Pronto", label: "Pronto" },
    { value: "Entregue", label: "Entregue" },
    { value: "Cancelado", label: "Cancelado" }
];

const PEDIDO_NEXT_STATUS = {
    Recebido: "EmAndamento",
    EmAndamento: "Pronto",
    Pronto: "Entregue"
};

const STATUS_TRANSICOES = {
    Recebido: ["Recebido", "EmAndamento", "Cancelado"],
    EmAndamento: ["EmAndamento", "Pronto", "Cancelado"],
    Pronto: ["Pronto", "Entregue", "Cancelado"],
    Entregue: ["Entregue"],
    Cancelado: ["Cancelado"]
};

const estado = {
    usuario: null,
    clientes: [],
    materiais: [],
    pedidos: []
};

const ui = {
    loginScreen: document.getElementById("login-screen"),
    mainScreen: document.getElementById("main-screen"),
    loginForm: document.getElementById("login-form"),
    loginError: document.getElementById("login-error"),
    logoutBtn: document.getElementById("logout-btn"),
    userName: document.getElementById("user-name"),
    pageTitle: document.getElementById("page-title"),
    currentDate: document.getElementById("current-date"),

    clienteForm: document.getElementById("cliente-form"),
    clienteNome: document.getElementById("cliente-nome"),
    clienteTelefone: document.getElementById("cliente-telefone"),
    clienteEmail: document.getElementById("cliente-email"),
    clienteEndereco: document.getElementById("cliente-endereco"),
    clienteFeedback: document.getElementById("cliente-feedback"),

    materialForm: document.getElementById("material-form"),
    materialNome: document.getElementById("material-nome"),
    materialDescricao: document.getElementById("material-descricao"),
    materialFeedback: document.getElementById("material-feedback"),

    pedidoForm: document.getElementById("pedido-form"),
    pedidoCliente: document.getElementById("pedido-cliente"),
    pedidoEntrega: document.getElementById("pedido-entrega"),
    pedidoObservacoes: document.getElementById("pedido-observacoes"),
    pedidoItens: document.getElementById("pedido-itens"),
    adicionarItem: document.getElementById("adicionar-item"),
    pedidoFeedback: document.getElementById("pedido-feedback"),
    pedidoStatusFilter: document.getElementById("pedido-status-filter"),

    kpiActive: document.getElementById("kpi-active"),
    kpiReady: document.getElementById("kpi-ready"),
    kpiDelivered: document.getElementById("kpi-delivered"),
    kpiClients: document.getElementById("kpi-clients"),

    navItems: document.querySelectorAll(".nav-item"),
    pageContents: document.querySelectorAll(".page-content"),
    semanaLista: document.getElementById("semana-lista"),
    semanaDiaFilter: document.getElementById("semana-dia-filter"),
    semanaSemanaFilter: document.getElementById("semana-semana-filter"),
    limparEntregues: document.getElementById("limpar-entregues")
};

document.addEventListener("DOMContentLoaded", iniciar);

function iniciar() {
    dataAtual();
    verificarAcesso();
    registrarEventos();
}

function dataAtual() {
    const hoje = new Date();
    const opts = { weekday: "long", year: "numeric", month: "long", day: "numeric" };
    if (ui.currentDate) {
        ui.currentDate.textContent = hoje.toLocaleDateString("pt-BR", opts);
    }
}

function verificarAcesso() {
    const usuario = localStorage.getItem("jc_usuario");
    if (usuario) {
        try {
            estado.usuario = JSON.parse(usuario);
            mostrarTelaPrincipal();
            return;
        } catch {
            localStorage.removeItem("jc_usuario");
        }
    }

    ui.loginScreen.style.display = "grid";
}

function registrarEventos() {
    ui.loginForm.addEventListener("submit", fazerLogin);
    ui.logoutBtn.addEventListener("click", fazerLogout);
    ui.clienteForm.addEventListener("submit", criarCliente);
    ui.materialForm.addEventListener("submit", criarMaterial);
    ui.pedidoForm.addEventListener("submit", criarPedido);
    ui.adicionarItem.addEventListener("click", adicionarLinhaItemPedido);
    ui.pedidoStatusFilter?.addEventListener("change", renderizarPedidos);
    ui.semanaDiaFilter?.addEventListener("change", renderizarSemana);
    ui.semanaSemanaFilter?.addEventListener("change", renderizarSemana);
    ui.limparEntregues?.addEventListener("click", limparEntregues);

    ui.navItems.forEach(item => {
        item.addEventListener("click", () => navegarPara(item.dataset.page));
    });

    ui.pedidoItens.addEventListener("click", (e) => {
        if (e.target.classList.contains("btn-remove-item")) {
            e.target.closest(".item-row")?.remove();
        }
    });

    const pedidosLista = document.getElementById("pedidos-lista");
    pedidosLista?.addEventListener("click", async (e) => {
        if (e.target.classList.contains("btn-remove")) {
            await removerPedido(Number(e.target.dataset.removeId));
        }
    });

    pedidosLista?.addEventListener("change", async (e) => {
        if (e.target.classList.contains("status-select")) {
            const pedidoId = Number(e.target.dataset.pedidoId);
            const novoStatus = e.target.value;
            await atualizarStatusPedido(pedidoId, novoStatus);
        }
    });

    const semanaLista = document.getElementById("semana-lista");
    semanaLista?.addEventListener("click", async (e) => {
        if (e.target.classList.contains("btn-remove-mini") || e.target.closest(".btn-remove-mini")) {
            const btn = e.target.classList.contains("btn-remove-mini") ? e.target : e.target.closest(".btn-remove-mini");
            await removerPedido(Number(btn.dataset.removeId));
            await carregarSemana();
        }
    });

    const clientesLista = document.getElementById("clientes-lista");
    clientesLista?.addEventListener("click", async (e) => {
        if (e.target.classList.contains("btn-remove-item")) {
            await removerCliente(Number(e.target.dataset.removeId));
        }
    });

    const materiaisLista = document.getElementById("materiais-lista");
    materiaisLista?.addEventListener("click", async (e) => {
        if (e.target.classList.contains("btn-remove-item")) {
            await removerMaterial(Number(e.target.dataset.removeId));
        }
    });
}

function navegarPara(pagina) {
    ui.navItems.forEach(item => {
        item.classList.toggle("active", item.dataset.page === pagina);
    });

    ui.pageContents.forEach(content => {
        content.classList.toggle("active", content.id === `page-${pagina}`);
    });

    const titulos = { pedidos: "Pedidos", clientes: "Clientes", materiais: "Materiais", semana: "Semana" };
    ui.pageTitle.textContent = titulos[pagina] || "";

    if (pagina === "pedidos") {
        carregarDadosPrincipal();
    } else if (pagina === "clientes") {
        carregarClientes();
    } else if (pagina === "materiais") {
        carregarMateriais();
    } else if (pagina === "semana") {
        carregarSemana();
    }
}

async function carregarDadosPrincipal() {
    await Promise.all([carregarClientes(), carregarMateriais()]);
    if (ui.pedidoItens.children.length === 0) {
        adicionarLinhaItemPedido();
    }
    await carregarPedidos();
}

function fazerLogin(event) {
    event.preventDefault();
    ui.loginError.textContent = "";

    const username = document.getElementById("username").value.trim();
    const password = document.getElementById("password").value;

    if (username === "admin" && password === "1234") {
        const usuario = { username: "admin", nome: "Administrador" };
        localStorage.setItem("jc_usuario", JSON.stringify(usuario));
        estado.usuario = usuario;
        ui.loginForm.reset();
        mostrarTelaPrincipal();
    } else {
        ui.loginError.textContent = "Usuário ou senha incorretos";
    }
}

function fazerLogout() {
    localStorage.removeItem("jc_usuario");
    estado.usuario = null;
    ui.loginScreen.style.display = "grid";
    ui.mainScreen.style.display = "none";
}

function mostrarTelaPrincipal() {
    ui.loginScreen.style.display = "none";
    ui.mainScreen.style.display = "flex";

    if (estado.usuario) {
        ui.userName.textContent = estado.usuario.nome || estado.usuario.username;
    }

    carregarDadosPrincipal();
}

async function criarCliente(event) {
    event.preventDefault();
    limparFeedback(ui.clienteFeedback);

    try {
        await requisicaoJson("/api/clientes", "POST", {
            nome: ui.clienteNome.value.trim(),
            telefone: valorOuNulo(ui.clienteTelefone.value),
            email: valorOuNulo(ui.clienteEmail.value),
            endereco: valorOuNulo(ui.clienteEndereco.value)
        });

        ui.clienteForm.reset();
        await carregarClientes();
        mostrarFeedback(ui.clienteFeedback, "Cliente cadastrado com sucesso.", true);
    } catch (erro) {
        mostrarFeedback(ui.clienteFeedback, erro.message, false);
    }
}

async function criarMaterial(event) {
    event.preventDefault();
    limparFeedback(ui.materialFeedback);

    try {
        await requisicaoJson("/api/materiais", "POST", {
            nome: ui.materialNome.value.trim(),
            descricao: valorOuNulo(ui.materialDescricao.value)
        });

        ui.materialForm.reset();
        await carregarMateriais();
        atualizarSelectsMateriais();
        mostrarFeedback(ui.materialFeedback, "Material cadastrado com sucesso.", true);
    } catch (erro) {
        mostrarFeedback(ui.materialFeedback, erro.message, false);
    }
}

async function criarPedido(event) {
    event.preventDefault();
    limparFeedback(ui.pedidoFeedback);

    const clienteId = Number(ui.pedidoCliente.value);
    if (!clienteId) {
        mostrarFeedback(ui.pedidoFeedback, "Selecione um cliente.", false);
        return;
    }

    const itens = obterItensFormulario();
    if (itens.length === 0) {
        mostrarFeedback(ui.pedidoFeedback, "Adicione ao menos um item.", false);
        return;
    }

    try {
        await requisicaoJson("/api/pedidos", "POST", {
            clienteId,
            dataEntregaPrevista: ui.pedidoEntrega.value ? `${ui.pedidoEntrega.value}T00:00:00` : null,
            observacoes: valorOuNulo(ui.pedidoObservacoes.value),
            itens
        });

        ui.pedidoForm.reset();
        ui.pedidoItens.innerHTML = "";
        adicionarLinhaItemPedido();
        await carregarPedidos();
        mostrarFeedback(ui.pedidoFeedback, "Pedido criado com sucesso.", true);
    } catch (erro) {
        mostrarFeedback(ui.pedidoFeedback, erro.message, false);
    }
}

function adicionarLinhaItemPedido() {
    const row = document.createElement("div");
    row.className = "item-row";
    row.innerHTML = `
        <select class="item-material" required>${renderizarOpcoesMateriais()}</select>
        <input type="number" class="item-quantidade" min="1" value="1" required>
        <input type="text" class="item-observacao" placeholder="Obs">
        <button type="button" class="btn-remove-item" title="Remover">Á</button>
    `;
    ui.pedidoItens.appendChild(row);
}

function obterItensFormulario() {
    const linhas = ui.pedidoItens.querySelectorAll(".item-row");
    const itens = [];

    linhas.forEach(linha => {
        const materialId = Number(linha.querySelector(".item-material").value);
        const quantidade = Number(linha.querySelector(".item-quantidade").value);
        const observacao = linha.querySelector(".item-observacao").value;

        if (materialId && quantidade > 0) {
            itens.push({
                materialId,
                quantidade,
                observacao: valorOuNulo(observacao)
            });
        }
    });

    return itens;
}

async function atualizarStatusPedido(pedidoId, novoStatus) {
    try {
        await requisicaoJson(`/api/pedidos/${pedidoId}/status`, "PUT", { status: novoStatus });
        await carregarPedidos();
    } catch (erro) {
        alert(`Erro: ${erro.message}`);
    }
}

async function removerPedido(pedidoId) {
    if (!confirm("Tem certeza que deseja excluir este pedido? Esta ação não pode ser desfeita.")) {
        return;
    }

    try {
        await requisicaoJson(`/api/pedidos/${pedidoId}`, "DELETE");
        await carregarPedidos();
    } catch (erro) {
        alert(`Erro: ${erro.message}`);
    }
}

async function limparEntregues() {
    const entregues = estado.pedidos.filter(p => p.status === "Entregue");
    if (entregues.length === 0) {
        alert("Nenhum pedido entregue para limpar.");
        return;
    }

    if (!confirm(`Tem certeza que deseja excluir todos os ${entregues.length} pedidos entregues? Esta ação não pode ser desfeita.`)) {
        return;
    }

    try {
        for (const pedido of entregues) {
            await requisicaoJson(`/api/pedidos/${pedido.id}`, "DELETE");
        }
        await carregarPedidos();
    } catch (erro) {
        alert(`Erro: ${erro.message}`);
    }
}

async function removerCliente(id) {
    if (!confirm("Tem certeza que deseja excluir este cliente?")) {
        return;
    }

    try {
        await requisicaoJson(`/api/clientes/${id}`, "DELETE");
        await carregarClientes();
    } catch (erro) {
        alert(`Erro: ${erro.message}`);
    }
}

async function removerMaterial(id) {
    if (!confirm("Tem certeza que deseja excluir este material?")) {
        return;
    }

    try {
        await requisicaoJson(`/api/materiais/${id}`, "DELETE");
        await carregarMateriais();
    } catch (erro) {
        alert(`Erro: ${erro.message}`);
    }
}

async function carregarClientes() {
    estado.clientes = extrairItems(await requisicaoJson("/api/clientes?pageSize=100"));

    if (ui.pedidoCliente) {
        ui.pedidoCliente.innerHTML = `<option value="">Selecione</option>` +
            estado.clientes.map(c => `<option value="${c.id}">${escapeHtml(c.nome)}</option>`).join("");
    }

    const lista = document.getElementById("clientes-lista");
    const count = document.getElementById("clientes-count");
    if (count) count.textContent = estado.clientes.length;
    if (ui.kpiClients) ui.kpiClients.textContent = estado.clientes.length;

    if (!lista) return;

    if (estado.clientes.length === 0) {
        lista.innerHTML = "<p class='vazio'>Nenhum cliente cadastrado.</p>";
        return;
    }

    lista.innerHTML = estado.clientes.map(c => `
        <div class="item-card">
            <div class="item-card-content">
                <h4>${escapeHtml(c.nome)}</h4>
                <p class="item-meta">
                    ${c.telefone ? `<span>${escapeHtml(c.telefone)}</span>` : ""}
                    ${c.email ? `<span>${escapeHtml(c.email)}</span>` : ""}
                    ${c.endereco ? `<span>${escapeHtml(c.endereco)}</span>` : ""}
                </p>
            </div>
            <button type="button" class="btn-remove-item" data-remove-id="${c.id}" title="Excluir">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                </svg>
            </button>
        </div>
    `).join("");
}

async function carregarMateriais() {
    estado.materiais = extrairItems(await requisicaoJson("/api/materiais?pageSize=100"));
    atualizarSelectsMateriais();

    const lista = document.getElementById("materiais-lista");
    const count = document.getElementById("materiais-count");
    if (count) count.textContent = estado.materiais.length;

    if (!lista) return;

    if (estado.materiais.length === 0) {
        lista.innerHTML = "<p class='vazio'>Nenhum material cadastrado.</p>";
        return;
    }

    lista.innerHTML = estado.materiais.map(m => `
        <div class="item-card">
            <div class="item-card-content">
                <h4>${escapeHtml(m.nome)}</h4>
                <p>${escapeHtml(m.descricao || "Sem descrição")}</p>
            </div>
            <button type="button" class="btn-remove-item" data-remove-id="${m.id}" title="Excluir">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                </svg>
            </button>
        </div>
    `).join("");
}

function atualizarSelectsMateriais() {
    const selects = ui.pedidoItens?.querySelectorAll(".item-material");
    if (!selects) return;

    selects.forEach(select => {
        const valor = select.value;
        select.innerHTML = renderizarOpcoesMateriais(valor);
    });
}

function renderizarOpcoesMateriais(valorSelecionado = "") {
    if (estado.materiais.length === 0) {
        return "<option value=''>Cadastre materiais primeiro</option>";
    }

    return "<option value=''>Selecione</option>" +
        estado.materiais.map(m =>
            `<option value="${m.id}" ${String(m.id) === String(valorSelecionado) ? "selected" : ""}>${escapeHtml(m.nome)}</option>`
        ).join("");
}

async function carregarSemana() {
    await carregarPedidos();
    renderizarSemana();
}

function renderizarSemana() {
    const lista = ui.semanaLista;
    if (!lista) return;

    const filtroDia = ui.semanaDiaFilter?.value;
    const filtroSemana = parseInt(ui.semanaSemanaFilter?.value || "0");

    const hoje = new Date();
    const inicioSemanaAtual = new Date(hoje);
    inicioSemanaAtual.setDate(hoje.getDate() - hoje.getDay());
    inicioSemanaAtual.setHours(0, 0, 0, 0);

    const inicioSemana = new Date(inicioSemanaAtual);
    inicioSemana.setDate(inicioSemanaAtual.getDate() + (filtroSemana * 7));

    const pedidosSemana = estado.pedidos.filter(p => {
        const dataPedido = new Date(p.criadoEm);
        return dataPedido >= inicioSemana && dataPedido < new Date(inicioSemana.getTime() + 7 * 24 * 60 * 60 * 1000);
    });

    const dias = [];
    for (let i = 0; i < 7; i++) {
        const data = new Date(inicioSemana);
        data.setDate(inicioSemana.getDate() + i);
        const diaNome = ["Domingo", "Segunda-feira", "Terça-feira", "Quarta-feira", "Quinta-feira", "Sexta-feira", "Sábado"][data.getDay()];
        dias.push({ nome: diaNome, data: data, pedidos: [] });
    }

    pedidosSemana.forEach(pedido => {
        const diaPedido = new Date(pedido.criadoEm).getDay();
        const dia = dias.find(d => d.data.getDay() === diaPedido);
        if (dia) dia.pedidos.push(pedido);
    });

    const diasFiltrados = filtroDia !== undefined && filtroDia !== ""
        ? dias.filter(d => String(d.data.getDay()) === filtroDia)
        : dias;

    lista.innerHTML = diasFiltrados.map(dia => `
        <div class="semana-dia">
            <div class="semana-dia-header">
                <strong>${dia.nome}</strong>
                <span>${dia.data.toLocaleDateString("pt-BR")}</span>
                <span class="badge">${dia.pedidos.length}</span>
            </div>
            <div class="semana-pedidos">
                ${dia.pedidos.length === 0 ? "<p class='vazio'>Nenhum pedido</p>" :
                dia.pedidos.map(p => `
                    <article class="pedido-card-mini">
                        <span class="pedido-id">#${p.id}</span>
                        <span class="pedido-cliente">${escapeHtml(p.clienteNome)}</span>
                        <div class="pedido-mini-actions">
                            <span class="pedido-status status-${normalizarStatusClasse(p.status)}">${obterStatusLabel(p.status)}</span>
                            ${p.status !== "Entregue" ? `
                                <button type="button" class="btn-remove-mini" data-remove-id="${p.id}" title="Excluir">
                                    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                        <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                                    </svg>
                                </button>
                            ` : ""}
                        </div>
                    </article>
                `).join("")}
            </div>
        </div>
    `).join("");
}

async function carregarPedidos() {
    estado.pedidos = extrairItems(await requisicaoJson("/api/pedidos?pageSize=100"));
    atualizarIndicadores();
    renderizarPedidos();
}

function renderizarPedidos() {
    const lista = document.getElementById("pedidos-lista");
    const count = document.getElementById("pedidos-count");
    const filtro = ui.pedidoStatusFilter?.value || "";
    const pedidos = filtro ? estado.pedidos.filter(p => p.status === filtro) : estado.pedidos;

    if (count) count.textContent = pedidos.length;
    if (!lista) return;

    if (pedidos.length === 0) {
        lista.innerHTML = "<p class='vazio'>Nenhum pedido.</p>";
        return;
    }

    lista.innerHTML = pedidos.map(pedido => {
        const statusClass = `status-${normalizarStatusClasse(pedido.status)}`;
        const btnRemover = pedido.status !== "Entregue"
            ? `<button type="button" class="btn-remove" data-remove-id="${pedido.id}">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                    <path d="M3 6h18M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                </svg>
                Excluir
            </button>`
            : "";

        const statusPermitidos = STATUS_TRANSICOES[pedido.status] || [pedido.status];
        const opcoesStatus = STATUS_OPCOES.filter(s => statusPermitidos.includes(s.value)).map(s =>
            `<option value="${s.value}" ${s.value === pedido.status ? "selected" : ""}>${s.label}</option>`
        ).join("");

        return `
            <article class="pedido-card">
                <span class="pedido-id">#${pedido.id}</span>
                <div>
                    <div class="pedido-cliente">${escapeHtml(pedido.clienteNome)}</div>
                    <div class="pedido-meta">
                        <span>Criado: ${formatarData(pedido.criadoEm)}</span>
                        <span>${pedido.quantidadeTotalItens} itens</span>
                        ${pedido.dataEntregaPrevista ? `<span>Entrega: ${formatarData(pedido.dataEntregaPrevista)}</span>` : ""}
                    </div>
                </div>
                <div>
                    <span class="pedido-status ${statusClass}">${obterStatusLabel(pedido.status)}</span>
                    <ul class="pedido-itens">
                        ${pedido.itens.map(i => `<li>${escapeHtml(i.materialNome)} - ${i.quantidade}</li>`).join("")}
                    </ul>
                </div>
                <div class="pedido-actions">
                    <select class="status-select" data-pedido-id="${pedido.id}">
                        ${opcoesStatus}
                    </select>
                    ${btnRemover}
                </div>
            </article>
        `;
    }).join("");
}

function atualizarIndicadores() {
    const ativos = estado.pedidos.filter(p => !["Entregue", "Cancelado"].includes(p.status)).length;
    const prontos = estado.pedidos.filter(p => p.status === "Pronto").length;
    const entregues = estado.pedidos.filter(p => p.status === "Entregue").length;

    if (ui.kpiActive) ui.kpiActive.textContent = ativos;
    if (ui.kpiReady) ui.kpiReady.textContent = prontos;
    if (ui.kpiDelivered) ui.kpiDelivered.textContent = entregues;
    if (ui.kpiClients) ui.kpiClients.textContent = estado.clientes.length;
}

function extrairItems(resposta) {
    if (Array.isArray(resposta)) return resposta;
    if (Array.isArray(resposta?.items)) return resposta.items;
    return [];
}

function obterStatusLabel(status) {
    return STATUS_OPCOES.find(s => s.value === status)?.label || status;
}

function normalizarStatusClasse(status) {
    return String(status)
        .normalize("NFD")
        .replace(/[\u0300-\u036f]/g, "")
        .replace(/[^a-zA-Z0-9]/g, "")
        .toLowerCase();
}

function valorOuNulo(valor) {
    const texto = valor?.trim();
    return texto || null;
}

function limparFeedback(el) {
    el.textContent = "";
    el.className = "feedback";
}

function mostrarFeedback(el, msg, sucesso) {
    el.textContent = msg;
    el.className = `feedback ${sucesso ? "success" : "error"}`;
}

function formatarData(dataIso) {
    return new Date(dataIso).toLocaleDateString("pt-BR");
}

function escapeHtml(valor) {
    return String(valor)
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#39;");
}

async function requisicaoJson(url, method = "GET", body) {
    const opcoes = { method, headers: {} };
    if (body !== undefined) {
        opcoes.headers["Content-Type"] = "application/json";
        opcoes.body = JSON.stringify(body);
    }

    const resposta = await fetch(url, opcoes);
    if (!resposta.ok) {
        let msg = "Erro na requisição.";
        try {
            const dados = await resposta.json();
            msg = dados.message || dados.title || Object.values(dados.errors || {})[0]?.[0] || msg;
        } catch { }
        throw new Error(msg);
    }

    if (resposta.status === 204) return null;
    return resposta.json();
}

