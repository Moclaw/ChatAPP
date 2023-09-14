using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationServices _notificationServices;
        public NotificationController(NotificationServices notificationServices)
        {
            _notificationServices = notificationServices;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetNotifications()
        {
            int userId = int.Parse(this.User.Claims.FirstOrDefault(c => c.Type == "userId")!.Value);
            var result = _notificationServices.GetNotifications(userId);
            if (result == null)
            {
                return BadRequest(new DefaultResponse { Message = "Get notifications failed" });
            }
            return Ok(new DefaultResponse { Message = "Get notifications success", Data = result, Count = result.Count });
        }

        [HttpPost("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult ReadNotification(int id)
        {
            var result = _notificationServices.ReadNotification(id);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult ReadAllNotifications()
        {
            int userId = int.Parse(User.Claims.First(c => c.Type == "UserId").Value);
            var result = _notificationServices.ReadAllNotifications(userId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message });
        }
       
    }
}

