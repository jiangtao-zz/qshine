using qshine.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace qshine.Tests
{
    public class TestEntity:AuditableEntity
    {
        public string V1 { get; set; }
        public decimal V2 { get; set; }
        public DateTime V3 { get; set; }
        public Dictionary<string, int> V4 { get; set; }
        public TestEntity V5 { get; set; }
        public int[] V6;
        public decimal[] V7;
        public List<string> V8;
        public bool V9;
    }
}
