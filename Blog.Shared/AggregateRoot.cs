namespace Blog.Shared;

public abstract class AggregateRoot<T>
{
    /// <summary>
    /// id
    /// </summary>
    public T Id { get;protected set; }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if(obj == null) return false;
        var that = obj as AggregateRoot<T>;
        return Id.Equals(that.Id);
    }

    public virtual void SetId(T id)
    {
        Id = id;
    }
    
    /*public static bool operator ==(AggregateRoot<T> left, AggregateRoot<T> right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(AggregateRoot<T> left, AggregateRoot<T> right)
    {
        return !(left == right);
    }*/
}