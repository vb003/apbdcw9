using Tutorial9.DTOs;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<int> AddProductToWarehouse(ProductWarehouseDto productWarehouseDto);

    // Endpoint 2 - z użyciem procedury:
    Task<int> AddProductToWarehouseProc(ProductWarehouseDto productWarehouseDto);
}