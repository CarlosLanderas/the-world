using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TheWorld.Controllers.Api
{ 
    [Route("api/trips/{tripName}/stops")]
    public class StopController : Controller
    {
        private ILogger<StopController> _logger;
        private IWorldRepository _repository;
        private ICoordService _coordService;

        public StopController(IWorldRepository repository, ILogger<StopController> logger, ICoordService coordService)
        {
            _repository = repository;
            _logger = logger;
            _coordService = coordService;
        }
        // GET: /<controller>/
        public JsonResult Get(string tripName)
        {
            try
            {
                var results = _repository.GetTripByName(tripName, User.Identity.Name);
                if (results == null)
                {
                    return Json(null);
                }

                return Json(Mapper.Map<IEnumerable<StopViewModel>>(results.Stops.OrderBy(s=>s.Order)));
            }
            catch (Exception ex)
            {
               _logger.LogError($"Failed to get stops for trip {tripName}",ex);
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json("Error ocurred finding trip name");
            }
        }

        [HttpPost]
        public async Task<JsonResult> Post(string tripName, [FromBody] StopViewModel vm)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //Map to the entity
                    var newStop = Mapper.Map<Stop>(vm);
                    //Looking up GeoCoordinates

                    var coordResult = await _coordService.LookUp(newStop.Name);
                    if (!coordResult.Success)
                    {
                        Response.StatusCode = (int) HttpStatusCode.BadRequest;
                        Json(coordResult.Message);
                    }

                    newStop.Longitude = coordResult.Longitude;
                    newStop.Latitude = coordResult.Latitude;

                    _repository.AddStop(tripName, User.Identity.Name, newStop);
                    //Save to the database
                    if (_repository.SaveAll())
                    {
                        Response.StatusCode = (int) HttpStatusCode.Created;
                        return Json(Mapper.Map<StopViewModel>(newStop));
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new stop", ex);
                Response.StatusCode = (int) HttpStatusCode.BadRequest;
                return Json("Failed to save new stop");
            }

            Response.StatusCode = (int) HttpStatusCode.BadRequest;
            return Json(new {Errors = ModelState.Values.SelectMany(e => e.Errors).Select(m => m.ErrorMessage)});
        }
    }
}
