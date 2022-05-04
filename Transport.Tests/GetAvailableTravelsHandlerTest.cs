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
    public class GetAvailableTravelsHandlerTest
    {
        [TestMethod]
        public void FilterResults_ResultsFound()
        {
            // Arrange
            var data = new List<Travel>() {
                new Travel {
                    Id = 2,
                    DepartureTime = DateTime.Parse("2022-06-15T22:20:00"),
                    Destination = "Malta",
                    Source = "Gdańsk",
                    FreeSeats = 89
                },
                new Travel {
                    Id = 3,
                    DepartureTime = DateTime.Parse("2022-07-15T22:20:00"),
                    Destination = "Francja",
                    Source = "Gdańsk",
                    FreeSeats = 83
                },
                new Travel {
                    Id = 5,
                    DepartureTime = DateTime.Parse("2022-07-20T12:20:00"),
                    Destination = "Czechy",
                    Source = "Gdańsk",
                    FreeSeats = 79
                }
            };
            var checkData = data.ToList();

            var mockPublish = new Mock<Action<EventModel>>();
            var mockReply = new Mock<Action<EventModel, string, string>>();
            var handler = new GetAvailableTravelsHandler(mockPublish.Object, mockReply.Object, "dummyConnString");

            // Act
            var res = handler.FilterResult(data, 2, "any", "Czechy");

            // Assert
            Assert.AreEqual(res.First().Destination, "Czechy");
            Assert.AreEqual(res.First().TravelId, 5);
            Assert.AreEqual(res.Count(), 1);
        }

        [TestMethod]
        public void FilterResults_ResultsNotFound()
        {
            // Arrange
            var data = new List<Travel>() {
                new Travel {
                    Id = 2,
                    DepartureTime = DateTime.Parse("2022-06-15T22:20:00"),
                    Destination = "Malta",
                    Source = "Gdańsk",
                    FreeSeats = 89
                },
                new Travel {
                    Id = 3,
                    DepartureTime = DateTime.Parse("2022-07-15T22:20:00"),
                    Destination = "Francja",
                    Source = "Gdańsk",
                    FreeSeats = 83
                },
                new Travel {
                    Id = 5,
                    DepartureTime = DateTime.Parse("2022-07-20T12:20:00"),
                    Destination = "Czechy",
                    Source = "Gdańsk",
                    FreeSeats = 79
                }
            };

            var mockPublish = new Mock<Action<EventModel>>();
            var mockReply = new Mock<Action<EventModel, string, string>>();
            var handler = new GetAvailableTravelsHandler(mockPublish.Object, mockReply.Object, "dummyConnString");

            // Act
            var res = handler.FilterResult(data, 2, "Warszawa", "Czechy");

            // Assert
            Assert.AreEqual(res, null);
        }

        [TestMethod]
        public void GenerateTravels_ProperResults()
        {
            // Arrange
            var dest = new List<Destination>()
            {
                new Destination { Id = 1, Name = "Malta"},
                new Destination { Id = 2, Name = "Włochy"},
                new Destination { Id = 3, Name = "Francja"},
                new Destination { Id = 4, Name = "Dania"}
            }.AsQueryable();
            var src = new List<Source>()
            {
                new Source { Id = 1, Name = "Gdańsk"},
                new Source { Id = 2, Name = "Warszawa"},
                new Source { Id = 3, Name = "Kraków"}
            }.AsQueryable();
            var mockPublish = new Mock<Action<EventModel>>();
            var mockReply = new Mock<Action<EventModel, string, string>>();
            var handler = new GetAvailableTravelsHandler(mockPublish.Object, mockReply.Object, "dummyConnString");

            var mockSrc = new Mock<DbSet<Source>>();

            mockSrc.As<IQueryable<Source>>().Setup(m => m.Provider).Returns(src.Provider);
            mockSrc.As<IQueryable<Source>>().Setup(m => m.Expression).Returns(src.Expression);
            mockSrc.As<IQueryable<Source>>().Setup(m => m.ElementType).Returns(src.ElementType);
            mockSrc.As<IQueryable<Source>>().Setup(m => m.GetEnumerator()).Returns(src.GetEnumerator());

            var mockDest = new Mock<DbSet<Destination>>();

            mockDest.As<IQueryable<Destination>>().Setup(m => m.Provider).Returns(dest.Provider);
            mockDest.As<IQueryable<Destination>>().Setup(m => m.Expression).Returns(dest.Expression);
            mockDest.As<IQueryable<Destination>>().Setup(m => m.ElementType).Returns(dest.ElementType);
            mockDest.As<IQueryable<Destination>>().Setup(m => m.GetEnumerator()).Returns(dest.GetEnumerator());

            var mockContext = new Mock<TransportContext>();
            mockContext.Setup(m => m.Sources).Returns(mockSrc.Object);
            mockContext.Setup(m => m.Destinations).Returns(mockDest.Object);

            // Act
            var res = handler.GenerateTravels(DateTime.Now.Date, mockContext.Object);

            // Assert
            Assert.AreEqual(res.Count(), dest.Count() * src.Count());
            Assert.AreEqual(res.Where(x => x.Source == "Gdańsk").Count(), dest.Count());
            Assert.AreEqual(res.Where(x => x.Destination == "Malta").Count(), src.Count());
            Assert.AreEqual(res.Where(x => x.DepartureTime >= DateTime.Now.Date && x.DepartureTime < DateTime.Now.Date.AddDays(1)).Count(), res.Count());
        }
    }
}
