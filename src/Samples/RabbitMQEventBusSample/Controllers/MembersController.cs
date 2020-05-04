using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Galaxy.Core.EventBus.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RabbitMQEventBusSample.Events;
using RabbitMQEventBusSample.DTO;

namespace RabbitMQEventBusSample.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MembersController : ControllerBase
    {
        readonly IEventBus _eventBus;
        readonly IRepository<MemberInfo, string> _repository;

        public MembersController(IEventBus eventBus, IRepository<MemberInfo, string> repository)
        {
            _eventBus = eventBus;
            _repository = repository;
        }

        [HttpGet("{id}")]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(MemberInfo), (int)HttpStatusCode.OK)]
        public async Task<ActionResult<MemberInfo>> GetById(string id)
        {
            await Task.Yield();
            return _repository.Get(id);
        }

        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> Post([FromBody] MemberInfo body)
        {
            _repository.Add(body);

            var @evt = new MemberCreatedEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                MemberId = body.Id,
                MemberName = body.MemberName
            };

            await _eventBus.PublishAsync(@evt);

            return CreatedAtAction(nameof(GetById), new { id = body.Id }, null);
        }

        [HttpPut]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        public async Task<ActionResult> Put([FromBody] MemberInfo body)
        {
            await Task.Yield();
            var item = _repository.Get(body.Id);
            if (item == null) return NotFound(new { Message = $"Not found item with id {body.Id}" });

            _repository.Update(body);

            var @evt = new MemberUpdatedEvent
            {
                Timestamp = DateTimeOffset.UtcNow,
                MemberId = body.Id,
                MemberName = body.MemberName
            };

            await _eventBus.PublishAsync(@evt);
           
            return CreatedAtAction(nameof(GetById), new { id = body.Id }, null);
        }
    }
}