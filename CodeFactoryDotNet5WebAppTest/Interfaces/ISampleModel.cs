namespace TestCodeFactoryDotNet5WebApp.Interfaces
{
	public interface ISampleModel
	{	
		string RequestId { get; set; }
		
		string RequestName { get; set; }

		string RequestTitle { get; set; }

		void GetFlowRate(string testName, string description, int testNumber);
	}
}