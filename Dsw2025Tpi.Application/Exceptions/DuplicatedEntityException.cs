namespace Dsw2025Tpi.Application.Exceptions
{
    [Serializable]
    internal class DuplicatedEntityException : Exception
    {
        public DuplicatedEntityException()
        {
        }

        public DuplicatedEntityException(string? message) : base(message)
        {
        }

        public DuplicatedEntityException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}