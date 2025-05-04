namespace Tutorial9.Services;

public interface IDbService
{
    Task<bool> ProductExistsAsync(int ProductId);
    Task<bool> WarehouseExistsAsync(int WarehouseId);
    Task<bool> AmountIsValid(int Amount);
    Task<bool> OrderExistsAsync(int ProductId, int Amount, DateTime CreatedAt);
    Task<int> AddProductToWarehouse(int ProductId, int WarehouseId, int Amount,DateTime CreatedAt,int OrderId);
    Task<Decimal> GetProductPriceAsync(int ProductId);
    Task<bool> IsOrderFulfilledAsync(int OrderId);
    Task<int> AddProductToWarehouseUsingProcedure(int ProductId, int WarehouseId, int Amount, DateTime CreatedAt, int OrderId);

}