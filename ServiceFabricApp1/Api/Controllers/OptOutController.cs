using Common;
using Common.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    public class OptOutController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]OptOut request)
        {
            var service = ServiceProxy.Create<IMyStatefulService>(new Uri(ServiceEndpoints.MyStatefulService), new ServicePartitionKey(0));

            if (!ModelState.IsValid)
            {
                return BadRequest("One or more required properties were missing.  Please check and try again.");
            }

            try
            {
                await service.AddToQueueAsync(request);
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            List<OptOut> optouts;
            var actor = ActorProxy.Create<IActor1>(ActorId.CreateRandom(), new Uri(ServiceEndpoints.Actor1));

            try
            {
                optouts = await actor.GetOptOutsAsync();
            }
            catch (Exception ex)
            {
                ServiceEventSource.Current.Error(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return Json(optouts);
        }
    }
}