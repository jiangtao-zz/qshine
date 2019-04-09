using qshine.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace qshine.Tests
{
    public class TestObject1
    {
        /// <summary>
        /// Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// String
        /// </summary>
        public string StringV { get; set; }
        /// <summary>
        /// Int
        /// </summary>
        public int IntV { get; set; }
        /// <summary>
        /// Long
        /// </summary>
        public long LongV { get; set; }
        /// <summary>
        /// Decimal
        /// </summary>
        public decimal DecimalV { get; set; }
        /// <summary>
        /// DateTime
        /// </summary>
        public DateTime DateTimeV { get; set; }

        public static List<TestObject1> DataStore
        {
            get
            {
                return SourceDataStore.Serialize().Deserialize<List<TestObject1>>();
            }
        }

        public static List<TestObject1> SourceDataStore = new List<TestObject1>
        {
            new TestObject1{Id=1, StringV="A", IntV=1, LongV=12L, DecimalV = 1.2M, DateTimeV = new DateTime(2000,1,1,1,2,3)},
            new TestObject1{Id=2, StringV="A", IntV=2, LongV=22L, DecimalV = 2.2M, DateTimeV = new DateTime(2000,1,2,1,2,3)},
            new TestObject1{Id=3, StringV="A", IntV=3, LongV=32L, DecimalV = 3.2M, DateTimeV = new DateTime(2000,1,3,1,2,3)},
            new TestObject1{Id=4, StringV="B", IntV=10, LongV=12L, DecimalV = 1.2M, DateTimeV = new DateTime(2000,1,1,1,2,3)},
            new TestObject1{Id=5, StringV="B", IntV=20, LongV=22L, DecimalV = 2.2M, DateTimeV = new DateTime(2000,1,2,1,2,3)},
            new TestObject1{Id=6, StringV="B", IntV=30, LongV=32L, DecimalV = 3.2M, DateTimeV = new DateTime(2000,1,3,1,2,3)},
        };

        public static void ChangeData(int id, string s, int i)
        {
            var d = SourceDataStore.SingleOrDefault(x => x.Id == id);
            if (d != null)
            {
                d.StringV = s;
                d.IntV = i;

                MyCacheDataChangeMonitor.Current.InvalidCacheTags(typeof(TestObject1).FullName);
            }
        }
    }

    public class MyCacheDataChangeMonitor: CacheDataChangedMonitor
    {
        static MyCacheDataChangeMonitor _current = new MyCacheDataChangeMonitor();
        public static MyCacheDataChangeMonitor Current
        {
            get
            {
                return _current;
            }
        }
    }
}
