using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Mvc;
using Microsoft.Framework.Logging;
using TheWorld.Models;
using TheWorld.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Api
{
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
        public JsonResult Index()
        {
            return Json(Mapper.Map<IEnumerable<TripViewModel>>(_worldRepository.GetAllTripsWithStops()));
        }

        [HttpPost]
        public JsonResult Post([FromBody]TripViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var newTrip = Mapper.Map<Trip>(vm);
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
