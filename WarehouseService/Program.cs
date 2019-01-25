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
			var warehouses = new List<Warehouse> { new Warehouse() { ID = 1, Code = "WarehouseA" }, new Warehouse() { ID = 2, Code = "WarehouseB" } };

			// raise warehouse created event
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start(); // This is important!

			bus.Publish(new WarehouseCreated() { WarehouseID = 1 });
			// raise warehouse stock count
		}
	}

	public interface IWarehouseCreated {
		int WarehouseID { get; }
	}

	public class WarehouseCreated : IWarehouseCreated {
		public int WarehouseID { get; set; }
	}

	public class Warehouse {
		public int ID { get; set; }
		public string Code { get; set; }
	}
}
