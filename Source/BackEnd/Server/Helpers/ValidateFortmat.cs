namespace Sinco.Server.Helpers
{
	public class ValidateFortmat
	{
		public class ImportValidationException : Exception
		{
			public List<string> Errors { get; }

			public ImportValidationException(string message , List<string> errors)
				: base(message)
			{
				Errors = errors;
			}
		}
	}
}
