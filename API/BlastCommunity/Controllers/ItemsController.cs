using System.Text;
using System.Xml.Linq;

using Blast_Community.HelpClass;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Blast_Community.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        /// <summary>
        /// 插入id
        /// </summary>
        /// <returns>明细表插入ID</returns>
        private int InsertID()
        {
            int next = 1;
            using (var db = new Entity.Models.Context())
            {
                int rows = db.groupinfos.Count();
                if (rows != 0)
                {
                    int max = db.groupinfos.Max(e => e.Id);
                    if (rows != max)
                    {
                        int min = db.groupinfos.Min(e => e.Id);
                        var all = Enumerable.Range(min, max - min + 1).ToList();
                        var exits = db.groupinfos.Select(e => e.Id).ToList();
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
        /// 所有分组
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetGroupItems")]
        public string GetGroupItems()
        {
            using (var db = new Entity.Models.Context())
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("SELECT DISTINCT a.* FROM `grouping` as a ");
                strSql.Append("JOIN `groupinfo` as b on a.Id = b.GId ");
                strSql.Append($"WHERE b.`delete` = {(int)TypeEnum.Boolean.False} ");
                strSql.Append("ORDER BY sort ASC");

                var groupList = db.groupings.FromSqlRaw(strSql.ToString()).ToList();

                return JsonConvert.SerializeObject(groupList);
            }
        }

        /// <summary>
        /// 查询分组
        /// </summary>
        /// <param name="find"></param>
        /// <returns></returns>
        [HttpGet("GetGroupItemsFind")]
        public string GetGroupItems(string find)
        {
            using (var db = new Entity.Models.Context())
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append("SELECT DISTINCT a.* FROM `grouping` as a ");
                strSql.Append("JOIN `groupinfo` as b on a.Id = b.GId ");
                strSql.Append($"WHERE b.`delete` = {(int)TypeEnum.Boolean.False} AND a.ItemName LIKE '%{find}%'");
                strSql.Append("ORDER BY sort ASC");

                var groupList = db.groupings.FromSqlRaw(strSql.ToString()).ToList();

                return JsonConvert.SerializeObject(groupList);
            }
        }

        /// <summary>
        /// 首字母排序分组成json集合
        /// </summary>
        /// <param name="InfoList"></param>
        /// <returns></returns>
        private JArray? ListToGroupJson(List<Entity.Models.groupinfo> InfoList)
        {
            if (InfoList != null)
            {
                JArray jsonArray = new JArray();

                var task = Task.Run(async () =>
                {
                    var group = InfoList
                        .GroupBy(e => e.sort)
                        .Select(group => new { sort = group.Key })
                        .ToArray();

                    foreach (var items in group)
                    {
                        var tmpList = InfoList
                            .Where(e => e.sort == items.sort)
                            .ToList();

                        List<groupinfoJson> list = new List<groupinfoJson>();
                        List<Task> tasks = new List<Task>();
                        object ObjectLock = new object();

                        foreach (var item in tmpList)
                        {
                            tasks.Add(Task.Run(() =>
                            {
                                var newinfo = new groupinfoJson
                                {
                                    Id = item.Id,
                                    GId = item.GId,
                                    ItemName = item.ItemName,
                                    Item_Remarks = item.Item_Remarks,
                                    createTime = item.createTime.ToString("yyyy/MM/dd HH:mm:ss"),
                                    sort = item.sort,
                                    creator = item.creator,
                                    complaint = item.complaint,
                                    delete = item.delete
                                };

                                lock (ObjectLock)
                                {
                                    list.Add(newinfo);
                                }
                            }));
                        }
                        await Task.WhenAll(tasks);

                        //循环引用报错？？？ 这里加了Ignore处理
                        //JArray json = JArray.FromObject(list, JsonSerializer.Create(new JsonSerializerSettings
                        //{
                        //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                        //}));
                        JArray json = JArray.FromObject(list);
                        jsonArray.Add(json);
                    }
                });
                task.Wait();
                return jsonArray;
            }
            else
            {
                return null;
            }
        }

        private class groupinfoJson : Entity.Models.groupinfo
        {
            //json屏蔽字段 改写部分数据

            public string createTime { get; set; }
            [JsonIgnore]
            public virtual Entity.Models.grouping GIdNavigation { get; set; }
            [JsonIgnore]
            public virtual Entity.Models.user creatorNavigation { get; set; }
        }

        /// <summary>
        /// 所有项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetItemsList")]
        public string GetItemsList(int id)
        {
            using (var db = new Entity.Models.Context())
            {
                var plus = db.groupings.Where(e => e.Id == id).ToList().FirstOrDefault();
                if (plus != null)
                {
                    plus.statistics += 1;
                    db.Update(plus);
                    db.SaveChanges();
                }

                var itemsList = db.groupinfos
                    .Where(e => e.GId == id && e.delete == (int)TypeEnum.Boolean.False)
                    .OrderBy(e => e.sort)
                    .ToList();

                if (itemsList != null)
                {
                    return JsonConvert.SerializeObject(ListToGroupJson(itemsList));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// 模糊查询项目
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("GetItemsListFind")]
        public string GetItemsList(int id, string find)
        {
            using (var db = new Entity.Models.Context())
            {
                var itemsList = db.groupinfos
                    .Where(e => e.GId == id && e.delete == (int)TypeEnum.Boolean.False && e.ItemName.Contains(find))
                    .OrderBy(e => e.sort)
                    .ToList();

                if (itemsList != null)
                {
                    return JsonConvert.SerializeObject(ListToGroupJson(itemsList));
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [HttpPost("Contribution")]
        public string Contribution([FromBody] object addItem)
        {
            var outjson = new AddItemJson();

            try
            {
                //UserKey GroupName Item Remarks
                JObject json = JObject.Parse(addItem.ToString());
                int uid = 0;
                try
                {
                    uid = Convert.ToInt32(JObject.Parse(CalcFunction.Base64ToObject(json["UserKey"].ToString()).ToString())["Id"].ToString());

                }
                catch (Exception)
                {
                    outjson = new AddItemJson()
                    {
                        Code = -1,
                        Error = "登录过期"
                    };
                }
                
                string groupName = json["GroupName"].ToString().Trim();
                using (var db = new Entity.Models.Context())
                {
                    var group = db.groupings.Where(e => e.ItemName == groupName).ToList().FirstOrDefault();
                    string item = json["Item"].ToString().Trim();

                    if (group == null)
                    {
                        group = new Entity.Models.grouping
                        {
                            Id = db.groupings.Any() ? db.groupings.Max(e => e.Id) + 1 : 1,
                            ItemName = groupName,
                            createTime = DateTime.Now,
                            statistics = 0,
                            sort = CalcFunction.InitialChar(groupName)
                        };
                        db.groupings.Add(group);
                    }
                    else
                    {
                        var exist = db.groupinfos.Where(e => e.GId == group.Id && e.ItemName == item).ToList().FirstOrDefault();
                        if (exist != null) 
                        {
                            return JsonConvert.SerializeObject(new AddItemJson
                            {
                                Code = 502,
                                Error = "该项目已经存在"
                            });
                        }
                    }

                    
                    var NewItem = new Entity.Models.groupinfo
                    {
                        Id = InsertID(),
                        GId = group.Id,
                        ItemName = item,
                        Item_Remarks = json["Remarks"].ToString().Trim(),
                        createTime = DateTime.Now,
                        sort = CalcFunction.InitialChar(item),
                        creator = uid,
                        complaint = (int)TypeEnum.Boolean.False,
                        delete = (int)TypeEnum.Boolean.False
                    };
                    db.groupinfos.Add(NewItem);

                    if (!string.IsNullOrEmpty(item) && !string.IsNullOrEmpty(groupName))
                    {
                        db.SaveChanges();
                    }
                    else
                    {
                        outjson = new AddItemJson()
                        {
                            Code = 500,
                            Error = "名称不能为空"
                        };
                    }
                }
                
            }
            catch (Exception ex)
            {
                outjson = new AddItemJson()
                {
                    Code = 0,
                    Error = "服务异常"
                };
            }

            return JsonConvert.SerializeObject(outjson);
        }

        private class AddItemJson
        {
            public int Code { get; set; } = 200;
            public string? Error { get; set; }
        }
    }
}
