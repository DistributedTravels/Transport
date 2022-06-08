using MassTransit;
using Models.Transport;
using Models.Transport.Dto;
using Models.Reservations;
using Transport.Database;
using Transport.Database.Tables;
using Newtonsoft.Json;

namespace Transport.Consumers
{
    public class UpdateTransportTOEventConsumer : IConsumer<UpdateTransportTOEvent>
    {
        public async Task Consume(ConsumeContext<UpdateTransportTOEvent> context)
        {
            var @event = context.Message;

            switch (@event.Table)
            {
                case UpdateTransportTOEvent.Tables.TRAVEL:
                    await HandleTravelUpdate(@event.TravelDetails, @event.Action, context);
                    break;
                default:
                    break;
            }
        }

        public async Task HandleTravelUpdate(TravelChangeDto tcd, UpdateTransportTOEvent.Actions action, ConsumeContext ctx)
        {
            using (var context = new TransportContext())
            {
                var trvl = from travel in context.Travels where travel.Id == tcd.Id select travel;
                switch (action)
                {
                    case UpdateTransportTOEvent.Actions.NEW:
                        if (!trvl.Any())
                            if (tcd.Destination.Length > 0 && tcd.Source.Length > 0 && tcd.AvailableSeats >= 0 && tcd.DepartureTime > DateTime.UtcNow && tcd.Price > 0)
                            {
                                context.Add(new Travel { DepartureTime = tcd.DepartureTime.ToUniversalTime(), Source = tcd.Source, Destination = tcd.Destination, FreeSeats = tcd.AvailableSeats, Price = tcd.Price });
                                context.Changes.Add(new Change()
                                {
                                    DateTime = DateTime.UtcNow.ToUniversalTime(),
                                    Description = $"New travel added from {tcd.Source} to {tcd.Destination} with {tcd.AvailableSeats} seats and for {tcd.Price}",
                                });
                            }
                        break;
                    case UpdateTransportTOEvent.Actions.UPDATE:
                        if (trvl.Any())
                        {
                            var utrvl = trvl.Single();
                            if (tcd.AvailableSeats == 0)
                            {
                                // no seats available on plane, unreserve everything
                                var rsrv = from reservation in context.Reservations
                                           where reservation.TravelId == utrvl.Id && (reservation.State == ReservationState.RESERVED || reservation.State == ReservationState.PURCHASED)
                                           select reservation;
                                foreach (var reserv in rsrv)
                                {
                                    await ctx.Publish(new ChangesInReservationsEvent()
                                    {
                                        ReservationId = reserv.ReserveId,
                                        ReservationAvailable = false,
                                        ChangesInTransport = new TransportChange()
                                        {
                                            TransportId = utrvl.Id,
                                            PlaneAvailable = false,
                                            FreeSeatsChange = tcd.AvailableSeats,
                                            ChangeInTransportPrice = tcd.Price >= 0 ? tcd.Price : utrvl.Price,
                                        },
                                        ChangesInHotel = new HotelChange()
                                        {
                                            HotelId = -1,
                                            HotelAvailable = false,
                                            HotelName = "",
                                            ChangeInHotelPrice = 0,
                                            WifiAvailable = false,
                                            BreakfastAvailable = false,
                                            BigRoomNumberChange = 0,
                                            SmallRoomNumberChange = 0,
                                        }
                                    });
                                    reserv.State = ReservationState.INVALIDATED;
                                    context.Changes.Add(new Change()
                                    {
                                        DateTime = DateTime.UtcNow.ToUniversalTime(),
                                        Description = $"Invalidated reservation ID: {reserv.ReserveId} - no avaialable seats",
                                    });
                                }

                                context.Changes.Add(new Change()
                                {
                                    DateTime = DateTime.UtcNow.ToUniversalTime(),
                                    Description = $"Travel no more available seats Id: {utrvl.Id}",
                                });
                            }
                            else if (tcd.AvailableSeats > 0)
                            {
                                // unreserve some, starting from just reservations
                                var rsrv = from reservation in context.Reservations
                                           where reservation.TravelId == utrvl.Id && (reservation.State == ReservationState.RESERVED || reservation.State == ReservationState.PURCHASED)
                                           orderby reservation.State descending
                                           select reservation;
                                int sum = 0;
                                foreach (var reserv in rsrv)
                                {
                                    if (sum + reserv.ReservedSeats <= tcd.AvailableSeats)
                                    {
                                        // reservation can stay
                                        sum += reserv.ReservedSeats;
                                        // check for price change and send update accordingly
                                        if (tcd.Price >= 0 && tcd.Price != utrvl.Price)
                                        {
                                            await ctx.Publish(new ChangesInReservationsEvent()
                                            {
                                                ReservationId = reserv.ReserveId,
                                                ReservationAvailable = true,
                                                ChangesInTransport = new TransportChange()
                                                {
                                                    TransportId = utrvl.Id,
                                                    PlaneAvailable = true,
                                                    FreeSeatsChange = tcd.AvailableSeats,
                                                    ChangeInTransportPrice = tcd.Price,
                                                },
                                                ChangesInHotel = new HotelChange()
                                                {
                                                    HotelId = -1,
                                                    HotelAvailable = false,
                                                    HotelName = "",
                                                    ChangeInHotelPrice = 0,
                                                    WifiAvailable = false,
                                                    BreakfastAvailable = false,
                                                    BigRoomNumberChange = 0,
                                                    SmallRoomNumberChange = 0,
                                                }
                                            });
                                        }
                                        else
                                        {
                                            await ctx.Publish(new ChangesInReservationsEvent()
                                            {
                                                ReservationId = reserv.ReserveId,
                                                ReservationAvailable = true,
                                                ChangesInTransport = new TransportChange()
                                                {
                                                    TransportId = utrvl.Id,
                                                    PlaneAvailable = true,
                                                    FreeSeatsChange = tcd.AvailableSeats,
                                                    ChangeInTransportPrice = utrvl.Price,
                                                },
                                                ChangesInHotel = new HotelChange()
                                                {
                                                    HotelId = -1,
                                                    HotelAvailable = false,
                                                    HotelName = "",
                                                    ChangeInHotelPrice = 0,
                                                    WifiAvailable = false,
                                                    BreakfastAvailable = false,
                                                    BigRoomNumberChange = 0,
                                                    SmallRoomNumberChange = 0,
                                                }
                                            });
                                        }
                                    }
                                    else
                                    {
                                        // reservation not available
                                        if (tcd.Price >= 0 && tcd.Price != utrvl.Price)
                                        {
                                            await ctx.Publish(new ChangesInReservationsEvent()
                                            {
                                                ReservationId = reserv.ReserveId,
                                                ReservationAvailable = false,
                                                ChangesInTransport = new TransportChange()
                                                {
                                                    TransportId = utrvl.Id,
                                                    PlaneAvailable = true,
                                                    FreeSeatsChange = tcd.AvailableSeats,
                                                    ChangeInTransportPrice = tcd.Price,
                                                },
                                                ChangesInHotel = new HotelChange()
                                                {
                                                    HotelId = -1,
                                                    HotelAvailable = false,
                                                    HotelName = "",
                                                    ChangeInHotelPrice = 0,
                                                    WifiAvailable = false,
                                                    BreakfastAvailable = false,
                                                    BigRoomNumberChange = 0,
                                                    SmallRoomNumberChange = 0,
                                                }
                                            });
                                        }
                                        else
                                        {
                                            await ctx.Publish(new ChangesInReservationsEvent()
                                            {
                                                ReservationId = reserv.ReserveId,
                                                ReservationAvailable = false,
                                                ChangesInTransport = new TransportChange()
                                                {
                                                    TransportId = utrvl.Id,
                                                    PlaneAvailable = true,
                                                    FreeSeatsChange = tcd.AvailableSeats,
                                                    ChangeInTransportPrice = utrvl.Price,
                                                },
                                                ChangesInHotel = new HotelChange()
                                                {
                                                    HotelId = -1,
                                                    HotelAvailable = false,
                                                    HotelName = "",
                                                    ChangeInHotelPrice = 0,
                                                    WifiAvailable = false,
                                                    BreakfastAvailable = false,
                                                    BigRoomNumberChange = 0,
                                                    SmallRoomNumberChange = 0,
                                                }
                                            });
                                        }
                                        reserv.State = ReservationState.INVALIDATED;
                                        context.Changes.Add(new Change()
                                        {
                                            DateTime = DateTime.UtcNow.ToUniversalTime(),
                                            Description = $"Invalidated reservation ID: {reserv.ReserveId} - no seats available",
                                        });
                                    }
                                }
                                if (tcd.Price >= 0 && tcd.Price != utrvl.Price)
                                {
                                    // price also changed - update flight
                                    utrvl.Price = tcd.Price;
                                    context.Changes.Add(new Change()
                                    {
                                        DateTime = DateTime.UtcNow.ToUniversalTime(),
                                        Description = $"Price change for travel ID: {utrvl.Id} - new price {utrvl.Price}",
                                    });
                                }
                                utrvl.FreeSeats = tcd.AvailableSeats;
                                context.Changes.Add(new Change()
                                {
                                    DateTime = DateTime.UtcNow.ToUniversalTime(),
                                    Description = $"Available seats change in travel ID: {utrvl.Id} - seats available without reservations {utrvl.FreeSeats}",
                                });
                            }
                            else if (tcd.Price >= 0 && tcd.Price != utrvl.Price)
                            {
                                // just simple price change
                                var rsrv = from reservation in context.Reservations
                                           where reservation.TravelId == utrvl.Id && (reservation.State == ReservationState.RESERVED || reservation.State == ReservationState.PURCHASED)
                                           select reservation;
                                foreach (var reserve in rsrv)
                                {
                                    await ctx.Publish(new ChangesInReservationsEvent()
                                    {
                                        ReservationId = reserve.ReserveId,
                                        ReservationAvailable = true,
                                        ChangesInTransport = new TransportChange()
                                        {
                                            TransportId = utrvl.Id,
                                            PlaneAvailable = true,
                                            FreeSeatsChange = utrvl.FreeSeats,
                                            ChangeInTransportPrice = tcd.Price,
                                        },
                                        ChangesInHotel = new HotelChange()
                                        {
                                            HotelId = -1,
                                            HotelAvailable = false,
                                            HotelName = "",
                                            ChangeInHotelPrice = 0,
                                            WifiAvailable = false,
                                            BreakfastAvailable = false,
                                            BigRoomNumberChange = 0,
                                            SmallRoomNumberChange = 0,
                                        }
                                    });
                                }
                                utrvl.Price = tcd.Price;
                                context.Changes.Add(new Change()
                                {
                                    DateTime = DateTime.UtcNow.ToUniversalTime(),
                                    Description = $"Changed tarvel ID: {utrvl.Id} price to {utrvl.Price}",
                                });
                            }
                        }
                        break;
                    case UpdateTransportTOEvent.Actions.DELETE:
                        if (trvl.Any())
                        {
                            var flight = trvl.Single();
                            // invalidate all reservations
                            var rsrv = from reservation in context.Reservations
                                       where reservation.TravelId == flight.Id && (reservation.State == ReservationState.RESERVED || reservation.State == ReservationState.PURCHASED)
                                       select reservation;
                            foreach (var reserv in rsrv)
                            {
                                await ctx.Publish(new ChangesInReservationsEvent() 
                                { 
                                    ReservationId = reserv.ReserveId,
                                    ReservationAvailable = false,
                                    ChangesInTransport = new TransportChange()
                                    {
                                        TransportId = flight.Id,
                                        PlaneAvailable = false,
                                        FreeSeatsChange = 0,
                                        ChangeInTransportPrice = 0,
                                    },
                                    ChangesInHotel = new HotelChange()
                                    {
                                        HotelId = -1,
                                        HotelAvailable = false,
                                        HotelName = "",
                                        ChangeInHotelPrice = 0,
                                        WifiAvailable = false,
                                        BreakfastAvailable = false,
                                        BigRoomNumberChange = 0,
                                        SmallRoomNumberChange = 0,
                                    }
                                });
                                reserv.State = ReservationState.INVALIDATED;
                                context.Changes.Add(new Change()
                                {
                                    DateTime = DateTime.UtcNow.ToUniversalTime(),
                                    Description = $"Invalidated reservation ID: {reserv.ReserveId} - travel cancelled",
                                });
                            }

                            context.Changes.Add(new Change()
                            {
                                DateTime = DateTime.UtcNow.ToUniversalTime(),
                                Description = $"Travel cancelled Id: {flight.Id}",
                            });
                            context.Remove(flight);
                        }
                        break;
                    default:
                        break;
                }
                if (context.ChangeTracker.HasChanges())
                    Console.WriteLine($"Transport: Change {action} in table TRAVEL");

                await context.SaveChangesAsync();
            }
        }
    }
}
