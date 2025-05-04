using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;

namespace Tutorial9.Services;

public class DbService : IDbService
{
    private readonly IConfiguration _configuration;
    public DbService(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<bool> ProductExistsAsync(int ProductId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        
        command.Connection = connection;
        await connection.OpenAsync();
        
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = @"Select COUNT(1) from Products where IdProduct = @ProductId";
            command.Parameters.AddWithValue("@ProductId", ProductId);
            var result = await command.ExecuteScalarAsync();
            bool productExists = Convert.ToInt32(result) > 0;
            
            await transaction.CommitAsync();
            return productExists;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
       
    }

    public async Task<bool> WarehouseExistsAsync(int WarehouseId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.CommandText = @"Select COUNT(1) from Warehouses where IdWarehouse = @WarehouseId";
            command.Parameters.AddWithValue("@WarehouseId", WarehouseId);
            var result = await command.ExecuteScalarAsync();
            bool warehouseExists = Convert.ToInt32(result) > 0;
            await transaction.CommitAsync();
            return warehouseExists;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AmountIsValid(int Amount)
    {
        return Amount > 0;
    }
    public async Task<bool> OrderExistsAsync(int ProductId, int Amount, DateTime CreatedAt)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;

        try
        {
            command.CommandText = @"
                SELECT COUNT(1) 
                FROM [Order] 
                WHERE IdProduct = @ProductId 
                AND Amount = @Amount 
                AND CreatedAt < @CreatedAt";

            command.Parameters.AddWithValue("@ProductId", ProductId);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@CreatedAt", CreatedAt); 

            var result = await command.ExecuteScalarAsync();
            bool orderExists = Convert.ToInt32(result) > 0;
            await transaction.CommitAsync();
            return orderExists;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }


    public async Task<int> AddProductToWarehouse(int ProductId, int WarehouseId, int Amount, DateTime CreatedAt,int OrderId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            if (!await WarehouseExistsAsync(WarehouseId))
            {
                throw new Exception($"Warehouse {WarehouseId} does not exist");
            }

            if (!await ProductExistsAsync(ProductId))
            {
                throw new Exception($"Product {ProductId} does not exist");
            }

            if (!await ProductExistsAsync(ProductId))
            {
                throw new Exception($"Product {ProductId} does not exist");
            }

            if (Amount <= 0)
            {
                throw new Exception($"Amount has to be greater than 0");
            }

            decimal productPrice = await GetProductPriceAsync(ProductId);
            decimal totalPrice = productPrice * Amount;
            

            command.CommandText = @"
            INSERT INTO Product_Warehouse (ProductId, WarehouseId, Amount, Price, CreatedAt, OrderId) 
            VALUES (@ProductId, @WarehouseId, @Amount, @Price, @CreatedAt, @OrderId)";
        
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@ProductId", ProductId);
            command.Parameters.AddWithValue("@WarehouseId", WarehouseId);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@Price", totalPrice);  
            command.Parameters.AddWithValue("@CreatedAt", DateTime.UtcNow);  
            command.Parameters.AddWithValue("@OrderId", OrderId); 
            var result = await command.ExecuteNonQueryAsync();
            int generatedId = Convert.ToInt32(result);
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            return generatedId;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();  
            throw new Exception($"Failed to add product: {ex.Message}");
        }
    }

    public async Task<decimal> GetProductPriceAsync(int ProductId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        command.CommandText = @"Select Price from Products where IdProduct = @ProductId";
        command.Parameters.AddWithValue("@ProductId", ProductId);
        var result = await command.ExecuteScalarAsync();
        if (result != null && decimal.TryParse(result.ToString(), out decimal price))
        {
            return price;
        }
        throw new Exception($"Product {ProductId} does not exist");
    }
    public async Task<bool> IsOrderFulfilledAsync(int OrderId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
    
        await connection.OpenAsync();
        command.CommandText = "SELECT COUNT(1) FROM Product_Warehouse WHERE OrderId = @OrderId";
        command.Parameters.AddWithValue("@OrderId", OrderId);

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result) > 0; 
    }

    public async Task<int> AddProductToWarehouseUsingProcedure(int ProductId, int WarehouseId, int Amount, DateTime CreatedAt,
        int OrderId)
    {
        await using SqlConnection connection = new SqlConnection(_configuration.GetConnectionString("Default"));
        await using SqlCommand command = new SqlCommand();
        command.Connection = connection;
        await connection.OpenAsync();
        var transaction = await connection.BeginTransactionAsync();
        command.Transaction = transaction as SqlTransaction;
        try
        {
            command.CommandText = "AddProductToWarehouse";
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@ProductId", ProductId);
            command.Parameters.AddWithValue("@WarehouseId", WarehouseId);
            command.Parameters.AddWithValue("@Amount", Amount);
            command.Parameters.AddWithValue("@CreatedAt", CreatedAt);

            var result = await command.ExecuteScalarAsync();
            int ProductWarehouseID = Convert.ToInt32(result);

            await transaction.CommitAsync();
            return ProductWarehouseID;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception($"Failed to add product: {ex.Message}");
        }
    }

}