using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Authorization;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Api
{
    [Authorize]
    [Route("api/trips")]
    public class TripController : Controller
    {
        private IWorldRepository _worldRepository;
        private ILogger<TripController> _logger;

        public TripController(IWorldRepository worldRepository, ILogger<TripController> logger)
        {
            _worldRepository = worldRepository;
            _logger = logger;
        }
        [HttpGet]
        public JsonResult Get()
        {
            //Thread.Sleep(2000);
            var trips = _worldRepository.GetUserTripsWithStops(User.Identity.Name);
            return Json(Mapper.Map<IEnumerable<TripViewModel>>(trips));
        }

        [HttpPost]
        public JsonResult Post([FromBody]TripViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newTrip = Mapper.Map<Trip>(vm);
                    newTrip.UserName = User.Identity.Name;
                    //Save to the database
                    _logger.LogInformation("Attempting to save a new trip");
                    _worldRepository.AddTrip(newTrip);
                    if (_worldRepository.SaveAll())
                    {
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        return Json(Mapper.Map<TripViewModel>(newTrip));
                    }
                }

                var errorsQuery = from errors in ModelState.Values.SelectMany(e => e.Errors)
                    select errors.ErrorMessage;
                return Json(new {Message = "Failed", Errors = errorsQuery});
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new trip", ex);
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json(new {Message = ex.Message});
            }
        }
    }
}
