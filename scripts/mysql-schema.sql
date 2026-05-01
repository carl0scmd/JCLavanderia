-- JC Lavanderia - schema MySQL/InnoDB
-- MySQL 8.0+ | utf8mb4 | utf8mb4_0900_ai_ci

CREATE DATABASE IF NOT EXISTS jc_lavanderia
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_0900_ai_ci;

USE jc_lavanderia;

CREATE TABLE IF NOT EXISTS clientes (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    email VARCHAR(150) NULL,
    telefone VARCHAR(20) NULL,
    endereco VARCHAR(300) NULL,
    criado_em DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    INDEX idx_clientes_nome (nome),
    INDEX idx_clientes_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS materiais (
    id INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    descricao VARCHAR(300) NULL,
    criado_em DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    UNIQUE KEY uk_materiais_nome (nome)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS pedidos (
    id INT AUTO_INCREMENT PRIMARY KEY,
    cliente_id INT NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Recebido',
    criado_em DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    atualizado_em DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
    data_entrega_prevista DATETIME(6) NULL,
    observacoes VARCHAR(500) NULL,
    CONSTRAINT fk_pedidos_clientes_cliente_id
        FOREIGN KEY (cliente_id) REFERENCES clientes(id)
        ON DELETE RESTRICT,
    INDEX idx_pedidos_status (status),
    INDEX idx_pedidos_cliente_id (cliente_id),
    INDEX idx_pedidos_criado_em (criado_em)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE TABLE IF NOT EXISTS itens_pedido (
    id INT AUTO_INCREMENT PRIMARY KEY,
    pedido_id INT NOT NULL,
    material_id INT NOT NULL,
    quantidade INT NOT NULL,
    observacao VARCHAR(300) NULL,
    CONSTRAINT fk_itens_pedido_pedidos_pedido_id
        FOREIGN KEY (pedido_id) REFERENCES pedidos(id)
        ON DELETE CASCADE,
    CONSTRAINT fk_itens_pedido_materiais_material_id
        FOREIGN KEY (material_id) REFERENCES materiais(id)
        ON DELETE RESTRICT,
    INDEX idx_itens_pedido_pedido_id (pedido_id),
    INDEX idx_itens_pedido_material_id (material_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

INSERT INTO materiais (nome, descricao) VALUES
    ('Toalha de Rosto', 'Toalha padrão para rosto'),
    ('Toalha de Banho', 'Toalha padrão banho'),
    ('Lençol Solteiro', 'Lençol para cama solteiro'),
    ('Lençol Casal', 'Lençol para cama casal'),
    ('Fronha', 'Fronha de travesseiro')
ON DUPLICATE KEY UPDATE nome = VALUES(nome);
