namespace Common.Domain.Exceptions;

public class LockAcquisitionTimeoutException : Exception
{
    public LockAcquisitionTimeoutException(string message) : base(message) { }
}