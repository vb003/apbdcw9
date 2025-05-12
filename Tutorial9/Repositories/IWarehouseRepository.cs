using Tutorial9.DTOs;

namespace Tutorial9.Repositories;

public interface IWarehouseRepository
{
    Task<int> AddProductToWarehouse(ProductWarehouseDto productWarehouseDto);
    Task<bool> DoesProductExist(int productId);
    Task<bool> DoesWarehouseExist(int warehouseId);
    Task<int> DoesOrderExist(int productId, int amount, DateTime createdAt);
    Task<bool> IsOrderFulfilled(int orderId);
    Task<bool> FulfillOrder(int orderId);
    Task<decimal> CalculatePrice(int productId, int amount);
    
    // Endpoint 2 - z użyciem procedury:
    Task<int> AddProductToWarehouseProc(ProductWarehouseDto productWarehouseDto);
    
}