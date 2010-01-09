using System;


namespace Taste.Common
{
    public class IllegalStateException : TasteException
    {
		public IllegalStateException() : base()
		{
		}

		public IllegalStateException(String message) : base(message)
		{
		}

		public IllegalStateException(Exception cause) : base(cause)
		{
		}

        public IllegalStateException(String message, Exception cause)
            : base(message, cause)
		{			
		}
    }
}
