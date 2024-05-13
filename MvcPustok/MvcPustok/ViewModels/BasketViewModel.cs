using System;
namespace MvcPustok.ViewModels
{
	public class BasketViewModel
	{
		
		public double Price { get; set; }
		public int Count { get; set; }
        public string Title { get; set; }
        public double TotalPrice { get; set; }
		public string Img { get; set; }
		public bool Status { get; set; }
    }
}


