using ChatServer.Data;
using ChatServer.Models.Entities;
using ChatServer.Models.PostModels;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace ChatServer.Services
{
    public class UserServices
    {
        private readonly string _signKey;
        private readonly string _scryptKey;
        private readonly ChatAPPContext _context;
        private readonly ILogger<UserServices> _logger;

        public UserServices(ChatAPPContext context, ILogger<UserServices> logger, IConfiguration configuration)
        {
            _signKey = configuration.GetValue<string>("JWT:SignKey");
            _scryptKey = configuration.GetValue<string>("SecrectKey");
            _context = context;
            _logger = logger;
        }

        public DefaultResponse Login(LoginPostModel model)
        {
            try
            {
                var user = _context.Users.Where(x => x.Username == model.UserName).FirstOrDefault()!;
                if (user == null || CryptoHelper.Decrypt(user.Password!, _scryptKey) != model.Password)
                {
                    return new DefaultResponse { Message = "Login failed" };
                }
                var token = JwtUtils.GenerateToken(user, _signKey);
                user.Password = null!;
                var result = new { Token = token, Data = user };

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
                if (_context.Users.Any(x => x.Username == model.Username))
                {
                    return new DefaultResponse { Message = "Username is already taken" };
                }

                if (model.Password != model.ConfirmPassword)
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
                _context.Users.Add(user);
                _context.SaveChanges();

                if (user == null)
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

        public object? GetProfile(int userId)
        {
            try
            {
                var user = _context.Users
                    .Include(x => x.ChannelMemberships)
                  .Include(x => x.File)
                  .Include(x => x.Notifications)
                  .ThenInclude(x => x.Message)
                  .Where(x => x.Id == userId)
                  .FirstOrDefault();

                if (user == null)
                {
                    return null;
                }

                var result = new
                {
                    user.Id,
                    user.Username,
                    File = new
                    {
                        user?.File?.Path,
                        user?.File?.Filename
                    },
                    Notifications = user?.Notifications != null ?
                      user?.Notifications?.Select(x => new
                      {
                          x.Id,
                          x.MessageId,
                          x?.IsRead,
                          Message = new
                          {
                              x?.Message?.Id,
                              x?.Message?.Content,
                              x?.Message?.UserId,
                              Sender = new
                              {
                                  x?.Message?.User?.Id,
                                  x?.Message?.User?.Username,
                                  File = new
                                  {
                                      x?.Message?.User?.File?.Path,
                                      x?.Message?.User?.File?.Filename
                                  }
                              }
                          }
                      }) : null,
                    ChannelMemberships = user?.ChannelMemberships != null ? user?.ChannelMemberships?.Select(x => new
                    {
                        x.Id,
                        x.ChannelId,
                        x?.UserId,
                        Channel = new
                        {
                            x?.Channel?.Id,
                            x?.Channel?.Name,
                        }
                    }) : null
                };


                return user;
            }
            catch (Exception e)
            {
                _logger.LogError(e?.Message);
                return null;
            }
        }

    }
}
