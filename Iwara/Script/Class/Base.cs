using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iwara.Script.Class
{
    public class Base
    {
        public enum Sort
        {
            Date,
            Views,
            Likes
        }
        public enum Site
        {
            Iwara,
            Ecchi
        }
        public class CustomUrl
        {
            public string subDomain;
            public string cgi;
        }
        public class HostsInfo
        {
            public string ipLocation;
            public string ip;
            public string delay;
            public bool error;
        }
        public class Video
        {
            public UserShort author;
            public string title;
            public string date;
            public string description;
            public Dictionary<string, string> tags = new Dictionary<string, string>();
            public ArrayList moreFromUser;
            public ArrayList moreLikeThis;

            public bool isYouTube = false;
            public string YouTubeUrl;
            public Dictionary<string, string> videosUrlList = new Dictionary<string, string>();

            public string url;
            public string coverUrl;
            public string likes;
            public string views;
            public ArrayList comment;
        }
        public class VideoShort
        {
            public UserShortNA author;
            public string title;

            public string url;
            public string coverUrl;
            public bool noCover = false;
            public string likes;
            public string views;
        }
        public class Image
        {
            public UserShort author;
            public string title;
            public string date;
            public string description;
            public Dictionary<string, string> tags = new Dictionary<string, string>();
            public bool isGallery;

            public ArrayList imagesUrlList;

            public string url;
            public string coverUrl;
            public string likes;
            public string views;
            public ArrayList comment;
        }
        public class ImageShort
        {
            public UserShortNA author;
            public string title;
            public bool isGallery;

            public string url;
            public string coverUrl;
            public string likes;
            public string views;
        }
        public class Comment
        {
            public int id;
            public int parentId;
            public UserShort author;
            public string date;
            public string content;
        }
        public class Tag
        {
            public string name;
            public string url;
        }
        public class UserShort
        {
            public string userName;
            public string userId;
            public string avatarUrl;
        }
        public class UserShortNA
        {
            public string userName;
            public string userId;
        }
        public class User
        {
            public string name;
            public string userId;
            public string avatarUrl;
            public string joinDate;
            public string lastSeen;
            public string description;
            public UserShort following;
            public UserShort followers;

            public VideoShort videos;
            public ImageShort images;

        }
    }
}
