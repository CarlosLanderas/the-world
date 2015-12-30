using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity;
using Microsoft.Framework.Logging;
using TheWorld.Controllers.Api;

namespace TheWorld.Models
{
    public class WorldRepository : IWorldRepository
    {
        private WorldContext _context;
        private ILogger<WorldRepository> _logger;

        public WorldRepository(WorldContext context, ILogger<WorldRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<Trip> GetAllTrips()
        {
            try
            {
                return _context.Trips.OrderBy(t => t.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get trips from database", ex);
                return null;
            }
        }

        public IEnumerable<Trip> GetAllTripsWithStops()
        {
            try
            {
                return _context.Trips
                .Include(t => t.Stops)
                .OrderBy(t => t.Name)
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get trips with stops from database", ex);
                return null;
            }
        }

        public Trip GetTripByName(string tripName, string username)
        {
            return _context.Trips.Include(t => t.Stops).FirstOrDefault(
                t => string.Equals(t.Name, tripName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(t.UserName, username, StringComparison.CurrentCultureIgnoreCase)
                );
        }

        public void AddStop(string tripName, string userName, Stop newStop)
        {
            var theTrip = GetTripByName(tripName, userName);

            var order = theTrip.Stops.Any() ? theTrip.Stops.Max(s => s.Order) + 1 : 1;
            newStop.Order = order;
            theTrip.Stops.Add(newStop);
            _context.Stops.Add(newStop);
        }

        public IEnumerable<Trip> GetUserTripsWithStops(string name)
        {
            try
            {
                return _context.Trips
                .Include(t => t.Stops)
                .Where(u => u.UserName == name)
                .OrderBy(t => t.Name)
                .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Could not get trips with stops from database", ex);
                return null;
            }

        }

        public void AddTrip(Trip newTrip)
        {
            _context.Add(newTrip);
        }

        public bool SaveAll()
        {
            return _context.SaveChanges() > 0;
        }


    }
}
