public interface IStatefulChat <T>
{
    public T Chat(T input);
}

public record ChatPayload(string Text);