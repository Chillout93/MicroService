using MassTransit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace InventoryService
{
    class Program
    {
        static void Main(string[] args)
        {
			var branches = new List<Branch>();
			var products = new List<Product>();
			var warehouses = new List<Warehouse>();
			var productBranches = new List<ProductBranch>();
			var productWarehouses = new List<ProductWarehouse>();

			// consume branches
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
			{
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h =>
				{
					h.Username("admin");
					h.Password("admin");
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IBranchCreated ", ep =>
				{
					ep.Handler<IBranchCreated>(context =>
					{
						Console.WriteLine($"Received branch: {JsonConvert.SerializeObject(context.Message)}");
						branches.Add(new Branch() { ID = context.Message.BranchID, WarehouseID = context.Message.WarehouseID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IProductCreated", ep =>
					{
						ep.Handler<IProductCreated>(context => {
						Console.WriteLine($"Received product: {JsonConvert.SerializeObject(context.Message)}");
						products.Add(new Product() { ID = context.Message.ProductID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IWarehouseCreated", ep =>
				{
					ep.Handler<IWarehouseCreated>(context => {
						Console.WriteLine($"Received warehouse: {JsonConvert.SerializeObject(context.Message)}");
						warehouses.Add(new Warehouse() { ID = context.Message.WarehouseID });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IAllocationCreated", ep => {
					ep.Handler<IAllocationCreated>(context => {
						Console.WriteLine($"Received warehouse: {JsonConvert.SerializeObject(context.Message)}");
						var productWarehouse = productWarehouses.FirstOrDefault(x => x.ProductID == context.Message.ProductID && x.WarehouseID == context.Message.WarhouseID);
						var branchWarehouse = productBranches.FirstOrDefault(x => x.BranchID == context.Message.BranchID && x.ProductID == context.Message.ProductID);
						if (productWarehouse != null && branchWarehouse != null) {
							productWarehouse.SoftQuantity -= 1;
							branchWarehouse.SoftQuantity += 1;
						}

						//bus.Publish(new WarehouseStockUpdated() { ProductID = productWarehouse.ProductID, WarehouseID = productWarehouse.WarehouseID, Quantity = productWarehouse.SoftQuantity });
						//bus.Publish(new BranchStockUpdated() { ProductID = branchWarehouse.ProductID, BranchID = branchWarehouse.BranchID, Quantity = branchWarehouse.SoftQuantity });

						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IWarehouseStockCounted", ep =>
				{
					ep.Handler<IWarehouseStockCounted>(context => {
						Console.WriteLine($"Received warehouse count: {JsonConvert.SerializeObject(context.Message)}");
						var productWarhouse = productWarehouses.FirstOrDefault(x => x.WarehouseID == context.Message.WarehouseID && x.ProductID == context.Message.ProductID);
						if (productWarhouse != null) productWarehouses.Remove(productWarhouse);

						productWarhouse = new ProductWarehouse() { WarehouseID = context.Message.WarehouseID, ProductID = context.Message.ProductID, SoftQuantity = context.Message.Quantity };
						productWarehouses.Add(productWarhouse);

						//bus.Publish(new WarehouseStockUpdated() { WarehouseID = productWarhouse.WarehouseID, ProductID = productWarhouse.ProductID, Quantity = productWarhouse.SoftQuantity });
						return Task.CompletedTask;
					});
				});

				sbc.ReceiveEndpoint(host, "InventoryService_IBranchStockCounted ", ep =>
				{
					ep.Handler<IBranchStockCounted>(context => {
						Console.WriteLine($"Received warehouse count: {JsonConvert.SerializeObject(context.Message)}");
						var productBranch = productBranches.FirstOrDefault(x => x.BranchID == context.Message.BranchID && x.ProductID == context.Message.ProductID);
						if (productBranch != null)
							productBranches.Remove(productBranch);

						productBranch = new ProductBranch() { BranchID = context.Message.BranchID, ProductID = context.Message.ProductID, SoftQuantity = context.Message.Quantity };
						productBranches.Add(productBranch);

						//bus.Publish(new BranchStockUpdated() { BranchID = productBranch.BranchID, ProductID = productBranch.ProductID, Quantity = productBranch.SoftQuantity });
						return Task.CompletedTask;
					});
				});
			});

			bus.Start(); 

			Console.WriteLine("Press any key to exit");
			Console.ReadKey();
		}
    }

	public class Warehouse {
		public int ID { get; set; }
		public string Code { get; set; }

		public IList<ProductWarehouse> WarehouseStock { get; set; }
	}

	public class Product {
		public int ID { get; set; }
		public string Barcode { get; set; }
	}

	public class Branch {
		public int ID { get; set; }
		public int WarehouseID { get; set; }
	}

	public class ProductBranch {
		public int ProductID { get; set; }
		public int BranchID { get; set; }
		public int SoftQuantity { get; set; }
		public int HardQuantity { get; set; }
	}

	public class ProductWarehouse {
		public int ProductID { get; set; }
		public int WarehouseID { get; set; }
		public int SoftQuantity { get; set; }
		public int HardQuantity { get; set; }
	}

	public class WarehouseStockUpdated : IWarehouseStockUpdated {
		public int WarehouseID { get; set; }

		public int ProductID { get; set; }

		public int Quantity { get; set; }
	}

	public class BranchStockUpdated : IBranchStockUpdated {
		public int BranchID { get; set; }
		public int ProductID { get; set; }
		public int Quantity { get; set; }
	}

	public interface IBranchStockCounted {
		int BranchID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}

	public interface IWarehouseStockCounted {
		int WarehouseID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}

	public interface IAllocationCreated {
		int WarhouseID { get; }
		int BranchID { get; }
		int ProductID { get; }
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
