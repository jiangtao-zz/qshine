using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using qshine.EventSourcing;
using qshine.Messaging;
using qshine.Configuration;

namespace qshine.eventstore.Tests
{
    [TestClass()]
    public class EventStoreTests
    {
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            ApplicationEnvironment.Build("app.config");
        }

        [TestMethod()]
        public void RestoreFromEs_Change_Save_long_Test()
        {
            //create repository            
            var repository = new EventStoreRepository<TestRoot1>();

            //get an aggregate by Id
            var ar = repository.Get<TestRoot1>(100);

            if (ar == null)
            {
                ar = new TestRoot1(100L);
            }

            //Change domain
            ar.Command1("BBB", 123);
            ar.Command2(456);
            ar.Command2(789);

            //Save the aggregate
            repository.Save(ar);

            //Verify ar result
            Assert.AreEqual("BBB", ar.V1);
            Assert.AreEqual(123, ar.V2);
            Assert.AreEqual(789L, ar.V3);
            //Assert.AreEqual(5, ar.Version);
            Assert.AreEqual(0, ar.EventQueue.Count());
        }

        [TestMethod()]
        public void RestoreFromEs_Change_Save_guid_Test()
        {
            var guid = new Guid("775BBEA8-2CE7-4AF3-AC8B-13D050491768");
            //create repository            
            var repository = new EventStoreRepository<TestRoot2>();

            //get an aggregate by Id
            var ar = repository.Get<TestRoot2>(guid);

            if (ar == null)
            {
                ar = new TestRoot2(guid);
            }

            //Change domain
            ar.Command1("BBB", 123);
            ar.Command2(456);
            ar.Command2(789);

            //Save the aggregate
            repository.Save(ar);

            //Verify ar result
            Assert.AreEqual("BBB", ar.V1);
            Assert.AreEqual(123, ar.V2);
            Assert.AreEqual(789L, ar.V3);
            //Assert.AreEqual(5, ar.Version);
            Assert.AreEqual(0, ar.EventQueue.Count());
        }
    }

    /// <summary>
    /// Test Objects
    /// </summary>
    public class TestRoot1 : Aggregate,
        IHandler<Command1CompletedEvent>,
        IHandler<Command2CompletedEvent>
    {
        /// <summary>
        /// default for replay
        /// </summary>
        public TestRoot1() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public TestRoot1(long id)
        {
            Id = id;
        }
        /// <summary>
        /// 
        /// </summary>
        public string V1 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int V2 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public long V3 { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void Command1(string v1, int v2)
        {
            RaiseEvent(new Command1CompletedEvent
            {
                V1 = v1,
                V2 = v2
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v3"></param>
        public void Command2(long v3)
        {
            RaiseEvent(new Command2CompletedEvent
            {
                V3 = v3
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public void Handle(Command1CompletedEvent @event)
        {
            V1 = @event.V1;
            V2 = @event.V2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public void Handle(Command2CompletedEvent @event)
        {
            V3 = @event.V3;
        }
    }

    /// <summary>
    /// Test Objects
    /// </summary>
    public class TestRoot2 : Aggregate,
        IHandler<Command1CompletedEvent>,
        IHandler<Command2CompletedEvent>
    {
        /// <summary>
        /// default for replay
        /// </summary>
        public TestRoot2() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        public TestRoot2(Guid id)
        {
            Id = id;
        }
        /// <summary>
        /// 
        /// </summary>
        public string V1 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public int V2 { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public long V3 { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        public void Command1(string v1, int v2)
        {
            RaiseEvent(new Command1CompletedEvent
            {
                V1 = v1,
                V2 = v2
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v3"></param>
        public void Command2(long v3)
        {
            RaiseEvent(new Command2CompletedEvent
            {
                V3 = v3
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public void Handle(Command1CompletedEvent @event)
        {
            V1 = @event.V1;
            V2 = @event.V2;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="event"></param>
        public void Handle(Command2CompletedEvent @event)
        {
            V3 = @event.V3;
        }
    }


    public class Command1CompletedEvent : IDomainEvent
    {
        public Command1CompletedEvent()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string V1 { get; set; }
        public int V2 { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int Version { get; set; }
    }

    public class Command2CompletedEvent : IDomainEvent
    {
        public Command2CompletedEvent()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public long V3 { get; set; }

        public int Version { get; set; }

        public DateTimeOffset TimeStamp { get; set; }
    }
}
