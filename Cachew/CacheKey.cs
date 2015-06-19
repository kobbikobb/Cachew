using System;
using System.Linq;

namespace Cachew
{
    public class CacheKey : IEquatable<CacheKey>
    {
        private readonly string name;
        private readonly object[] arguments;

        public CacheKey(string name, params object[] arguments)
        {
            if (name == null) throw new ArgumentNullException("name");

            this.name = name;
            this.arguments = arguments;
        }

        #region Equals

        public bool Equals(CacheKey other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (!string.Equals(name, other.name)) return false;
            if (arguments == null && other.arguments == null) return true;
            if (arguments == null || other.arguments == null) return false;
            return arguments.SequenceEqual(other.arguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CacheKey)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((arguments != null ? arguments.GetHashCode() : 0) * 397) ^ (name != null ? name.GetHashCode() : 0);
            }
        }

        #endregion
    }
}
