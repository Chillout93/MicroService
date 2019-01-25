using MassTransit;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace WarehouseService
{
    class Program
    {
        static void Main(string[] args)
        {
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start(); 

			bus.Publish(new WarehouseCreated() { WarehouseID = 1 });
			bus.Publish(new WarehouseStockCounted() { WarehouseID = 1, ProductID = 1, Quantity = 5 });
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
