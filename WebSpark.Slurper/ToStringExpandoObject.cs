using System.Collections.Generic;
using System.Dynamic;

namespace WebSpark.Slurper;

/// <summary>
/// Delegate for ToString method implementation
/// </summary>
public delegate string ToStringFunc();

/// <summary>
/// An extension of DynamicObject that provides string conversion and value conversion capabilities
/// </summary>
public sealed class ToStringExpandoObject : DynamicObject
{
    /// <summary>
    /// Gets the dictionary of members for this dynamic object
    /// </summary>
    public IDictionary<string, object> Members { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ToStringExpandoObject"/> class
    /// </summary>
    public ToStringExpandoObject()
    {
        this.Members = new Dictionary<string, object>();
    }

    /// <summary>
    /// Tries to delete a member from the dynamic object
    /// </summary>
    /// <param name="binder">The binder providing the name of the member to delete</param>
    /// <returns>True if the member was successfully deleted; otherwise, false</returns>
    public override bool TryDeleteMember(DeleteMemberBinder binder)
    {
        return this.Members.Remove(binder.Name);
    }

    /// <summary>
    /// Tries to get the value of a member in the dynamic object
    /// </summary>
    /// <param name="binder">The binder providing the name of the member to get</param>
    /// <param name="result">When this method returns, contains the value of the specified member, if found</param>
    /// <returns>True if the member was found; otherwise, false</returns>
    public override bool TryGetMember(GetMemberBinder binder, out object result)
    {
        return this.Members.TryGetValue(binder.Name, out result);
    }

    /// <summary>
    /// Tries to set the value of a member in the dynamic object
    /// </summary>
    /// <param name="binder">The binder providing the name of the member to set</param>
    /// <param name="value">The value to set for the member</param>
    /// <returns>True if the member was set successfully; otherwise, false</returns>
    public override bool TrySetMember(SetMemberBinder binder, object value)
    {
        this.Members[binder.Name] = value;
        return true;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a string
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator string(ToStringExpandoObject e) => e.ToString();

    /// <summary>
    /// Implicitly converts the dynamic object to a nullable boolean
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator bool?(ToStringExpandoObject e)
    {
        if (bool.TryParse(e.ToString(), out bool b))
        {
            return b;
        }
        return null;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a nullable integer
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator int?(ToStringExpandoObject e)
    {
        if (int.TryParse(e.ToString(), out int b))
        {
            return b;
        }
        return null;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a nullable decimal
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator decimal?(ToStringExpandoObject e)
    {
        if (decimal.TryParse(e.ToString(), out decimal b))
        {
            return b;
        }
        return null;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a nullable double
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator double?(ToStringExpandoObject e)
    {
        if (double.TryParse(e.ToString(), out double b))
        {
            return b;
        }
        return null;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a nullable long
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    public static implicit operator long?(ToStringExpandoObject e)
    {
        if (long.TryParse(e.ToString(), out long b))
        {
            return b;
        }
        return null;
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a long
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    /// <exception cref="ValueConversionException">Thrown when the value cannot be converted to a long</exception>
    public static implicit operator long(ToStringExpandoObject e)
    {
        var b = (long?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(long), e);
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a boolean
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    /// <exception cref="ValueConversionException">Thrown when the value cannot be converted to a boolean</exception>
    public static implicit operator bool(ToStringExpandoObject e)
    {
        var b = (bool?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(bool), e);
    }

    /// <summary>
    /// Implicitly converts the dynamic object to an integer
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    /// <exception cref="ValueConversionException">Thrown when the value cannot be converted to an integer</exception>
    public static implicit operator int(ToStringExpandoObject e)
    {
        var b = (int?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(int), e);
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a decimal
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    /// <exception cref="ValueConversionException">Thrown when the value cannot be converted to a decimal</exception>
    public static implicit operator decimal(ToStringExpandoObject e)
    {
        var b = (decimal?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(decimal), e);
    }

    /// <summary>
    /// Implicitly converts the dynamic object to a double
    /// </summary>
    /// <param name="e">The dynamic object to convert</param>
    /// <exception cref="ValueConversionException">Thrown when the value cannot be converted to a double</exception>
    public static implicit operator double(ToStringExpandoObject e)
    {
        var b = (double?)e;
        if (b.HasValue)
        {
            return b.Value;
        }
        throw new ValueConversionException(typeof(double), e);
    }

    /// <summary>
    /// Returns a string representation of the dynamic object
    /// </summary>
    /// <returns>A string representation of the dynamic object</returns>
    public override string ToString()
    {
        //see if we defined a ToString member
        //if not, use the base implementation
        object methodObj;
        this.Members.TryGetValue("ToString", out methodObj);
        ToStringFunc method = methodObj as ToStringFunc;
        if (method == null)
            return base.ToString();

        return method();
    }
}