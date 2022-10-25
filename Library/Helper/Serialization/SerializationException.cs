namespace Helper.Serialization;

public class SerializationException : Exception
{
    public SerializationException()
    {
    }

    public SerializationException(string msg) : base(msg)
    {
    }

    public SerializationException(string msg, Exception ex) : base(msg, ex)
    {
    }
}