using ChatServer.Models.PostModels;
using ChatServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChannelController : ControllerBase
    {
        private readonly ChannelServices _channelServices;

        public ChannelController(ChannelServices channelServices)
        {
            _channelServices = channelServices;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> CreateChannel(ChannelPostModel model)
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var result = await _channelServices.CreateChannel(model, userId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }

        [HttpPost("{channelId}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> SendMessage(int channelId, MessagePostModel model)
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var result = await _channelServices.SendMessage(channelId, model, userId);
            if (result == null)
            {
                return BadRequest(new DefaultResponse { Message = result?.Message! });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }

        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> DeleteChannel(int channelId)
        {
            var result = await _channelServices.DeleteChannel(channelId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetChannels()
        {
            var result = _channelServices.GetChannels();
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data, Count = result.Count });
        }

        [HttpGet("{userId}/channel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetChannelsByUserId(int userId)
        {
            var result = _channelServices.GetChannelsByUserId(userId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data, Count = result.Count });
        }

        [HttpPut("{channelId}/updateStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> UpdateChannelStatus(int channelId)
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var result = await _channelServices.UpdateChannelStatus(channelId, userId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }

        [HttpGet("{channelId}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult> GetChannelMessagesByUserId(int channelId)
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var result =  await _channelServices.GetChannelMessagesByUserId(channelId, userId);
            if (result == null)
            {
                return BadRequest(new DefaultResponse { Message = "Channel not found" });
            }

            return Ok(
                new DefaultResponse { Message = "Get channel messages success", Data = result.Data, Count =result.Count }
            );
        }

        [HttpGet("{channelId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetChannel(int channelId)
        {
            var result = _channelServices.GetChannel(channelId);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }
    }
}
