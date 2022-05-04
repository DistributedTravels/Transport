using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Transport.Database.Tables;
using Transport.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models.Transport;
using Models;
using Transport.Handlers;
using System.Threading.Tasks;

namespace Transport.Tests
{
    [TestClass]
    public class ReserveTravelHandlerTest
    {
        [TestMethod]
        public async Task UpdateFlightSeats_IdFound()
        {
            // Arrange
            var data = new List<Travel>() {
                new Travel {
                    Id = 2,
                    DepartureTime = DateTime.Parse("2022-06-15T22:20:00"),
                    Destination = "Malta",
                    Source = "Gdañsk",
                    FreeSeats = 89
                },
                new Travel {
                    Id = 3,
                    DepartureTime = DateTime.Parse("2022-07-15T22:20:00"),
                    Destination = "Francja",
                    Source = "Gdañsk",
                    FreeSeats = 83
                },
                new Travel {
                    Id = 5,
                    DepartureTime = DateTime.Parse("2022-07-20T12:20:00"),
                    Destination = "Czechy",
                    Source = "Gdañsk",
                    FreeSeats = 79
                }
            }.AsQueryable();
            var checkData = data.ToList();
            var @event = new ReserveTravelEvent(2, 4);

            // setup
            var mockSet = new Mock<DbSet<Travel>>();
            var mockContext = new Mock<TransportContext>();

            mockSet.As<IQueryable<Travel>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockContext.Setup(m => m.Travels).Returns(mockSet.Object);

            var mockPublish = new Mock<Action<EventModel>>();
            var mockReply = new Mock<Action<EventModel,string,string>>();
            var handler = new ReserveTravelHandler(mockPublish.Object, mockReply.Object, "dummyConnString");

            // Act
            var res = await handler.UpdateFlightSeats(mockContext.Object, @event);

            // Assert
            Assert.AreEqual(checkData[0].FreeSeats, 89 - 4);
            Assert.AreEqual(checkData[1].FreeSeats, 83);
            Assert.AreEqual(checkData[2].FreeSeats, 79);
            Assert.AreEqual(res, 89 - 4);
        }

        [TestMethod]
        public async Task UpdateFlightSeats_IdNotFound()
        {
            // Arrange
            var data = new List<Travel>() {
                new Travel {
                    Id = 2,
                    DepartureTime = DateTime.Parse("2022-06-15T22:20:00"),
                    Destination = "Malta",
                    Source = "Gdañsk",
                    FreeSeats = 89
                },
                new Travel {
                    Id = 3,
                    DepartureTime = DateTime.Parse("2022-07-15T22:20:00"),
                    Destination = "Francja",
                    Source = "Gdañsk",
                    FreeSeats = 83
                },
                new Travel {
                    Id = 5,
                    DepartureTime = DateTime.Parse("2022-07-20T12:20:00"),
                    Destination = "Czechy",
                    Source = "Gdañsk",
                    FreeSeats = 79
                }
            }.AsQueryable();
            var checkData = data.ToList();
            var @event = new ReserveTravelEvent(1, 4);

            // setup
            var mockSet = new Mock<DbSet<Travel>>();
            var mockContext = new Mock<TransportContext>();

            mockSet.As<IQueryable<Travel>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Travel>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            mockContext.Setup(m => m.Travels).Returns(mockSet.Object);

            var mockPublish = new Mock<Action<EventModel>>();
            var mockReply = new Mock<Action<EventModel, string, string>>();
            var handler = new ReserveTravelHandler(mockPublish.Object, mockReply.Object, "dummyConnString");

            // Act
            var res = await handler.UpdateFlightSeats(mockContext.Object, @event);

            // Assert
            Assert.AreEqual(res, null);
        }
    }
}