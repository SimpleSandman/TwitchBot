namespace TwitchBot.Api.Helpers.ErrorExceptions
{
    using System.Globalization;

    // custom exception class for throwing application specific exceptions (e.g. for validation) 
    // that can be caught and handled within the application
    public class NotFoundException : Exception
    {
        public NotFoundException() : base() { }

        public NotFoundException(string message) : base(message) { }

        public NotFoundException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args)) { }
    }
}
