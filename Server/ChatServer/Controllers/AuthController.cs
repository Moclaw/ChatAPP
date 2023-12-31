﻿using ChatServer.Models.PostModels;
using ChatServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ChatServer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserServices _userServices;

        public AuthController(UserServices userServices)
        {
            _userServices = userServices;
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult Login(LoginPostModel model)
        {
            try
            {
                var result = _userServices.Login(model);
                if (result.Data == null)
                {
                    return BadRequest(new DefaultResponse { Message = result.Message });
                }

                return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
            }
            catch (Exception e)
            {
                return BadRequest(new DefaultResponse { Message = e.Message });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult Register(RegisterPostModel model)
        {
            var result = _userServices.Register(model);
            if (result.Data == null)
            {
                return BadRequest(new DefaultResponse { Message = result.Message });
            }
            return Ok(new DefaultResponse { Message = result.Message, Data = result.Data });
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetProfile()
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");

            if (userId == 0)
            {
                return BadRequest(new DefaultResponse { Message = "User Not Found" });
            }
            var user = _userServices.GetProfile(userId);
            if (user == null)
            {
                return NotFound(new DefaultResponse { Message = "User Not Found" });
            }

            return Ok(new DefaultResponse { Message = "Success", Data = user });
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetUser()
        {
            int userId = int.Parse(this.User.FindFirst("userId")?.Value ?? "0");
            var user = _userServices.GetUsers(userId);
            if (user == null)
            {
                return NotFound(new DefaultResponse { Message = "User Not Found" });
            }

            return Ok(new DefaultResponse { Message = "Success", Data = user });
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesDefaultResponseType]
        public ActionResult GetUserById(int id)
        {
            var user = _userServices.GetUserById(id);
            if (user == null)
            {
                return NotFound(new DefaultResponse { Message = "User Not Found" });
            }

            return Ok(new DefaultResponse { Message = "Success", Data = user });
        }

    }
}
