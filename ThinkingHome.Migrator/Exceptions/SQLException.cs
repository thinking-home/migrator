using System;

namespace ThinkingHome.Migrator.Exceptions
{
    public class SQLException : Exception
    {
        public SQLException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}