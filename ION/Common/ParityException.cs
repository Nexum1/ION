using System;

public class ParityException : Exception
{
    public ParityException()
      : base("The parity check failed on deserialization")
    {
    }
}