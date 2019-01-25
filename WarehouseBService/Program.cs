using MassTransit;
using System;

namespace WarehouseBService
{
    class Program
    {
		static void Main(string[] args) {
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start();

			bus.Publish(new WarehouseCreated() { WarehouseID = 2 });
			bus.Publish(new WarehouseStockCounted() { ProductID = 1, WarehouseID = 2, Quantity = 5 });
		}
    }

	public class Warehouse {
		public int ID { get; set; }
		public string Code { get; set; }
	}

	public class WarehouseStockCounted : IWarehouseStockCounted {
		public int WarehouseID { get; set; }

		public int ProductID { get; set; }

		public int Quantity { get; set; }
	}

	public class WarehouseCreated : IWarehouseCreated {
		public int WarehouseID { get; set; }
	}

	public interface IWarehouseCreated {
		int WarehouseID { get; }
	}

	public interface IWarehouseStockCounted {
		int WarehouseID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}
}
