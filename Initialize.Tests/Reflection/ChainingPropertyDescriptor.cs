using System.ComponentModel;

namespace Initialize.Tests.Reflection {
    public abstract class ChainingPropertyDescriptor : PropertyDescriptor {
        private readonly PropertyDescriptor _root;
        protected PropertyDescriptor Root { get { return this._root; } }
        protected ChainingPropertyDescriptor(PropertyDescriptor root)
            : base(root) {
			this._root = root;
        }
        public override void AddValueChanged(object component, EventHandler handler) {
			this.Root.AddValueChanged(component, handler);
        }
        public override AttributeCollection Attributes {
            get {
                return this.Root.Attributes;
            }
        }
        public override bool CanResetValue(object component) {
            return this.Root.CanResetValue(component);
        }
        public override string Category {
            get {
                return this.Root.Category;
            }
        }
        public override Type ComponentType {
            get { return this.Root.ComponentType; }
        }
        public override TypeConverter Converter {
            get {
                return this.Root.Converter;
            }
        }
        public override string Description {
            get {
                return this.Root.Description;
            }
        }
        public override bool DesignTimeOnly {
            get {
                return this.Root.DesignTimeOnly;
            }
        }
        public override string DisplayName {
            get {
                return this.Root.DisplayName;
            }
        }
        public override bool Equals(object obj) {
            return this.Root.Equals(obj);
        }
        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter) {
            return this.Root.GetChildProperties(instance, filter);
        }
        public override object GetEditor(Type editorBaseType) {
            return this.Root.GetEditor(editorBaseType);
        }
        public override int GetHashCode() {
            return this.Root.GetHashCode();
        }
        public override object GetValue(object component) {
            return this.Root.GetValue(component);
        }
        public override bool IsBrowsable {
            get {
                return this.Root.IsBrowsable;
            }
        }
        public override bool IsLocalizable {
            get {
                return this.Root.IsLocalizable;
            }
        }
        public override bool IsReadOnly {
            get { return this.Root.IsReadOnly; }
        }
        public override string Name {
            get {
                return this.Root.Name;
            }
        }
        public override Type PropertyType {
            get { return this.Root.PropertyType; }
        }
        public override void RemoveValueChanged(object component, EventHandler handler) {
			this.Root.RemoveValueChanged(component, handler);
        }
        public override void ResetValue(object component) {
			this.Root.ResetValue(component);
        }
        public override void SetValue(object component, object value) {
			this.Root.SetValue(component, value);
        }
        public override bool ShouldSerializeValue(object component) {
            return this.Root.ShouldSerializeValue(component);
        }
        public override bool SupportsChangeEvents {
            get {
                return this.Root.SupportsChangeEvents;
            }
        }
        public override string ToString() {
            return this.Root.ToString();
        }
    }
}