
using TestCodeFactoryDotNet5WebApp.Interfaces;
using System;
using Microsoft.Extensions.Logging;

namespace TestCodeFactoryDotNet5WebApp.Models
{
	public class ItemModel: IItemModel
	{
		/// <summary>
		/// Logger for all logging interactions in the class.
		/// </summary>
		private readonly ILogger _logger;
	
		public  string RequestId { get; set; }
		
		public  string RequestName { get; set; }		
		
		public bool IsEnabled { get; set; }
				
	}
}