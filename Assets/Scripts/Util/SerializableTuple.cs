using System;

/*
This class is just a way for me to be able to serialize tuples.
*/
[Serializable]
public class SerializableTuple<T1, T2>
{
    public T1 Item1;
    public T2 Item2;

    public SerializableTuple(T1 item1, T2 item2)
    {
        Item1 = item1;
        Item2 = item2;
    }

    public override string ToString()
    {
        return $"({Item1}, {Item2})";
    }
}
