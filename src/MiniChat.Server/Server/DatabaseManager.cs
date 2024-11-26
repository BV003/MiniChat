using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using MiniChat.Transmitting;
namespace MiniChat.Server
{
    public static class DatabaseManager
    {
        /// <summary>
        /// 异步的执行数据库操作
        /// </summary>
        /// <param name="user">用户对象</param>
        /// <param name="operation">操作内容</param>
        /// <param name="parameter">操作附带的参数</param>
        public static async Task<RequestResult> ExecuteDBAction(User user, string operation, string parameter = "")
        {
            if (user != null && (!operation?.Equals(string.Empty)) == true && parameter != null)
            {
                RequestResult result = new RequestResult();
                switch (operation)
                {
                    case "Signup":
                        switch (await CheckUserExistsAsync(user.UserName))
                        {
                            case true:
                                result.Success = false;
                                break;
                            case false:
                                if (await UserSignupAsync(user))
                                    result.Success = true;
                                break;
                        }
                        break;
                    case "Login":
                        switch (await CheckUserExistsAsync(user.UserName, user.Password))
                        {
                            case true:
                                if (await GetUserStateAsync(user.UserName) == 0)
                                    if (await UserOnlineAsync(user.UserName))
                                        result.Success = true;
                                break;
                            case false:
                                result.Success = false;
                                break;
                        }
                        break;
                    case "AlterInfo":
                        if (await AlterUserInfoAsync(user) == true)
                            result.Success = true;
                        break;
                    case "AlterPassword":
                        result.Success = await AlterUserPasswordAsync(user.UserName, user.Password, parameter);
                        break;
                    case "GetUserInfo":
                        User newUser = await GetUserInfoAsync(user.UserName);
                        if (newUser != null)
                        {
                            result.Success = true;
                            result.Object = newUser;
                        }
                        break;
                    case "GetFriends":
                        User[] newUsers = await GetUserFriendsAsync(user.UserName);
                        if (newUsers != null)
                        {
                            result.Success = true;
                            result.Object = newUsers;
                        }
                        break;
                    case "AddFriend":
                        result.Success = await UserAddFriendAsync(user.UserName, parameter);
                        break;
                    case "RemoveFriend":
                        result.Success = await UserRemoveFriendAsync(user.UserName, parameter);
                        break;
                }
                return result;
            }
            return null;
        }
        public static async Task<bool?> CheckUserExistsAsync(string userName)
        {
            string sqlCmd = "SELECT u_name FROM User_Info WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            object queryResult = await Sqlbin.getFirstDataAsync(sqlCmd, new SqlParameter[] { nameParameter });
            if (queryResult == null)
            {
                return false;
            }
            if (queryResult.ToString().Equals(userName))
            {
                return true;
            }
            return null;
        }
        /// <summary>
        /// 异步的校验用户是否存在。存在返回true，不存在返回false，执行失败返回null
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="userPassword">用户密码</param>
        public static async Task<bool?> CheckUserExistsAsync(string userName, string userPassword)
        {
            string sqlCmd = "SELECT u_name,u_pwd FROM User_Info WHERE u_name=@name AND u_pwd=@pwd";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            SqlParameter passwordParameter = new SqlParameter("@pwd", SqlDbType.NVarChar) { Value = userPassword };
            object queryResult = await Sqlbin.getFirstDataAsync(sqlCmd, new SqlParameter[] { nameParameter, passwordParameter });
            if (queryResult == null)
            {
                return false;
            }
            if (queryResult.ToString().Equals(userName))
            {
                return true;
            }
            return null;
        }

        /// <summary>
        /// 异步的用户注册。注册成功返回true，执行失败返回false
        /// </summary>
        /// <param name="user">用户对象</param>
        public static async Task<bool> UserSignupAsync(User user)
        {
            if (user == null) { return false; }
            StringBuilder sqlCmd = new StringBuilder("INSERT INTO User_Info(u_name,u_pwd,u_nickname,u_gender,u_age,u_head_icon) ");
            sqlCmd.Append("VALUES(@name,@pwd,@nickname,@gender,@age,@headicon)");
            SqlParameter[] sqlParameters =
            {
                new SqlParameter ("@name", SqlDbType.NVarChar) { Value = user.UserName },
                new SqlParameter ("@pwd", SqlDbType.NVarChar) { Value = user.Password },
                new SqlParameter ("@nickname", SqlDbType.NVarChar) { Value = user.NickName },
                new SqlParameter ("@gender", SqlDbType.NVarChar) { Value = user.Gender },
                new SqlParameter ("@age", SqlDbType.Int) { Value = user.Age },
                new SqlParameter ("@headicon", SqlDbType.NVarChar) { Value = Serverbin.SetUserHeadIcon(user.UserName, user.HeadIcon) }
            };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd.ToString(), sqlParameters) == -255)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步的修改用户信息。修改成功返回true，执行失败返回false
        /// </summary>
        /// <param name="user">用户对象</param>
        public static async Task<bool> AlterUserInfoAsync(User user)
        {
            if (user == null) { return false; }
            string sqlCmd = "UPDATE User_Info SET u_nickname=@nickname,u_gender=@gender,u_age=@age,u_head_icon=@headicon WHERE u_name=@name";
            int result = await Sqlbin.returnAffectedRowsAsync(sqlCmd, new SqlParameter[]
            {
                new SqlParameter ("@nickname", SqlDbType.NVarChar) { Value = user.NickName },
                new SqlParameter ("@gender", SqlDbType.NVarChar) { Value = user.Gender },
                new SqlParameter ("@age", SqlDbType.Int) { Value = user.Age },
                new SqlParameter ("@headicon", SqlDbType.NVarChar) { Value = Serverbin.SetUserHeadIcon(user.UserName, user.HeadIcon) },
                new SqlParameter ("@name", SqlDbType.NVarChar) { Value = user.UserName }
            });
            if (result == -255)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步的修改用户密码。修改成功返回true，修改失败返回false，执行失败返回null
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="oldPwd">原密码</param>
        /// <param name="newPwd">新密码</param>
        public static async Task<bool?> AlterUserPasswordAsync(string userName, string oldPwd, string newPwd)
        {
            string selectSqlCmd = "SELECT u_pwd FROM User_Info WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            string password = (await Sqlbin.getFirstDataAsync(selectSqlCmd, new SqlParameter[] { nameParameter }))?.ToString();
            if (password == null || (password?.Equals("-255") ?? true))
            {
                return null;
            }
            if (password.Equals(oldPwd))
            {
                string updateSqlCmd = "UPDATE User_Info SET u_pwd=@newPwd WHERE u_name=@name";
                SqlParameter[] sqlParameters = new SqlParameter[]
                {
                    new SqlParameter("@newPwd", SqlDbType.NVarChar) { Value = newPwd },
                    new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName }
                };
                if (await Sqlbin.returnAffectedRowsAsync(updateSqlCmd, sqlParameters) == -255)
                {
                    return null;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 异步的获取用户的状态。返回表示用户状态的数字，执行失败返回null
        /// </summary>
        /// <param name="userName">用户名</param>
        public static async Task<int?> GetUserStateAsync(string userName)
        {
            string sqlCmd = "SELECT u_state FROM User_Info WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            int? result = await Sqlbin.getFirstDataAsync(sqlCmd, new SqlParameter[] { nameParameter }) as int?;
            if (result == -255)
            {
                return null;
            }
            return result;
        }

        /// <summary>
        /// 异步的将用户状态更改为在线。成功返回true，执行失败返回false
        /// </summary>
        /// <param name="userName">用户名</param>
        public static async Task<bool> UserOnlineAsync(string userName)
        {
            string sqlCmd = "UPDATE User_Info SET u_state=1 WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, new SqlParameter[] { nameParameter }) == -255)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步的将用户状态更改为离线。成功返回true，执行失败返回false
        /// </summary>
        /// <param name="userName">用户名</param>
        public static async Task<bool> UserOfflineAsync(string userName)
        {
            string sqlCmd = "UPDATE User_Info SET u_state=0,offline_time=@time WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            SqlParameter timeParameter = new SqlParameter("@time", SqlDbType.DateTime) { Value = DateTime.Now.ToString() };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, new SqlParameter[] { nameParameter, timeParameter }) == -255)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步的获取用户的信息。获取成功返回用户对象，如果找不到用户或执行失败返回null
        /// </summary>
        /// <param name="userName">用户名</param>
        public static async Task<User> GetUserInfoAsync(string userName)
        {
            User user = null;
            string sqlCmd = "SELECT u_id,u_name,u_pwd,u_nickname,u_gender,u_age,u_head_icon,u_state FROM User_Info WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            DataTable userTable = await Sqlbin.getTableAsync(sqlCmd, "User", new SqlParameter[] { nameParameter });
            if (userTable != null)
            {
                if (userTable.Rows.Count > 0)
                {
                    user = new User()
                    {
                        UserID = Convert.ToInt32(userTable.Rows[0]["u_id"]),
                        UserName = userTable.Rows[0]["u_name"].ToString(),
                        Password = userTable.Rows[0]["u_pwd"].ToString(),
                        NickName = userTable.Rows[0]["u_nickname"].ToString(),
                        Gender = userTable.Rows[0]["u_gender"].ToString(),
                        Age = Convert.ToInt32(userTable.Rows[0]["u_age"]),
                        HeadIcon = Serverbin.GetUserHeadIcon(userTable.Rows[0]["u_head_icon"].ToString()),
                        State = Convert.ToInt32(userTable.Rows[0]["u_state"])
                    };
                }
            }
            return user;
        }

        /// <summary>
        /// 异步的获取用户的所有好友。返回好友对象的集合，如果执行失败返回null
        /// </summary>
        /// <param name="userName">用户名</param>
        public static async Task<User[]> GetUserFriendsAsync(string userName)
        {
            string sqlCmd = "SELECT f_name FROM User_Friend_Info WHERE u_name=@name";
            SqlParameter nameParameter = new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName };
            DataTable friendsTable = await Sqlbin.getTableAsync(sqlCmd, "Friends", new SqlParameter[] { nameParameter });
            if (friendsTable != null)
            {
                User[] users = new User[friendsTable.Rows.Count];
                for (int index = 0; index < users.Length; index++)
                {
                    users[index] = await GetUserInfoAsync(friendsTable.Rows[index]["f_name"].ToString());
                }
                return users;
            }
            return null;
        }

        /// <summary>
        /// 异步的添加好友。添加成功返回true，添加失败返回false
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="friendUserName">好友的用户名</param>
        public static async Task<bool> UserAddFriendAsync(string userName, string friendUserName)
        {
            string sqlCmd = "INSERT INTO User_Friend_Info VALUES(@name,@friendName)";
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName },
                new SqlParameter("@friendName", SqlDbType.NVarChar) { Value = friendUserName }
            };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, sqlParameters) == -255)
            {
                return false;
            }
            if (userName.Equals(friendUserName))
            {
                return true;
            }
            sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@name", SqlDbType.NVarChar) { Value = friendUserName },
                new SqlParameter("@friendName", SqlDbType.NVarChar) { Value = userName }
            };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, sqlParameters) == -255)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 异步的移除好友。移除成功返回true，移除失败返回false
        /// </summary>
        /// <param name="userName">用户名</param>
        /// <param name="friendUserName">好友的用户名</param>
        public static async Task<bool> UserRemoveFriendAsync(string userName, string friendUserName)
        {
            string sqlCmd = "DELETE User_Friend_Info WHERE u_name=@name AND f_name=@friendName";
            SqlParameter[] sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@name", SqlDbType.NVarChar) { Value = userName },
                new SqlParameter("@friendName", SqlDbType.NVarChar) { Value = friendUserName }
            };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, sqlParameters) == -255)
            {
                return false; ;
            }
            if (userName.Equals(friendUserName))
            {
                return true;
            }
            sqlParameters = new SqlParameter[]
            {
                new SqlParameter("@name", SqlDbType.NVarChar) { Value = friendUserName },
                new SqlParameter("@friendName", SqlDbType.NVarChar) { Value = userName }
            };
            if (await Sqlbin.returnAffectedRowsAsync(sqlCmd, sqlParameters) == -255)
            {
                return false;
            }
            return true;
        }
    }
}
