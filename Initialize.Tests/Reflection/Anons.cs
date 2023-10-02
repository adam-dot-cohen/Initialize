using Initialize.Reflection;

namespace Initialize.Tests.Reflection
{
    public class Anons
    {
        [Test]
        public void TestAnonTypeAccess()
        {
            var obj = new {A = 123, B = "def"};

            var accessor = ObjectAccessor.Create(obj);
            Assert.AreEqual(123, accessor["A"]);
            Assert.AreEqual("def", accessor["B"]);
        }
        [Test]
        public void TestAnonCtor()
        {
            var obj = new {A = 123, B = "def"};

            var accessor = TypeAccessor.Create(obj.GetType());
            Assert.False(accessor.CreateNewSupported);
        }

        [Test]
        public void TestPrivateTypeAccess()
        {
            var obj = new Private { A = 123, B = "def" };

            var accessor = ObjectAccessor.Create(obj);
            Assert.AreEqual(123, accessor["A"]);
            Assert.AreEqual("def", accessor["B"]);
        }

        [Test]
        public void TestPrivateTypeCtor()
        {
            var accessor = TypeAccessor.Create(typeof (Private));
            Assert.True(accessor.CreateNewSupported);
            object obj = accessor.CreateNew();
            Assert.NotNull(obj);
            Assert.IsAssignableFrom<Private>(obj);
        }

        private sealed class Private
        {
            public int A { get; set; }
            public string B { get; set; }
        }
    }


}
