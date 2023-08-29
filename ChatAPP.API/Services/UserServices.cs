using ChatAPP.API.Contexts;
using ChatAPP.API.Models.DTO;
using ChatAPP.API.Models.PostModels;
using ChatAPP.API.Utils;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatAPP.API.Services
{
    public class UserServices : BaseServices<User>
    {
        private readonly string _signKey;
        private readonly string _scryptKey;

        public UserServices(ChatAPPContext context, ILogger<User> logger, IConfiguration configuration) : base(context, logger)
        {
            _signKey = configuration.GetValue<string>("JWT:SignKey");
            _scryptKey = configuration.GetValue<string>("SecrectKey");
        }

        public DefaultResponse Login(LoginPostModel model)
        {
            try
            {
                var user = _context.Users.Where(x => x.Username == model.UserName).FirstOrDefault()!;
                if (user == null || CryptoHelper.Decrypt(user.Password, _scryptKey) != model.Password)
                {
                    return new DefaultResponse { Message = "Login failed" };
                }
                var token = JwtUtils.GenerateToken(user, _signKey);
                user.Password = null!;
                var result = new LoginResponse { Token = token, Message = "Login success", Data = user };

                return new DefaultResponse { Message = "Login success", Data = result };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new DefaultResponse { Message = "Login failed" };
            }
        }

        public DefaultResponse Register(RegisterPostModel model)
        {
            try
            {
                if(_context.Users.Any(x => x.Username == model.Username))
                {
                    return new DefaultResponse { Message = "Username is already taken" };
                }

                if(model.Password != model.ConfirmPassword)
                {
                    return new DefaultResponse { Message = "Password and confirm password is not match" };
                }

                model.Password = CryptoHelper.Encrypt(model.Password!, _scryptKey);

                var user = new User
                {
                    Username = model.Username!,
                    Password = model.Password!,
                    FileId = model.FileId,
                };
                user.Id = Add(user);
                if (user.Id == 0)
                {
                    return new DefaultResponse { Message = "Register failed" };
                }
                user.Password = null!;
                var token = JwtUtils.GenerateToken(user, _signKey);
                return new DefaultResponse { Message = "Register success", Data = new { user, token } };
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return new DefaultResponse { Message = "Register failed" };
            }
        }

        public User GetProfile(int userId)
        {
            try
            {
                var user = GetById(userId);
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return null!;
            }
        }

    }
}
