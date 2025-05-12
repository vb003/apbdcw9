using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.DTOs;
using Tutorial9.Repositories;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;

    public WarehouseService(IWarehouseRepository warehouseRepository)
    {
        _warehouseRepository = warehouseRepository;
    }
    public async Task<int> AddProductToWarehouse(ProductWarehouseDto productWarehouseDto)
    {
        if (await _warehouseRepository.DoesProductExist(productWarehouseDto.IdProduct) == false)
        {
            throw new InvalidOperationException("Product not found");
        }

        if (await _warehouseRepository.DoesWarehouseExist(productWarehouseDto.IdWarehouse) == false)
        {
            throw new InvalidOperationException("Warehouse not found");
        }

        var orderId = await _warehouseRepository.DoesOrderExist(productWarehouseDto.IdProduct, productWarehouseDto.Amount,
            productWarehouseDto.CreatedAt);

        if (orderId <= 0)
        {
            throw new InvalidOperationException("Order not found");
        }
        productWarehouseDto.IdOrder = orderId;
        
        if (await _warehouseRepository.IsOrderFulfilled(orderId))
        {
            throw new InvalidOperationException("Order is already fulfilled");
        }

        if (await _warehouseRepository.FulfillOrder(orderId) == false)
        {
            throw new InvalidOperationException("Order couldnt be fulfilled");
        }
        
        int idProductWarehouse = await _warehouseRepository.AddProductToWarehouse(productWarehouseDto);
        return idProductWarehouse;
    }
    
    // Metoda do endpointu 2 - z użyciem procedury składowanej:
    public async Task<int> AddProductToWarehouseProc(ProductWarehouseDto productWarehouseDto)
    {
        int idProductWarehouse = await _warehouseRepository.AddProductToWarehouseProc(productWarehouseDto);
        return idProductWarehouse;
    }
}