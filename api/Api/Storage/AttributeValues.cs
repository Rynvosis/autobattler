using System.Globalization;
using Amazon.DynamoDBv2.Model;

namespace Api.Storage;

public static class AttributeValues
{
    public static AttributeValue Number(long value)
    {
        return new AttributeValue { N = value.ToString(CultureInfo.InvariantCulture) };
    }

    public static int ToInt32(AttributeValue value)
    {
        return int.Parse(value.N, CultureInfo.InvariantCulture);
    }

    public static long ToInt64(AttributeValue value)
    {
        return long.Parse(value.N, CultureInfo.InvariantCulture);
    }
}
