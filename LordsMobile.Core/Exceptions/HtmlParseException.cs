using System;

namespace LordsMobile.Core.Exceptions
{
    /// <summary>
    /// HtmlParseException.
    /// </summary>
    [Serializable]
    public class HtmlParseException : Exception
    {
        /// <inheritdoc />
        public HtmlParseException(string message) : base(message)
        {
        }
    }
}
