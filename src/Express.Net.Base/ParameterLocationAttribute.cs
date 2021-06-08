using System;

namespace Express.Net
{
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromBodyAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromCookieAttribute : Attribute
    {
        public string? Name { get; private set; }

        public FromCookieAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromFormAttribute : Attribute
    {
        public string? Name { get; private set; }

        public FromFormAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromHeaderAttribute : Attribute
    {
        public string? Name { get; private set; }

        public FromHeaderAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromQueryAttribute : Attribute
    {
        public string? Name { get; private set; }

        public FromQueryAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromRouteAttribute : Attribute
    {
        public string? Name { get; private set; }

        public FromRouteAttribute(string? name = null)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public sealed class FromServicesAttribute : Attribute
    {
    }
}
