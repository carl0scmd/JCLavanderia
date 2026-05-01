# PRD - Sistema de Gestão de Pedidos

**Empresa:** JC Lavanderia (Industrial e Hospitalar)  
**Versão:** 1.1 (Atualizado com MySQL obrigatório)

## 1. Objetivo

Desenvolver um sistema para gerenciar pedidos de coleta e entrega, garantindo controle, rastreabilidade e organização das operações.

## 2. Problema

Falta de controle centralizado de pedidos, dificultando:

- Acompanhamento de status
- Organização de clientes
- Gestão de itens

## 3. Usuários

- Operadores internos
- Administradores

## 4. Funcionalidades

### Clientes

- Cadastrar cliente
- Listar clientes
- Editar cliente

### Pedidos

- Criar pedido vinculado a cliente
- Adicionar itens ao pedido
- Listar pedidos

### Status

- Atualizar status do pedido

## 5. Requisitos Técnicos

- **Backend:** C#
- **Banco de Dados:** **MySQL (obrigatório)**
- **Arquitetura:** API REST
- **ORM:** Entity Framework Core

## 6. Modelagem do Banco de Dados (MySQL)

### Tabela: clientes

```sql
CREATE TABLE clientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150),
    telefone VARCHAR(20),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Tabela: pedidos

```sql
CREATE TABLE pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    status VARCHAR(50) NOT NULL,
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP,
    atualizado_em DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (cliente_id) REFERENCES clientes(id)
);
```

### Tabela: materiais

```sql
CREATE TABLE materiais (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    descricao VARCHAR(300),
    criado_em DATETIME DEFAULT CURRENT_TIMESTAMP
);
```

### Tabela: itens_pedido

```sql
CREATE TABLE itens_pedido (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    material_id INT NOT NULL,
    quantidade INT NOT NULL,
    observacao VARCHAR(300),
    FOREIGN KEY (pedido_id) REFERENCES pedidos(id),
    FOREIGN KEY (material_id) REFERENCES materiais(id)
);
```

## 7. Requisitos Funcionais

| ID | Descrição |
|----|-----------|
| RF01 | Cadastrar cliente |
| RF02 | Criar pedido |
| RF03 | Vincular pedido a cliente |
| RF04 | Adicionar itens ao pedido |
| RF05 | Atualizar status |
| RF06 | Listar pedidos |

## 8. Requisitos Não Funcionais

- API organizada em camadas
- Código limpo
- Uso de Git
- Documentação clara (README)
- Facilidade de execução

## 9. Prazo

- 14 dias

## 10. Entrega

- Repositório Git
- Script SQL do banco
- Instruções para rodar

## 11. Critérios de Avaliação

- Organização
- Lógica
- Clareza
- Estrutura

## 12. Melhorias Opcionais

- Enum para status
- Soft delete
- Paginação nas listagens
- Filtros por status

## 13. Sugestão de Status

```text
CRIADO
EM_COLETA
EM_PROCESSAMENTO
EM_ENTREGA
FINALIZADO
```
