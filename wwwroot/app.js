const STATUS_OPCOES = [
    { value: "Recebido", label: "Recebido" },
    { value: "EmProcessamento", label: "Em processamento" },
    { value: "ProntoParaEntrega", label: "Pronto para entrega" },
    { value: "Entregue", label: "Entregue" },
    { value: "Cancelado", label: "Cancelado" }
];

const estado = {
    clientes: [],
    materiais: [],
    pedidos: []
};

const ui = {
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

    pedidosLista: document.getElementById("pedidos-lista")
};

document.addEventListener("DOMContentLoaded", iniciar);

async function iniciar() {
    registrarEventos();
    await Promise.all([carregarClientes(), carregarMateriais()]);
    adicionarLinhaItemPedido();
    await carregarPedidos();
}

function registrarEventos() {
    ui.clienteForm.addEventListener("submit", criarCliente);
    ui.materialForm.addEventListener("submit", criarMaterial);
    ui.pedidoForm.addEventListener("submit", criarPedido);
    ui.adicionarItem.addEventListener("click", () => adicionarLinhaItemPedido());

    ui.pedidosLista.addEventListener("click", async (event) => {
        const alvo = event.target;
        if (!(alvo instanceof HTMLButtonElement)) {
            return;
        }

        if (alvo.classList.contains("remover-item")) {
            const row = alvo.closest(".pedido-item-row");
            row?.remove();
            return;
        }

        if (alvo.classList.contains("atualizar-status")) {
            const pedidoId = Number(alvo.dataset.pedidoId);
            const seletor = document.getElementById(`status-${pedidoId}`);
            if (!(seletor instanceof HTMLSelectElement)) {
                return;
            }

            await atualizarStatusPedido(pedidoId, seletor.value);
        }
    });

    ui.pedidoItens.addEventListener("click", (event) => {
        const alvo = event.target;
        if (alvo instanceof HTMLButtonElement && alvo.classList.contains("remover-item")) {
            const row = alvo.closest(".pedido-item-row");
            row?.remove();
        }
    });
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
        atualizarSelectsDeMateriais();
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
        mostrarFeedback(ui.pedidoFeedback, "Selecione um cliente para criar o pedido.", false);
        return;
    }

    const itens = obterItensFormulario();
    if (itens.length === 0) {
        mostrarFeedback(ui.pedidoFeedback, "Inclua ao menos um item no pedido.", false);
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

function obterItensFormulario() {
    const linhas = [...ui.pedidoItens.querySelectorAll(".pedido-item-row")];
    const itens = [];

    for (const linha of linhas) {
        const materialInput = linha.querySelector(".item-material");
        const quantidadeInput = linha.querySelector(".item-quantidade");
        const observacaoInput = linha.querySelector(".item-observacao");

        if (!(materialInput instanceof HTMLSelectElement) ||
            !(quantidadeInput instanceof HTMLInputElement) ||
            !(observacaoInput instanceof HTMLInputElement)) {
            continue;
        }

        const materialId = Number(materialInput.value);
        const quantidade = Number(quantidadeInput.value);

        if (!materialId || !quantidade || quantidade < 1) {
            continue;
        }

        itens.push({
            materialId,
            quantidade,
            observacao: valorOuNulo(observacaoInput.value)
        });
    }

    return itens;
}

function adicionarLinhaItemPedido() {
    const row = document.createElement("div");
    row.className = "pedido-item-row";
    row.innerHTML = `
        <select class="item-material" required>
            ${renderizarOpcoesMateriais()}
        </select>
        <input type="number" class="item-quantidade" min="1" value="1" required>
        <input type="text" class="item-observacao" maxlength="300" placeholder="Observação do item (opcional)">
        <button type="button" class="btn-secundario remover-item">Remover</button>
    `;

    ui.pedidoItens.appendChild(row);
}

async function atualizarStatusPedido(pedidoId, status) {
    try {
        await requisicaoJson(`/api/pedidos/${pedidoId}/status`, "PUT", { status });
        await carregarPedidos();
    } catch (erro) {
        alert(`Erro ao atualizar status: ${erro.message}`);
    }
}

async function carregarClientes() {
    estado.clientes = await requisicaoJson("/api/clientes");
    ui.pedidoCliente.innerHTML = [
        "<option value=''>Selecione um cliente</option>",
        ...estado.clientes.map(cliente =>
            `<option value="${cliente.id}">${escapeHtml(cliente.nome)}</option>`)
    ].join("");
}

async function carregarMateriais() {
    estado.materiais = await requisicaoJson("/api/materiais");
    atualizarSelectsDeMateriais();
}

function atualizarSelectsDeMateriais() {
    const selects = ui.pedidoItens.querySelectorAll(".item-material");
    for (const select of selects) {
        if (!(select instanceof HTMLSelectElement)) {
            continue;
        }

        const valorAtual = select.value;
        select.innerHTML = renderizarOpcoesMateriais(valorAtual);
    }
}

function renderizarOpcoesMateriais(valorSelecionado = "") {
    if (estado.materiais.length === 0) {
        return "<option value=''>Cadastre materiais primeiro</option>";
    }

    const opcoes = estado.materiais.map(material => {
        const selected = String(material.id) === String(valorSelecionado) ? "selected" : "";
        return `<option value="${material.id}" ${selected}>${escapeHtml(material.nome)}</option>`;
    });

    return ["<option value=''>Selecione o material</option>", ...opcoes].join("");
}

async function carregarPedidos() {
    estado.pedidos = await requisicaoJson("/api/pedidos");
    renderizarPedidos();
}

function renderizarPedidos() {
    if (estado.pedidos.length === 0) {
        ui.pedidosLista.innerHTML = "<p class='vazio'>Nenhum pedido cadastrado até o momento.</p>";
        return;
    }

    ui.pedidosLista.innerHTML = estado.pedidos.map(pedido => {
        const itensHtml = pedido.itens.map(item => `
            <li>
                <strong>${escapeHtml(item.materialNome)}</strong> — Qtd: ${item.quantidade}
                ${item.observacao ? `<br><span>${escapeHtml(item.observacao)}</span>` : ""}
            </li>
        `).join("");

        const opcoesStatus = STATUS_OPCOES.map(opcao => {
            const selected = opcao.value === pedido.status ? "selected" : "";
            return `<option value="${opcao.value}" ${selected}>${opcao.label}</option>`;
        }).join("");

        return `
            <article class="pedido-card">
                <div class="pedido-card__topo">
                    <div>
                        <div class="pedido-card__cliente">Pedido #${pedido.id} • ${escapeHtml(pedido.clienteNome)}</div>
                        <div class="pedido-card__meta">
                            Criado em ${formatarData(pedido.criadoEm)} • Total de itens: ${pedido.quantidadeTotalItens}
                        </div>
                        <div class="pedido-card__meta">
                            Entrega prevista: ${pedido.dataEntregaPrevista ? formatarData(pedido.dataEntregaPrevista) : "Não informada"}
                        </div>
                        ${pedido.observacoes ? `<div class="pedido-card__meta">Obs: ${escapeHtml(pedido.observacoes)}</div>` : ""}
                    </div>
                    <div class="pedido-card__status">
                        <select id="status-${pedido.id}">${opcoesStatus}</select>
                        <button type="button" class="btn-secundario atualizar-status" data-pedido-id="${pedido.id}">
                            Atualizar status
                        </button>
                    </div>
                </div>
                <ul class="pedido-card__itens">${itensHtml}</ul>
            </article>
        `;
    }).join("");
}

function valorOuNulo(valor) {
    const texto = valor?.trim();
    return texto ? texto : null;
}

function limparFeedback(elemento) {
    elemento.textContent = "";
    elemento.className = "feedback";
}

function mostrarFeedback(elemento, mensagem, sucesso) {
    elemento.textContent = mensagem;
    elemento.className = `feedback ${sucesso ? "sucesso" : "erro"}`;
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
        let mensagem = "Falha na requisição.";

        try {
            const dados = await resposta.json();
            if (dados.message) {
                mensagem = dados.message;
            } else if (dados.title) {
                mensagem = dados.title;
            } else if (dados.errors) {
                const primeiroErro = Object.values(dados.errors).flat()[0];
                if (primeiroErro) {
                    mensagem = primeiroErro;
                }
            }
        } catch {
            mensagem = `Erro ${resposta.status}`;
        }

        throw new Error(mensagem);
    }

    if (resposta.status === 204) {
        return null;
    }

    return resposta.json();
}
