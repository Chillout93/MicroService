﻿using MassTransit;
using System;
using System.Collections.Generic;

namespace ProductService
{
    class Program
    {
        static void Main(string[] args)
        {
			var products = new List<Product> { new Product() { ID = 1, Barcode = "CoolChair" }, new Product() { ID = 2, Barcode = "CoolJacket" } };
			// raise branch created message
			var bus = Bus.Factory.CreateUsingRabbitMq(sbc => {
				var host = sbc.Host(new Uri("rabbitmq://mwapp-dev:5672/test"), h => {
					h.Username("admin");
					h.Password("admin");
				});
			});

			bus.Start(); // This is important!

			foreach (var product in products) {
				bus.Publish(new ProductCreated { ProductID = product.ID });
			}

		}
    }

	public interface IProductCreated {
		int ProductID { get; }
	}

	public class ProductCreated : IProductCreated {
		public int ProductID { get; set; }
	}


	public class Product {
		public int ID { get; set; }
		public string Barcode { get; set; }
	}
}