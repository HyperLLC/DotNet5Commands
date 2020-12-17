using TestCodeFactoryDotNet5WebApp.Interfaces;
using System;
using Microsoft.Extensions.Logging;
using CommonDeliveryFramework;
using System.Threading.Tasks;

namespace TestCodeFactoryDotNet5WebApp.Models
{
	public class SampleModel: ISampleModel
	{
		/// <summary>
		/// Logger for all logging interactions in the class.
		/// </summary>
		private readonly ILogger _logger;
	
		public  string RequestId { get; set; }
		
		public  string RequestName { get; set; }		
	}
}