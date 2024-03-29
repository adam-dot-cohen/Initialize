﻿using System.Reflection;

namespace Initialize.Reflection
{
    /// <summary>
    /// Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IEnumerable<Member>, IList<Member>
    {
        Member[] members;
        internal MemberSet(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;
			this.members = type.GetTypeAndInterfaceProperties(PublicInstance).Cast<MemberInfo>().Concat(type.GetFields(PublicInstance).Cast<MemberInfo>()).OrderBy(x => x.Name)
                .Select(member => new Member(member)).ToArray();
        }
        /// <summary>
        /// Return a sequence of all defined members
        /// </summary>
        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in this.members) yield return member;
        }
        /// <summary>
        /// Get a member by index
        /// </summary>
        public Member this[int index]
        {
            get { return this.members[index]; }
        }
        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        public int Count { get { return this.members.Length; } }
        Member IList<Member>.this[int index]
        {
            get { return this.members[index]; }
            set { throw new NotSupportedException(); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
        bool ICollection<Member>.Remove(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Add(Member item) { throw new NotSupportedException(); }
        void ICollection<Member>.Clear() { throw new NotSupportedException(); }
        void IList<Member>.RemoveAt(int index) { throw new NotSupportedException(); }
        void IList<Member>.Insert(int index, Member item) { throw new NotSupportedException(); }

        bool ICollection<Member>.Contains(Member item)  => this.members.Contains(item);
        void ICollection<Member>.CopyTo(Member[] array, int arrayIndex) { this.members.CopyTo(array, arrayIndex); }
        bool ICollection<Member>.IsReadOnly { get { return true; } }
        int IList<Member>.IndexOf(Member member) { return Array.IndexOf<Member>(this.members, member); }
        
    }
    /// <summary>
    /// Represents an abstracted view of an individual member defined for a type
    /// </summary>
    public sealed class Member
    {
        private readonly MemberInfo member;
        internal Member(MemberInfo member)
        {
            this.member = member;
        }
        /// <summary>
        /// The ordinal of this member among other members.
        /// Returns -1 in case the ordinal is not set.
        /// </summary>
        public int Ordinal
        {
            get
            {
                var ordinalAttr = this.member.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(OrdinalAttribute));

                if (ordinalAttr == null)
                {
                    return -1;
                }

                // OrdinalAttribute class must have only one constructor with a single argument.
                return Convert.ToInt32(ordinalAttr.ConstructorArguments.Single().Value);
            }
        }
        /// <summary>
        /// The name of this member
        /// </summary>
        public string Name { get { return this.member.Name; } }
        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        public Type Type
        {
            get
            {
                if(this.member is FieldInfo) return ((FieldInfo)this.member).FieldType;
                if (this.member is PropertyInfo) return ((PropertyInfo)this.member).PropertyType;
                throw new NotSupportedException(this.member.GetType().Name);
            }
        }

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType)
        {
            if (attributeType == null) throw new ArgumentNullException(nameof(attributeType));
            return Attribute.IsDefined(this.member, attributeType);
        }

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute(Type attributeType, bool inherit)
            => Attribute.GetCustomAttribute(this.member, attributeType, inherit);

		/// <summary>
		/// Property Can Write
		/// </summary>
		public bool CanWrite
        {
            get
            {
                switch (this.member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)this.member).CanWrite;
                    default: throw new NotSupportedException(this.member.MemberType.ToString());
                }
            }
        }

        /// <summary>
        /// Property Can Read
        /// </summary>
        public bool CanRead
        {
            get
            {
                switch (this.member.MemberType)
                {
                    case MemberTypes.Property: return ((PropertyInfo)this.member).CanRead;
                    default: throw new NotSupportedException(this.member.MemberType.ToString());
                }
            }
        }
    }
}
