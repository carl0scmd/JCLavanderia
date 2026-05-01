# JCLavanderia.Pedidos

ASP.NET Core 10 SPA com backend MySQL para gestão de pedidos de lavanderia.

## Run

```bash
dotnet run
```

Servidor de desenvolvimento:

- HTTP: `http://localhost:5185`
- HTTPS: `https://localhost:7122`

O banco MySQL `jc_lavanderia` deve existir antes da primeira execução. Use `scripts/mysql-schema.sql` ou crie o banco manualmente; o EF Core usa `EnsureCreated()` para criar tabelas ausentes a partir do modelo.

## API Routes

- `GET /api/pedidos` - Listar pedidos
- `GET /api/pedidos/{id}` - Obter pedido
- `POST /api/pedidos` - Criar pedido
- `PUT /api/pedidos/{id}/status` - Atualizar status (body: `{ "status": "Recebido"|"EmAndamento"|"Pronto"|"Entregue"|"Cancelado" }`)
- Rotas equivalentes para `/api/clientes` e `/api/materiais`

## Project Structure

| Directory | Purpose |
|-----------|---------|
| `Controllers/` | Endpoints API (Pedidos, Clientes, Materiais) |
| `Models/` | Entidades EF Core |
| `DTOs/` | DTOs de entrada e saída |
| `Data/` | `AppDbContext` configurado para MySQL |
| `wwwroot/` | SPA frontend estática |

## Key Patterns

- Primary constructor injection (e.g., `Controller(AppDbContext context)`)
- JSON enums serializados como strings (`[JsonConverter(typeof(JsonStringEnumConverter))]`)
- EF Core navigation properties: Pedido -> Cliente, Pedido -> PedidoItem -> Material
- PedidoStatus enum: `Recebido`, `EmAndamento`, `Pronto`, `Entregue`, `Cancelado`

## Database

MySQL 8.0+ com InnoDB e `utf8mb4_0900_ai_ci`.

Tabelas principais:

- `clientes`
- `materiais`
- `pedidos`
- `itens_pedido`

Não há testes configurados.
