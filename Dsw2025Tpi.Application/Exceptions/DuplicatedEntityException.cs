namespace Dsw2025Tpi.Application.Exceptions
{
    [Serializable]
    public class DuplicatedEntityException : Exception
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