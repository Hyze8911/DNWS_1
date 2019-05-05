using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

//600611030 guide me to this solution

namespace DNWS
{
    class TwitterApiPlugin : TwitterPlugin
    {
        private List<User> GetUser()
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> users = context.Users.Where(b => true).Include(b => b.Following).ToList();
                    return users;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public List<Following> FollowingList(string name)
        {
            using (var context = new TweetContext())
            {
                try
                {
                    List<User> followings = context.Users.Where(b => b.Name.Equals(name)).Include(b => b.Following).ToList();
                    return followings[0].Following;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public override HTTPResponse GetResponse(HTTPRequest request)
        {
            HTTPResponse response = new HTTPResponse(200);
            StringBuilder sb = new StringBuilder();
            string user = request.getRequestByKey("user");
            string password = request.getRequestByKey("password");
            string following = request.getRequestByKey("following");
            string message = request.getRequestByKey("message");
            string[] link = request.Filename.Split("?");

            if (link[0] == "users")
            {
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(GetUser());
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        Twitter.AddUser(user, password);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 403;
                        response.body = Encoding.UTF8.GetBytes("403 User already exists");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    Twitter twitter = new Twitter(user);
                    try
                    {
                        twitter.DelUser(user);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            else if (link[0] == "following")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    string json = JsonConvert.SerializeObject(FollowingList(user));
                    response.body = Encoding.UTF8.GetBytes(json);
                }
                else if (request.Method == "POST")
                {
                    if (Twitter.IsUserExist(following))
                    {
                        twitter.AddFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    else
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                else if (request.Method == "DELETE")
                {
                    try
                    {
                        twitter.RemoveFollowing(following);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            else if (link[0] == "tweet")
            {
                Twitter twitter = new Twitter(user);
                if (request.Method == "GET")
                {
                    try
                    {
                        string timeline = request.getRequestByKey("timeline");
                        if (timeline == "following")
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetFollowingTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                        else
                        {
                            string json = JsonConvert.SerializeObject(twitter.GetUserTimeline());
                            response.body = Encoding.UTF8.GetBytes(json);
                        }
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
                else if (request.Method == "POST")
                {
                    try
                    {
                        twitter.PostTweet(message);
                        response.body = Encoding.UTF8.GetBytes("200 OK");
                    }
                    catch (Exception)
                    {
                        response.status = 404;
                        response.body = Encoding.UTF8.GetBytes("404 User not found");
                    }
                }
            }
            response.type = "application/json";
            return response;
        }
    }

}