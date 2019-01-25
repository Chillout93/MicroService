using MassTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationService
{
    class Program
    {
        static void Main(string[] args)
        {
			var branches = new List<Branch>();
			var products = new List<Product>();
			var warehouses = new List<Warehouse>();
			var productWarehouses = new List<ProductWarehouse>();

			// consume branches
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});

				sbc.ReceiveEndpoint(host, "AllocationService_IBranchCreated", ep => {
					ep.Handler<IBranchCreated>(context => {
						Console.WriteLine($"Received branch: {JsonConvert.SerializeObject(context.Message)}");
						branches.Add(new Branch() { ID = context.Message.BranchID, WarehouseID = context.Message.WarehouseID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "AllocationService_IProductCreated", ep => {
					ep.Handler<IProductCreated>(context => {
						Console.WriteLine($"Received product: {JsonConvert.SerializeObject(context.Message)}");
						products.Add(new Product() { ID = context.Message.ProductID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "AllocationService_IWarehouseCreated", ep => {
						ep.Handler<IWarehouseCreated>(context => {
						Console.WriteLine($"Received product: {JsonConvert.SerializeObject(context.Message)}");
						warehouses.Add(new Warehouse() { ID = context.Message.WarehouseID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "AllocationService_IWarehouseStockUpdated", ep => {
					ep.Handler<IWarehouseStockUpdated>(context => {
						Console.WriteLine($"Received product: {JsonConvert.SerializeObject(context.Message)}");
						productWarehouses.Add(new ProductWarehouse() { ProductID = context.Message.ProductID, WarehouseID = context.Message.WarehouseID, SoftQuantity = context.Message.Quantity });

						return Task.CompletedTask;
					});
				});
			});

			bus.Start(); // This is important!

			// Simulate receiving an allocation, here we would check that the Warehouse and Branch received were actually valid based on the created messages consumed.
			var allocationRecieved = new AllocationCreated() { WarhouseID = 1, BranchID = 1, ProductID = 1  };
			var warehouseStock = productWarehouses.First(x => x.ProductID == allocationRecieved.ProductID && x.WarehouseID == allocationRecieved.WarhouseID);
			
			if (warehouseStock.SoftQuantity > 0) {
				bus.Publish(allocationRecieved);
			}

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}
    }

	public class Warehouse {
		public int ID { get; set; }
		public string Code { get; set; }
	}

	public class Branch {
		public int ID { get; set; }
		public int WarehouseID { get; set; }
	}

	public class Product {
		public int ID { get; set; }
		public string Barcode { get; set; }
	}

	public class ProductWarehouse {
		public int ProductID { get; set; }
		public int WarehouseID { get; set; }
		public int SoftQuantity { get; set; }
		public int HardQuantity { get; set; }
	}

	public class AllocationCreated : IAllocationCreated {
		public int WarhouseID { get; set; }
		public int BranchID { get; set; }
		public int ProductID { get; set; }
	}

	public class WarehouseStockUpdated : IWarehouseStockUpdated {
		public int WarehouseID { get; set; }

		public int ProductID { get; set; }

		public int Quantity { get; set; }
	}

	public interface IAllocationCreated {
		int WarhouseID { get; }
		int BranchID { get; }
		int ProductID { get;  }
	}

	public interface IWarehouseStockUpdated {
		int WarehouseID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}

	public interface IBranchStockUpdated {
		int BranchID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}

	public interface IWarehouseCreated {
		int WarehouseID { get; }
	}

	public interface IProductCreated {
		int ProductID { get; }
	}

	public interface IBranchCreated {
		int BranchID { get; }
		int WarehouseID { get; }
	}
}
