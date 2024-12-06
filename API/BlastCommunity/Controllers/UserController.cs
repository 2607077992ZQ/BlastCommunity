using Blast_Community.HelpClass;

using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blast_Community.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        /// <summary>
        /// 插入id
        /// </summary>
        /// <returns></returns>
        private int InsertID()
        {
            int next = 1;
            using (var db = new Entity.Models.Context())
            {
                int rows = db.users.Count();
                if (rows != 0)
                {
                    int max = db.users.Max(e => e.Id);
                    if (rows != max)
                    {
                        int min = db.users.Min(e => e.Id);
                        var all = Enumerable.Range(min, max - min + 1).ToList();
                        var exits = db.users.Select(e => e.Id).ToList();
                        var misslist = all.Except(exits).ToList();
                        next = misslist.FirstOrDefault();
                    }
                    else
                    {
                        next = max + 1;
                    }
                }
            }

            return next;
        }

        /// <summary>
        /// 核验用户是否存在
        /// </summary>
        /// <param name="user"></param>
        /// <returns>用户存在时为真</returns>
        [HttpPost("Verification")]
        public bool Verification(string userName)
        {
            try
            {
                using (var db = new Entity.Models.Context())
                {
                    var user = db.users.Where(e => e.username == userName).ToList().FirstOrDefault();
                    if (user != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                throw new Exception("请求超时");
            }
            
        }

        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="loginjson"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public string Login([FromBody] object loginjson)
        {
            var json = new LoginJson();

            try
            {
                JObject value = JObject.Parse(loginjson.ToString());
                string username = value["user"].ToString();
                string pwd = value["password"].ToString();


                if (Verification(username))
                {
                    using (var db = new Entity.Models.Context())
                    {
                        var user = db.users.Where(e => e.username == username).ToList().FirstOrDefault();
                        if (user != null && user.pwd == CalcFunction.GetSha256Hash(pwd))
                        {
                            if (user.banned == (int)TypeEnum.Boolean.True)
                            {
                                json = new LoginJson()
                                {
                                    Code = -1,
                                    Error = $"该账号已被Ban:原因:{user.banned_remarks}"
                                };
                            }
                            else if (user.enable == (int)TypeEnum.State.Disable)
                            {
                                json = new LoginJson()
                                {
                                    Code = -1,
                                    Error = "该账号已被禁止登录"
                                };
                            }
                            else
                            {
                                json = new LoginJson()
                                {
                                    UserKey = CalcFunction.ObjectToBase64(user),
                                    Name = username,
                                };

                                user.loginTime = DateTime.Now;
                                db.Update(user);
                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            json = new LoginJson
                            {
                                Code = 500,
                                Error = "账号或密码错误"
                            };
                        }
                    }
                }
                else
                {
                    json = new LoginJson
                    {
                        Code = 500,
                        Error = "账号或密码错误"
                    };
                }
            }
            catch (Exception)
            {
                json = new LoginJson
                {
                    Code = 0,
                    Error = "参数异常"
                };
            }

            return JsonConvert.SerializeObject(json);
        }

        /// <summary>
        /// 注册
        /// </summary>
        /// <param name="registerJson"></param>
        /// <returns></returns>
        [HttpPost("Register")]
        public string Register([FromBody] object registerJson)
        {
            var state = new RegisterJson();

            try
            {
                JObject value = JObject.Parse(registerJson.ToString());
                string userName = value["name"].ToString();
                string pwd = value["password"].ToString();
                if (userName != string.Empty)
                {
                    if (!Verification(userName))
                    {
                        using (var db = new Entity.Models.Context())
                        {
                            var user = new Entity.Models.user()
                            {
                                Id = InsertID(),
                                UID = CalcFunction.GuidStr(),
                                username = userName,
                                pwd = CalcFunction.GetSha256Hash(pwd),
                                role = (int)TypeEnum.Role.user,
                                banned = (int)TypeEnum.Boolean.False,
                                enable = (int)TypeEnum.State.Enable,
                                createTime = DateTime.Now
                            };
                            db.users.Add(user);
                            if(db.SaveChanges() > 0)
                            {
                                state.State = true;
                            }
                            else
                            {
                                state = new RegisterJson()
                                {
                                    Code = -1,
                                    error = "注册时出现未知错误"
                                };
                            }
                        }
                    }
                    else
                    {
                        state = new RegisterJson()
                        {
                            Code = -1,
                            error = "名称已被占用"
                        };
                    }
                }
                else
                {
                    state = new RegisterJson()
                    {
                        Code = -1,
                        error = "名称不合法"
                    };
                }
            }
            catch (Exception)
            {
                state = new RegisterJson
                {
                    Code = 0,
                    error = "参数异常"
                };
            }

            return JsonConvert.SerializeObject(state);
        }

        #region 返回的json信息

        /// <summary>
        /// 登录的身份
        /// </summary>
        private class LoginJson
        {
            public int Code { get; set; } = 200;
            public string? UserKey { get; set; }
            public string? Name { get; set; }
            public string? Error {  get; set; }
        }

        /// <summary>
        /// 注册返回的json
        /// </summary>
        private class RegisterJson
        {
            public int Code { get; set; } = 200;
            public bool State { get; set; } = false;
            public string? error { get; set; }
        }

        #endregion
    }
}
