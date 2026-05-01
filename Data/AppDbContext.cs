using JCLavanderia.Pedidos.Models;
using Microsoft.EntityFrameworkCore;

namespace JCLavanderia.Pedidos.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Material> Materiais => Set<Material>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();

    public override int SaveChanges()
    {
        AtualizarTimestampsPedidos();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        AtualizarTimestampsPedidos();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci");

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.ToTable("clientes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
            entity.Property(e => e.Telefone).HasColumnName("telefone").HasMaxLength(20);
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150);
            entity.Property(e => e.Endereco).HasColumnName("endereco").HasMaxLength(300);
            entity.Property(e => e.CriadoEm)
                .HasColumnName("criado_em")
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(e => e.Nome).HasDatabaseName("idx_clientes_nome");
            entity.HasIndex(e => e.Email).HasDatabaseName("idx_clientes_email");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.ToTable("materiais");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descricao).HasColumnName("descricao").HasMaxLength(300);
            entity.Property(e => e.CriadoEm)
                .HasColumnName("criado_em")
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.HasIndex(e => e.Nome).IsUnique().HasDatabaseName("uk_materiais_nome");
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("pedidos");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(e => e.Observacoes).HasColumnName("observacoes").HasMaxLength(500);
            entity.Property(e => e.CriadoEm)
                .HasColumnName("criado_em")
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.AtualizadoEm)
                .HasColumnName("atualizado_em")
                .HasColumnType("datetime(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            entity.Property(e => e.DataEntregaPrevista)
                .HasColumnName("data_entrega_prevista")
                .HasColumnType("datetime(6)");
            entity.HasIndex(e => e.Status).HasDatabaseName("idx_pedidos_status");
            entity.HasIndex(e => e.ClienteId).HasDatabaseName("idx_pedidos_cliente_id");
            entity.HasIndex(e => e.CriadoEm).HasDatabaseName("idx_pedidos_criado_em");

            entity.HasOne(e => e.Cliente)
                .WithMany(c => c.Pedidos)
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PedidoItem>(entity =>
        {
            entity.ToTable("itens_pedido");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.PedidoId).HasColumnName("pedido_id");
            entity.Property(e => e.MaterialId).HasColumnName("material_id");
            entity.Property(e => e.Quantidade).HasColumnName("quantidade").IsRequired();
            entity.Property(e => e.Observacao).HasColumnName("observacao").HasMaxLength(300);
            entity.HasIndex(e => e.PedidoId).HasDatabaseName("idx_itens_pedido_pedido_id");
            entity.HasIndex(e => e.MaterialId).HasDatabaseName("idx_itens_pedido_material_id");

            entity.HasOne(e => e.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(e => e.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Material)
                .WithMany(m => m.PedidoItens)
                .HasForeignKey(e => e.MaterialId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void AtualizarTimestampsPedidos()
    {
        var agora = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<Pedido>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CriadoEm = agora;
                entry.Entity.AtualizadoEm = agora;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.AtualizadoEm = agora;
            }
        }
    }
}
