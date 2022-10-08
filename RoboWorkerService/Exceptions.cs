namespace RoboWorkerService;

public class BussinesExceptions : Exception
{
    public BussinesExceptions(string msg) : base(msg)
    { }
    public BussinesExceptions(string msg, Exception ex) : base(msg, ex)
    { }


}

