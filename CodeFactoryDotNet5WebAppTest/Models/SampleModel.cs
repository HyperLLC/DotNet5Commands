using System;
using TestCodeFactoryDotNet5WebApp.Interfaces;
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
	
		public string RequestId { get; set; }
		
		public string RequestName { get; set; }
		
		public string RequestTitle { get; set; }

		
		public void GetFlowRate(string testName, string description, int testNumber)
		{
			_logger.InformationEnterLog();
		
			if(string.IsNullOrEmpty(testName))
			{
				_logger.ErrorLog($"The parameter {nameof(testName)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(testName));
			}
		
			if(string.IsNullOrEmpty(description))
			{
				_logger.ErrorLog($"The parameter {nameof(description)} was not provided. Will raise an argument exception");
				_logger.InformationExitLog();
				throw new ValidationException(nameof(description));
			}
		
			try
			{
				//test
			}
			catch (ManagedException)
			{
				//Throwing the managed exception. Override this logic if you have logic in this method to handle managed errors.
				throw;
			}
			catch (Exception unhandledException)
			{
				_logger.ErrorLog("An unhandled exception occured, see the exception for details. Will throw a UnhandledException", unhandledException);
				_logger.InformationExitLog();
				throw new UnhandledException();
			}
		
			_logger.InformationExitLog();
			throw new NotImplementedException();
		}		
	}
}