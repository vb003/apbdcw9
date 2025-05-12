using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.DTOs;

namespace Tutorial9.Repositories;

public class WarehouseRepository : IWarehouseRepository
{
      //  private readonly string _connectionString = "Data Source=localhost, 1433; User=sa; Password=yourStrong(!)Password; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
      private readonly string _connectionString;
  
    public WarehouseRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default");
    }
    
    public async Task<int> AddProductToWarehouse(ProductWarehouseDto productWarehouseDto)
    {
        string command = @"insert into Product_Warehouse (IdWarehouse, IdProduct, IdOrder, Amount, Price, CreatedAt)
                            values (@IdWarehouse, @IdProduct, @IdOrder, @Amount, @Price, @CreatedAt)"; 
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@IdWarehouse", productWarehouseDto.IdWarehouse);
            cmd.Parameters.AddWithValue("@IdProduct", productWarehouseDto.IdProduct);
            cmd.Parameters.AddWithValue("@IdOrder", productWarehouseDto.IdOrder);
            cmd.Parameters.AddWithValue("@Amount", productWarehouseDto.Amount);
            cmd.Parameters.AddWithValue("@CreatedAt", productWarehouseDto.CreatedAt);
            

            await conn.OpenAsync();
            
            decimal price = await CalculatePrice(productWarehouseDto.IdProduct, productWarehouseDto.Amount);
            
            cmd.Parameters.AddWithValue("@Price", price);

            await cmd.ExecuteNonQueryAsync();
            
            int idProductWarehouse = await GetMaxIdForProductWarehouse();
            return idProductWarehouse;
        }
    }
    
    public async Task<bool> DoesProductExist(int productId)
    {
        string command = @"SELECT COUNT(*) FROM Product WHERE IdProduct = @ProductId";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@ProductId", productId);
            
            await conn.OpenAsync();
            
            int count = (int) await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }

    public async Task<bool> DoesWarehouseExist(int warehouseId)
    {
        string command = @"SELECT COUNT(*) FROM Warehouse WHERE IdWarehouse = @warehouseId";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@warehouseId", warehouseId);
            
            await conn.OpenAsync();
            
            int count = (int) await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }

    public async Task<int> DoesOrderExist(int productId, int amount, DateTime createdAt)
    {
        string command = @"SELECT IdOrder FROM ""Order""
                WHERE IdProduct = @ProductId AND Amount = @Amount
                AND CreatedAt < @CreatedAt";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@productId", productId);
            cmd.Parameters.AddWithValue("@Amount", amount);
            cmd.Parameters.AddWithValue("@CreatedAt", createdAt);
            
            await conn.OpenAsync();
            
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    int idOrder = reader.GetInt32(reader.GetOrdinal("IdOrder"));
                    return idOrder;
                }

                return -1;
            }
        }
    }

    public async Task<bool> IsOrderFulfilled(int orderId)
    {
        string command = @"SELECT COUNT(*) FROM Product_Warehouse 
                WHERE IdOrder = @orderId";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@orderId", orderId);
            
            await conn.OpenAsync();
            
            int count = (int) await cmd.ExecuteScalarAsync();
            return count > 0;
        }
    }

    public async Task<bool> FulfillOrder(int orderId)
    {
        string command = @"UPDATE ""Order"" SET FulfilledAt = @FulfilledAt WHERE IdOrder = @orderId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@orderId", orderId);
            cmd.Parameters.AddWithValue("@FulfilledAt", DateTime.Now);
            
            await conn.OpenAsync();
            
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
    }

    public async Task<decimal> CalculatePrice(int productId, int amount)
    {
        string command = @"SELECT Price FROM Product WHERE IdProduct = @productId";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("@productId", productId);
            
            await conn.OpenAsync();
            
            decimal price = (decimal) await cmd.ExecuteScalarAsync();
            
            return price * amount;
        }
    }

    private async Task<int> GetMaxIdForProductWarehouse()
    {
        string command = @"SELECT MAX(IdProductWarehouse) FROM Product_Warehouse";
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();
            var id = (int)await cmd.ExecuteScalarAsync();
            return id;
        }
    }
    
    // ---
    // Metoda do endpointu 2 - z użyciem procedury składowanej:
    public async Task<int> AddProductToWarehouseProc(ProductWarehouseDto productWarehouseDto)
    {
        await using SqlConnection connection = new SqlConnection(_connectionString);
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();

        DbTransaction transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        // BEGIN TRANSACTION
        try
        {
            command.CommandText = @"EXECUTE AddProductToWarehouse @IdProduct, @IdWarehouse, @Amount, @CreatedAt;";
            
            command.Parameters.AddWithValue("@IdProduct",productWarehouseDto.IdProduct);
            command.Parameters.AddWithValue("@IdWarehouse", productWarehouseDto.IdWarehouse);
            command.Parameters.AddWithValue("@Amount", productWarehouseDto.Amount);
            command.Parameters.AddWithValue("@CreatedAt", productWarehouseDto.CreatedAt);
            
            await command.ExecuteNonQueryAsync();
            
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
        int idProductWarehouse = await GetMaxIdForProductWarehouse();
        return idProductWarehouse;
    }
}