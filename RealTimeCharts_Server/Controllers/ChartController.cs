using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeCharts_Server.DataStorage;
using RealTimeCharts_Server.HubConfig;
using RealTimeCharts_Server.TimerFeatures;
using StackExchange.Redis;
using Newtonsoft.Json;
using RealTimeCharts_Server.Models;

namespace RealTimeCharts_Server.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChartController : ControllerBase
    {
        private IHubContext<ChartHub> _hub;
        private IConnectionMultiplexer _redis;

        public ChartController(IHubContext<ChartHub> hub, IConnectionMultiplexer redis)
        {
            _hub = hub;
            _redis = redis;
        }

        public IActionResult Get()
        {
            ISubscriber sub = _redis.GetSubscriber();
            var timerManager = new TimerManager(() => sub.Publish("chartDataUpdate", JsonConvert.SerializeObject(DataManager.GetData())));

            sub.Subscribe("chartDataUpdate", (channel, message) => {
                _hub.Clients.All.SendAsync("transferchartdata", JsonConvert.DeserializeObject<List<ChartModel>>(message));
            });

            return Ok(new { Message = "Request Completed" });

        }
    }
}