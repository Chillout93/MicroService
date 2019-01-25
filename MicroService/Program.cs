﻿using MassTransit;
using System;
using System.Collections.Generic;

namespace MicroService
{
    class Program
    {
		static void Main(string[] args) {
			var branches = new List<Branch> { new Branch() { ID = 1, Code = "BranchA" }, new Branch() { ID = 2, Code = "BranchB" } };

			// raise branch created message
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start(); // This is important!

			foreach (var branch in branches) {
				bus.Publish(new BranchCreated { BranchID = branch.ID, WarehouseID = branch.WarehouseID });
				bus.Publish(new BranchStockCounted() { BranchID = branch.ID, ProductID = 1, Quantity = 5 });
			}
		}
    }

	public interface IBranchStockCounted {
		int BranchID { get; }
		int ProductID { get; }
		int Quantity { get; }
	}

	public class BranchStockCounted : IBranchStockCounted {
		public int BranchID { get; set; }

		public int ProductID { get; set; }

		public int Quantity { get; set; }
	}

	public interface IBranchCreated {
		int BranchID { get; }
		int WarehouseID { get; }
	}

	public class BranchCreated : IBranchCreated {
		public int BranchID {get;set;}
		public int WarehouseID { get; set; }
	}

	public class Branch {
		public int ID { get; set; }
		public string Code { get; set; }
		public int WarehouseID { get; set; }
	}
}
