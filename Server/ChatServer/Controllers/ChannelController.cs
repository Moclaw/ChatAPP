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
		public async Task<ActionResult> CreateChannel(ChannelPostModel model)
		{
			var result = await _channelServices.CreateChannel(model);
			if (result.Data == null)
			{
				return BadRequest(new DefaultResponse { Message = result.Message });

			}
			return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
		}

		[HttpPost("{channelId}/messages")]
		public async Task<ActionResult> SendMessage(int channelId, MessagePostModel model)
		{
			var result = await _channelServices.SendMessage(channelId, model);
			if (result == null)
			{
				return BadRequest(new DefaultResponse { Message = result?.Message });
			}
			return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
		
		}

		[HttpGet]
		public ActionResult GetChannels()
		{
			var result = _channelServices.GetChannels();
			if (result.Data == null)
			{
				return BadRequest(new DefaultResponse { Message = result.Message });
			}
			return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
		}

		[HttpGet("{channelId}")]
		public ActionResult GetChannel(int channelId)
		{
			var result = _channelServices.GetChannel(channelId);
			if (result.Data == null)
			{
				return BadRequest(new DefaultResponse { Message = result.Message });
			}
			return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
		}

		[HttpGet("{channelId}/messages")]
		public ActionResult GetChannelMessages(int channelId)
		{
			var result = _channelServices.GetChannelMessages(channelId);
			if (result.Data == null)
			{
				return BadRequest(new DefaultResponse { Message = result.Message });
			}
			return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
		}

    }
}
