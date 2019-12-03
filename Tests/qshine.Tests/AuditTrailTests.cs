using Microsoft.VisualStudio.TestTools.UnitTesting;
using qshine.Audit;
using qshine.Caching;
using qshine.Domain;
using qshine.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace qshine.Tests
{
    [TestClass]
    public class AuditTrailTests
    {
        [TestMethod()]
        public void SimpleEntityAuditTests()
        {
            var entity = new TestEntity
            {
                Id = 100L,
                V1 = "A1",
                V2 = (decimal)1.2,
                V3 = new System.DateTime(2000,1,2,10,05,01),
                V4 = new Dictionary<string, int>
                {
                    {"A",1 },
                    {"B",2 },
                },
            };

            var auditTrail = new EntityAudit<TestEntity>(entity);

            //change entity
            entity.V1 = "B1";
            entity.V4["B"] = 22;

            //intercept bus message
            var interceptor = Interceptor.Get<EventBus>();
            bool eventPublished = false;
            interceptor.OnSuccess += (sender, args) =>
            {
                var method = args.MethodName;
                var bus = args.Args[0];
                var message = args.Args[1] as AuditTrail;
                if (message!=null && message.EntityName == "qshine.Tests.TestEntity")
                {

                    Assert.AreEqual(2, message.Data.Count);
                    Assert.AreEqual("B1", message.Data["V1"].NewValue);
                    Assert.AreEqual("A1", message.Data["V1"].OldValue);

                    var v4 = message.Data["V4"].NewValue as Dictionary<string, object>;
                    var v4o = message.Data["V4"].OldValue as Dictionary<string, object>;

                    Assert.AreEqual("22", v4["B"].ToString());
                    Assert.AreEqual("2", v4o["B"].ToString());
                    eventPublished = true;
                }
            };

            //Save entity
            //Repository.Save(entity);
            auditTrail.AuditEntityUpdate(entity);

            if (!eventPublished)
            {
                Assert.Fail("Doesn't received Audit event.");
            }

        }
    }
}

