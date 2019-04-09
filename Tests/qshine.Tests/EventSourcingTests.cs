using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using qshine.EventSourcing;
using qshine.Messaging;
using Moq;
using qshine.Domain;

namespace qshine.Tests
{
    [TestClass()]
    public class EventSourcingTests
    {
        [TestMethod()]
        public void EntityIdType_Guid_Test()
        {
            var guid = Guid.NewGuid();
            var id = new EntityIdType(guid);

            var copiedGuid = new Guid(guid.ToString("B"));
            var id2 = new EntityIdType(copiedGuid);
            var id3 = new EntityIdType();
            var x = id3.ToString();
            id3 = copiedGuid;

            EntityIdType id4 = copiedGuid;

            Assert.AreEqual(id, id2);
            Assert.AreEqual(id, id3);
            Assert.AreEqual(id, id4);
            Assert.AreEqual(id, guid);
            Assert.AreEqual(id.GetValue<Guid>(), guid);
            Assert.AreEqual(id4.GetValue<Guid>(), guid);
            Assert.AreEqual(new EntityIdType().GetValue<Guid>(), default(Guid));

            Guid? value = id.GetNullableValue<Guid>();
            Assert.AreEqual(copiedGuid, value.Value);

            Guid? guidid5 = null;
            EntityIdType id5 = guidid5;
            Assert.AreEqual(null, id5.GetNullableValue<Guid>());

            guidid5 = copiedGuid;
            EntityIdType id6 = guidid5;
            Assert.AreEqual(guidid5, id6.GetNullableValue<Guid>());

        }

        [TestMethod()]
        public void EntityIdType_Long_Test()
        {
            long longId = 1001L;
            var id = new EntityIdType(longId);

            EntityIdType id2 =longId;

            Assert.AreEqual(id, id2);
            Assert.AreEqual(id.GetValue<long>(), 1001L);
            Assert.AreEqual(new EntityIdType().GetValue<long>(), default(long));

            long? value = new EntityIdType().GetNullableValue<long>();
            Assert.AreEqual(null, value);

            value = id.GetNullableValue<long>();
            Assert.AreEqual(1001L, value.Value);

            long? longid3 = null;
            EntityIdType id3 = longid3;
            Assert.AreEqual(null, id3.GetNullableValue<long>());

            longid3 = 123;
            EntityIdType id4 = longid3;
            Assert.AreEqual(123, id4.GetNullableValue<long>());


        }


        [TestMethod()]
        public void RaiseEventTest()
        {
            var t1 = new TestRoot1();
            t1.Command1("A", 1);
            Assert.AreEqual("A", t1.V1);
            Assert.AreEqual(1, t1.V2);
            Assert.AreEqual(0L, t1.V3);
            t1.Command2(3L);
            Assert.AreEqual(3L, t1.V3);
        }

        [TestMethod()]
        public void RestoreFromEs_Change_Save_long_Test()
        {
            //mock event store
            var esMock = new Mock<IEventStore>();
            esMock.Setup(x => x.Load(new EntityIdType(100), -1)).Returns(
                new List<IDomainEvent> {
                    new Command1CompletedEvent
                    {
                        Id = Guid.NewGuid(),
                        V1 = "A1",
                        V2 = 1,
                        Version = 0
                    },
                    new Command1CompletedEvent
                    {
                        Id = Guid.NewGuid(),
                        V1 = "A1",
                        V2 = 2,
                        Version = 1
                    },
                    new Command2CompletedEvent
                    {
                        Id = Guid.NewGuid(),
                        V3 = 12,
                        Version = 2
                    }
                });

            //create repository            
            var repository = new EventStoreRepository<TestRoot1>(esMock.Object);

            //get an aggregate by Id
            var ar = repository.Get(100L);

            //verify
            Assert.AreEqual("A1", ar.V1);
            Assert.AreEqual(2, ar.V2);
            Assert.AreEqual(12L, ar.V3);

            //Change domain
            ar.Command1("BBB", 123);
            ar.Command2(456);
            ar.Command2(789);

            //Save the aggregate
            repository.Save(ar);

            //verify total events
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x=>x.GetValue<long>()==100L),
                It.Is<IEnumerable<IDomainEvent>>(x=>x.Count() == 3)), Times.Once);
//            esMock.Verify(m => m.Save(
//                It.Is<IEnumerable<IDomainEvent>>(x => x.Single(y=>y.Version==3).AggregateId.Equals(100L))), Times.Once);
            //verify event1
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<long>() == 100L),
                It.Is<IEnumerable<IDomainEvent>>(x => x.Single(y => y.Version == 3)
                .TimeStamp.Subtract(DateTimeOffset.UtcNow).Seconds<1)), Times.Once);
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<long>() == 100L),
                It.Is<IEnumerable<IDomainEvent>>(x => 
                (x.Single(y => y.Version == 3) as Command1CompletedEvent)
                .V1=="BBB")), Times.Once);
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<long>() == 100L),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 3) as Command1CompletedEvent)
                .V2 == 123)), Times.Once);

            //verify event2
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<long>() == 100L),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 4) as Command2CompletedEvent)
                .V3 == 456L)), Times.Once);
            
            //verify event3
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<long>() == 100L),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 5) as Command2CompletedEvent)
                .V3 == 789L)), Times.Once);

            //Verify ar result
            Assert.AreEqual("BBB", ar.V1);
            Assert.AreEqual(123, ar.V2);
            Assert.AreEqual(789L, ar.V3);
            Assert.AreEqual(5, ar.Version);
            Assert.AreEqual(0, ar.EventQueue.Count());
        }

        [TestMethod()]
        public void RestoreFromEs_Change_Save_Guid_Test()
        {
            Guid aggregateGuid = new Guid("B58CAF46-4C2A-4949-B44A-1AFDB08DBFE8");

            var arType = typeof(TestRoot2).FullName;
            //mock event store
            var esMock = new Mock<IEventStore>();
            esMock.Setup(x => x.Load(new EntityIdType(aggregateGuid), -1)).Returns(
                new List<IDomainEvent> {
                });

            //create repository            
            var repository = new EventStoreRepository<TestRoot2>(esMock.Object);

            //get an aggregate by Id
            var ar = new TestRoot2(aggregateGuid);
            var expectedGuid = new EntityIdType(aggregateGuid).GetValue<Guid>();

            //Change domain
            ar.Command1("CCC", 120);
            ar.Command2(450);
            ar.Command2(780);

            //Save the aggregate
            repository.Save(ar);

            //verify total events
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x => x.Count() == 3)), Times.Once);
//            esMock.Verify(m => m.Save(
//                It.Is<IEnumerable<IDomainEvent>>(x => x.Single(y => y.Version == 0).AggregateId.Equals(200L))), Times.Once);
            //verify event1
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x => x.Single(y => y.Version == 0)
                .TimeStamp.Subtract(DateTimeOffset.UtcNow).Seconds < 1)), Times.Once);
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 0) as Command1CompletedEvent)
                .V1 == "CCC")), Times.Once);
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 0) as Command1CompletedEvent)
                .V2 == 120)), Times.Once);

            //verify event2
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 1) as Command2CompletedEvent)
                .V3 == 450L)), Times.Once);

            //verify event3
            esMock.Verify(m => m.Save(It.Is<EntityIdType>(x => x.GetValue<Guid>() == expectedGuid),
                It.Is<IEnumerable<IDomainEvent>>(x =>
                (x.Single(y => y.Version == 2) as Command2CompletedEvent)
                .V3 == 780L)), Times.Once);

            //Verify ar result
            Assert.AreEqual("CCC", ar.V1);
            Assert.AreEqual(120, ar.V2);
            Assert.AreEqual(780L, ar.V3);
            Assert.AreEqual(2, ar.Version);
            Assert.AreEqual(0, ar.EventQueue.Count());
        }
    }

    /// <summary>
    /// Test Objects
    /// </summary>
    public class TestRoot1:Aggregate,
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


    public class Command1CompletedEvent:IDomainEvent
    {
        public Command1CompletedEvent()
        {
            Id = Guid.NewGuid();
        }
        public Guid Id { get; set; }
        public string V1 { get; set; }
        public int V2 { get; set; }

        public DateTimeOffset TimeStamp { get; set; }

        public int Version { get; set;}
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
