using System.Collections;
using System.Data;
using System.Data.Common;

namespace Initialize.Reflection
{
    /// <summary>
    /// Provides a means of reading a sequence of objects as a data-reader, for example
    /// for use with SqlBulkCopy or other data-base oriented code
    /// </summary>
    public class ObjectReader : DbDataReader
    {
        private IEnumerator source;
        private readonly TypeAccessor accessor;
        private readonly string[] memberNames;
        private readonly Type[] effectiveTypes;
        private readonly BitArray allowNull;

        /// <summary>
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public static ObjectReader Create<T>(IEnumerable<T> source, params string[] members)
        {
            return new ObjectReader(typeof(T), source, members);
        }

        /// <summary>
        /// Creates a new ObjectReader instance for reading the supplied data
        /// </summary>
        /// <param name="type">The expected Type of the information to be read</param>
        /// <param name="source">The sequence of objects to represent</param>
        /// <param name="members">The members that should be exposed to the reader</param>
        public ObjectReader(Type type, IEnumerable source, params string[] members)
        {
            if (source == null) throw new ArgumentOutOfRangeException("source");

            

            bool allMembers = members == null || members.Length == 0;

            this.accessor = TypeAccessor.Create(type);
            if (this.accessor.GetMembersSupported)
            {
                // Sort members by ordinal first and then by name.
                var typeMembers = this.accessor.GetMembers().OrderBy(p => p.Ordinal).ToList();

                if (allMembers)
                {
                    members = new string[typeMembers.Count];
                    for (int i = 0; i < members.Length; i++)
                    {
                        members[i] = typeMembers[i].Name;
                    }
                }

                this.allowNull = new BitArray(members.Length);
                this.effectiveTypes = new Type[members.Length];
                for (int i = 0; i < members.Length; i++)
                {
                    Type memberType = null;
                    bool allowNull = true;
                    string hunt = members[i];
                    foreach (var member in typeMembers)
                    {
                        if (member.Name == hunt)
                        {
                            if (memberType == null)
                            {
                                var tmp = member.Type;
                                memberType = Nullable.GetUnderlyingType(tmp) ?? tmp;

                                allowNull = !(memberType.IsValueType && memberType == tmp);

                                // but keep checking, in case of duplicates
                            }
                            else
                            {
                                memberType = null; // duplicate found; say nothing
                                break;
                            }
                        }
                    }
                    this.allowNull[i] = allowNull;
                    this.effectiveTypes[i] = memberType ?? typeof(object);
                }
            }
            else if (allMembers)
            {
                throw new InvalidOperationException("Member information is not available for this type; the required members must be specified explicitly");
            }

            this.current = null;
            this.memberNames = (string[])members.Clone();

            this.source = source.GetEnumerator();
        }

        object current;


        public override int Depth
        {
            get { return 0; }
        }

        public override DataTable GetSchemaTable()
        {
            // these are the columns used by DataTable load
            DataTable table = new DataTable
            {
                Columns =
                {
                    {"ColumnOrdinal", typeof(int)},
                    {"ColumnName", typeof(string)},
                    {"DataType", typeof(Type)},
                    {"ColumnSize", typeof(int)},
                    {"AllowDBNull", typeof(bool)}
                }
            };
            object[] rowData = new object[5];
            for (int i = 0; i < this.memberNames.Length; i++)
            {
                rowData[0] = i;
                rowData[1] = this.memberNames[i];
                rowData[2] = this.effectiveTypes == null ? typeof(object) : this.effectiveTypes[i];
                rowData[3] = -1;
                rowData[4] = this.allowNull == null ? true : this.allowNull[i];
                table.Rows.Add(rowData);
            }
            return table;
        }
        public override void Close()
        {
			this.Shutdown();
        }

        public override bool HasRows
        {
            get
            {
                return this.active;
            }
        }
        private bool active = true;
        public override bool NextResult()
        {
			this.active = false;
            return false;
        }
        public override bool Read()
        {
            if (this.active)
            {
                var tmp = this.source;
                if (tmp != null && tmp.MoveNext())
                {
					this.current = tmp.Current;
                    return true;
                }
                else
                {
					this.active = false;
                }
            }
			this.current = null;
            return false;
        }

        public override int RecordsAffected
        {
            get { return 0; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing) this.Shutdown();
        }
        private void Shutdown()
        {
			this.active = false;
			this.current = null;
            var tmp = this.source as IDisposable;
			this.source = null;
            if (tmp != null) tmp.Dispose();
        }

        public override int FieldCount
        {
            get { return this.memberNames.Length; }
        }
        public override bool IsClosed
        {
            get
            {
                return this.source == null;
            }
        }

        public override bool GetBoolean(int i)
        {
            return (bool)this[i];
        }

        public override byte GetByte(int i)
        {
            return (byte)this[i];
        }

        public override long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            byte[] s = (byte[])this[i];
            int available = s.Length - (int)fieldOffset;
            if (available <= 0) return 0;

            int count = Math.Min(length, available);
            Buffer.BlockCopy(s, (int)fieldOffset, buffer, bufferoffset, count);
            return count;
        }

        public override char GetChar(int i)
        {
            return (char)this[i];
        }

        public override long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            string s = (string)this[i];
            int available = s.Length - (int)fieldoffset;
            if (available <= 0) return 0;

            int count = Math.Min(length, available);
            s.CopyTo((int)fieldoffset, buffer, bufferoffset, count);
            return count;
        }

        protected override DbDataReader GetDbDataReader(int i)
        {
            throw new NotSupportedException();
        }

        public override string GetDataTypeName(int i)
        {
            return (this.effectiveTypes == null ? typeof(object) : this.effectiveTypes[i]).Name;
        }

        public override DateTime GetDateTime(int i)
        {
            return (DateTime)this[i];
        }

        public override decimal GetDecimal(int i)
        {
            return (decimal)this[i];
        }

        public override double GetDouble(int i)
        {
            return (double)this[i];
        }

        public override Type GetFieldType(int i)
        {
            return this.effectiveTypes == null ? typeof(object) : this.effectiveTypes[i];
        }

        public override float GetFloat(int i)
        {
            return (float)this[i];
        }

        public override Guid GetGuid(int i)
        {
            return (Guid)this[i];
        }

        public override short GetInt16(int i)
        {
            return (short)this[i];
        }

        public override int GetInt32(int i)
        {
            return (int)this[i];
        }

        public override long GetInt64(int i)
        {
            return (long)this[i];
        }

        public override string GetName(int i)
        {
            return this.memberNames[i];
        }

        public override int GetOrdinal(string name)
        {
            return Array.IndexOf(this.memberNames, name);
        }

        public override string GetString(int i)
        {
            return (string)this[i];
        }

        public override object GetValue(int i)
        {
            return this[i];
        }

        public override IEnumerator GetEnumerator() => new DbEnumerator(this);

        public override int GetValues(object[] values)
        {
            // duplicate the key fields on the stack
            var members = this.memberNames;
            var current = this.current;
            var accessor = this.accessor;

            int count = Math.Min(values.Length, members.Length);
            for (int i = 0; i < count; i++) values[i] = accessor[current, members[i]] ?? DBNull.Value;
            return count;
        }

        public override bool IsDBNull(int i)
        {
            return this[i] is DBNull;
        }

        public override object this[string name]
        {
            get { return this.accessor[this.current, name] ?? DBNull.Value; }

        }
        /// <summary>
        /// Gets the value of the current object in the member specified
        /// </summary>
        public override object this[int i]
        {
            get { return this.accessor[this.current, this.memberNames[i]] ?? DBNull.Value; }
        }
    }
}