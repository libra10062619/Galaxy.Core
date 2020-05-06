using System.Collections.Generic;
using System.Linq;
using FocusClientSample.Infrustructures;
using Galaxy.Extensions.FocusClient;
using Microsoft.AspNetCore.Mvc;

namespace FocusClientSample.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        readonly IRepository<User> _dataProvider;
        public UsersController(IRepository<User> dataProvider)
        {
            _dataProvider = dataProvider;
        }
        // GET: api/Users
        [HttpGet]    
        [FocusClient("{pgIndex:int}/{pgSize:range(10,100)}", Name = "GetPagingUsers",
            Description = @"获取用户列表分页数据。param0：分页号，从0开始；param1：每页条目数")]
        [FocusClient("{pgIndex:int}/{pgSize:range(10,100)}/{keywords?}", Name = "GetPagingUsersByKeywords", 
            Description = @"获取用户列表分页数据。param0：分页号，从0开始；param1：每页条目数；param2：查询参数")]
        public IEnumerable<User> Get(int pgIndex=0, int pgSize=10, string keywords = null)
        {
            var data = _dataProvider.GetAll();
            var query = keywords == null ? data.Where(p => p.Username.Contains(keywords)) : data;
            var result = query.Skip(pgIndex * pgSize).Take(pgSize);
            return result;
        }

        // GET: api/Users/5
        [FocusClient("{id}", Name = "GetById", Description = @"通过用户Id获取用户信息")]
        public User Get(int id)
        {
            var result = _dataProvider.Get(id);
            return result;
        }

        // POST: api/Users
        [HttpPost]
        public void Post([FromBody] User value)
        {
            _dataProvider.Add(value);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] User value)
        {
            value.Id = id;
            _dataProvider.Update(value);
        }

        // DELETE: api/ApiWithActions/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            _dataProvider.Remove(id);
        }
    }
}
