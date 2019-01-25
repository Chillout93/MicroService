using MassTransit;
using System;
using System.Collections.Generic;

namespace ProductService
{
    class Program
    {
        static void Main(string[] args)
        {
			var products = new List<Product> { new Product() { ID = 1, Barcode = "CoolChair" }, new Product() { ID = 2, Barcode = "CoolJacket" } };

			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start(); 

			foreach (var product in products) {
				bus.Publish(new ProductCreated { ProductID = product.ID, BranchCodes = new List<string> { "MAIN" } });
			}

		}
    }

	public class Product {
		public int ID { get; set; }
		public string Barcode { get; set; }
	}

	public class ProductCreated : IProductCreated {
		public int ProductID { get; set; }
		public IList<string> BranchCodes { get; set; }
	}

	public interface IProductCreated {
		int ProductID { get; }
		IList<string> BranchCodes { get; set; }
	}
}
