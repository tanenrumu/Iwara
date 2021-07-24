using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NSoup;
using NSoup.Nodes;
using NSoup.Select;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Iwara.Script.Class.Base;
using static Iwara.Script.Network.Base;
using Comment = Iwara.Script.Class.Base.Comment;
using static Iwara.Script.UIManager;

namespace Iwara.Script.Class
{
    class Analyser
    {
        public static CustomUrl AnalyesUrl(string url)
        {
            CustomUrl customUrl = new CustomUrl();
            string subDomainReg = "(?<=//).*(?=.iwara.tv)";
            string cgiReg = "(?<=iwara.tv).*";
            customUrl.subDomain = Convert.ToString(Regex.Matches(url, subDomainReg)[0]);
            customUrl.cgi = Convert.ToString(Regex.Matches(url, cgiReg)[0]);
            return customUrl;
        }
        public static ArrayList AnalyseHosts(string hostsJson)
        {
            ArrayList hosts = new ArrayList();
            HostsInfo tempHosts = new HostsInfo();
            JObject keys = (JObject)JsonConvert.DeserializeObject(hostsJson);

            foreach (JProperty property in keys["data"].ToObject<JObject>().Properties())
            {
                tempHosts.delay = Convert.ToString(Convert.ToInt16(Convert.ToSingle(property.Value[0]["answer"]["time_consume"]) * 1000));
                if (property.Value[0]["answer"]["error"].ToString() == "")
                {
                    tempHosts.error = false;
                }
                else
                {
                    tempHosts.error = true;
                    continue;
                }
                foreach (JToken answer in property)
                {
                    foreach (var records in answer[0]["answer"]["records"])
                    {
                        tempHosts.ip = records["value"].ToString();
                        tempHosts.ipLocation = records["ip_location"].ToString();
                    }
                    hosts.Add(tempHosts);
                }
            }
            return hosts;
        }
        public static Video AnalyseVideoHtml(string htmlCode)
        {
            Video video = new Video();
            string avatarUrlReg = "(?<=src=\").*?(?=\")";
            string youtubeUrlReg = "(?<=src=\").*?(?=\")";
            string dateReg = "[1-9]\\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1])\\s+(20|21|22|23|[0-1]\\d):[0-5]\\d";
            string tagReg = "(?<=href=\").*?(?=\")";
            string videoIdReg = "(?<=<link rel=\"canonical\" href=\"/videos/).*?(?=\")";
            string userIdReg = "(?<=href=\"/users/).*?(?=\")";
            string likesOrViewsReg = "(\\d+(\\.+\\d+|\\.{0})\\,{0,1})+";
            Document mainDocument = NSoupClient.Parse(htmlCode);
            Elements infoElement = mainDocument.GetElementsByClass("node-info");

            Document infoDocument = NSoupClient.Parse(infoElement[0].Html());
            video.title = infoDocument.GetElementsByClass("title")[0].Text();

            UserShort userShort = new UserShort
            {
                userName = infoDocument.GetElementsByClass("username")[0].Text(),
                avatarUrl = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("user-picture")[0].Html(), avatarUrlReg)[0]),
                userId = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("user-picture")[0].Html(), userIdReg)[0])
            };

            video.author = userShort;

            video.date = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("submitted")[0].Html(), dateReg)[0]);
            if (infoDocument.GetElementsByClass("field-name-body").Count != 0)
            {
                video.description = infoDocument.GetElementsByClass("field-name-body")[0].Child(0).Child(0).Html();
            }
            else
            {
                video.description = "";
            }

            Elements tempViewsElement = infoDocument.GetElementsByClass("node-views");
            if (tempViewsElement.Count != 0)
            {
                MatchCollection tempLikesOrViews = Regex.Matches(tempViewsElement.Html(), likesOrViewsReg);
                if (tempLikesOrViews.Count == 1)
                {
                    video.likes = "0";
                    video.views = tempLikesOrViews[0].ToString();
                }
                else if (tempLikesOrViews.Count == 2)
                {
                    video.likes = tempLikesOrViews[0].ToString();
                    video.views = tempLikesOrViews[1].ToString();
                }
            }
            else
            {
                video.likes = "0";
                video.views = "0";
            }

            Elements tags = infoDocument.GetElementsByClass("field-name-field-categories")[0].GetAllElements();

            foreach (Element tag in tags)
            {
                MatchCollection match = Regex.Matches(tag.Html(), tagReg);
                if (match.Count != 0 && !video.tags.ContainsKey(tag.Text()))
                {
                    video.tags.Add(tag.Text(), Convert.ToString(match[0]));
                }
            }

            string httpResult = GetTextFromIwara(AnalyesUrl("https://www.iwara.tv/api/video/" + Convert.ToString(Regex.Matches(mainDocument.Html(), videoIdReg)[0])));
            if (httpResult == "[]")
            {
                video.isYouTube = true;
                Elements playerElement = mainDocument.GetElementsByClass("player");
                if (playerElement.Count != 0)
                {
                    video.YouTubeUrl = Convert.ToString(Regex.Matches(mainDocument.GetElementsByClass("player")[0].Html(), youtubeUrlReg)[0]);
                }
                else
                {
                    LogErrorNoRetry("This video had been deleted.");
                }
            }
            else
            {
                if (httpResult.Substring(0, 4) != "error")
                {
                    dynamic items = JsonConvert.DeserializeObject(httpResult);
                    foreach (dynamic item in items)
                    {
                        video.videosUrlList.Add(item.resolution.Value, "https:" + item.uri.Value);
                    }
                }
            }
            return video;
        }
        public static ArrayList AnalyseVideoPreviewHtml(string htmlCode)
        {
            ArrayList videos = new ArrayList();
            Document mainDocument = NSoupClient.Parse(htmlCode);
            Elements videosElement = mainDocument.GetElementsByClass("node-video");
            foreach (Element element in videosElement)
            {
                if (element.GetElementsByClass("even").Count == 0) { continue; }
                VideoShort videoShort = new VideoShort();
                string urlReg = "(?<=href=\").*?(?=\")";
                string coverUrlReg = "(?<=src=\").*?(?=\")";
                string userIdReg = "(?<=href=\"/users/).*?(?=\")";

                videoShort.title = element.GetElementsByClass("title")[0].Text();
                Elements tempLikes = element.GetElementsByClass("right-icon");
                videoShort.likes = tempLikes.Count == 0 ? "0" : tempLikes[0].Text();
                Elements tempViews = element.GetElementsByClass("left-icon");
                videoShort.views = tempViews.Count == 0 ? "0" : tempViews[0].Text();

                videoShort.url = Convert.ToString(Regex.Matches(element.GetElementsByClass("title")[0].Html(), urlReg)[0]);
                Element evenElement = element.GetElementsByClass("even")[0];
                MatchCollection tempCoverUrlMatch = Regex.Matches(evenElement.Html(), coverUrlReg);
                if (tempCoverUrlMatch.Count == 0)
                {
                    videoShort.noCover = true;
                }
                else
                {
                    videoShort.coverUrl = Convert.ToString(Regex.Matches(evenElement.Html(), coverUrlReg)[0]);
                }

                UserShortNA userShortNA = new UserShortNA
                {
                    userName = element.GetElementsByClass("username")[0].Text(),
                    userId = Convert.ToString(Regex.Matches(element.Html(), userIdReg)[0])
                };
                videoShort.author = userShortNA;
                videos.Add(videoShort);
            }
            return videos;
        }
        public static Image AnalyseImageHtml(string htmlCode)
        {
            Image image = new Image();
            string avatarUrlReg = "(?<=src=\").*?(?=\")";
            string dateReg = "[1-9]\\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1])\\s+(20|21|22|23|[0-1]\\d):[0-5]\\d";
            string tagReg = "(?<=href=\").*?(?=\")";
            string imagesUrlReg = "(?<=href=\").*?(?=\")";
            string userIdReg = "(?<=href=\"/users/).*?(?=\")";
            string likesOrViewsReg = "(\\d+(\\.+\\d+|\\.{0})\\,{0,1})+";
            Document mainDocument = NSoupClient.Parse(htmlCode);
            Elements infoElement = mainDocument.GetElementsByClass("node-info");

            Document infoDocument = NSoupClient.Parse(infoElement[0].Html());
            image.title = infoDocument.GetElementsByClass("title")[0].Text();

            UserShort userShort = new UserShort
            {
                userName = infoDocument.GetElementsByClass("username")[0].Text(),
                avatarUrl = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("user-picture")[0].Html(), avatarUrlReg)[0]),
                userId = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("user-picture")[0].Html(), userIdReg)[0])
            };

            image.author = userShort;

            image.date = Convert.ToString(Regex.Matches(infoDocument.GetElementsByClass("submitted")[0].Html(), dateReg)[0]);
            if (infoDocument.GetElementsByClass("field-name-body").Count != 0)
            {
                image.description = infoDocument.GetElementsByClass("field-name-body")[0].Child(0).Child(0).Html();
            }
            else
            {
                image.description = "";
            }

            Elements tempViewsElement = infoDocument.GetElementsByClass("node-views");
            if (tempViewsElement.Count != 0)
            {
                MatchCollection tempLikesOrViews = Regex.Matches(tempViewsElement.Html(), likesOrViewsReg);
                if (tempLikesOrViews.Count == 1)
                {
                    image.likes = "0";
                    image.views = tempLikesOrViews[0].ToString();
                }
                else if (tempLikesOrViews.Count == 2)
                {
                    image.likes = tempLikesOrViews[0].ToString();
                    image.views = tempLikesOrViews[1].ToString();
                }
            }
            else
            {
                image.likes = "0";
                image.views = "0";
            }

            Elements tags = infoDocument.GetElementsByClass("field-name-field-image-categories")[0].GetAllElements();

            foreach (Element tag in tags)
            {
                MatchCollection match = Regex.Matches(tag.Html(), tagReg);
                if (match.Count != 0 && !image.tags.ContainsKey(tag.Text()))
                {
                    image.tags.Add(tag.Text(), Convert.ToString(match[0]));
                }
            }
            Elements imagesUrlElements = mainDocument.GetElementsByClass("field-name-field-images")[0].GetElementsByClass("field-item");
            ArrayList imagesUrlList = new ArrayList();
            foreach (Element imageUrl in imagesUrlElements)
            {
                if(Regex.Matches(imageUrl.OuterHtml(), imagesUrlReg).Count != 0)
                {
                    imagesUrlList.Add(Convert.ToString(Regex.Matches(imageUrl.OuterHtml(), imagesUrlReg)[0]));
                }
            }
            image.imagesUrlList = imagesUrlList;
            return image;
        }
        public static ArrayList AnalyseImagePreviewHtml(string htmlCode)
        {
            ArrayList images = new ArrayList();
            Document mainDocument = NSoupClient.Parse(htmlCode);
            Elements videosElement = mainDocument.GetElementsByClass("node-image");
            foreach (Element element in videosElement)
            {
                if (element.GetElementsByClass("even").Count == 0) { continue; }
                ImageShort imageShort = new ImageShort();
                string urlReg = "(?<=href=\").*?(?=\")";
                string coverUrlReg = "(?<=src=\").*?(?=\")";
                string userIdReg = "(?<=href=\"/users/).*?(?=\")";

                imageShort.title = element.GetElementsByClass("title")[0].Text();
                Elements tempLikes = element.GetElementsByClass("right-icon");
                imageShort.likes = tempLikes.Count == 0 ? "0" : tempLikes[0].Text();
                Elements tempViews = element.GetElementsByClass("left-icon");
                imageShort.views = "0";
                foreach (Element leftElement in tempViews)
                {
                    if (leftElement.Parent.Html().Contains("likes"))
                    {
                        imageShort.views = leftElement.Text();
                    }
                    if (leftElement.Html().Contains("Gallery"))
                    {
                        imageShort.isGallery = true;
                    }
                }

                imageShort.url = Convert.ToString(Regex.Matches(element.GetElementsByClass("title")[0].Html(), urlReg)[0]);
                Element evenElement = element.GetElementsByClass("even")[0];
                imageShort.coverUrl = Convert.ToString(Regex.Matches(evenElement.Html(), coverUrlReg)[0]);

                UserShortNA userShortNA = new UserShortNA
                {
                    userName = element.GetElementsByClass("username")[0].Text(),
                    userId = Convert.ToString(Regex.Matches(element.Html(), userIdReg)[0])
                };
                imageShort.author = userShortNA;
                images.Add(imageShort);
            }
            return images;
        }
        // AnalyseCommentsHtml(new ArrayList(), new ArrayList(), htmlCode);
        public static ArrayList AnalyseCommentsHtml(ArrayList comments, ArrayList commentsId, string htmlCode, int parentId = -1)
        {
            Document mainDocument = NSoupClient.Parse(htmlCode);
            Element commentsElement = mainDocument.GetElementById("comments");
            if (!(commentsElement is null))
            {
                htmlCode = commentsElement.Html();
            }
            if (!(commentsElement is null) || (commentsElement is null && parentId != -1))
            {
                Document commentsDocument = NSoupClient.Parse(htmlCode);
                foreach (Element commentElement in commentsDocument.GetAllElements())
                {
                    if (commentElement.Id.Contains("comment-"))
                    {
                        if (commentElement.Id.Contains("comment-form") ||
                            commentElement.Id.Contains("comment-body") ||
                            commentElement.Id.Contains("-comment")) { continue; }
                        int id = Convert.ToInt32(commentElement.Id.Remove(0, 8));

                        Comment comment = AnalyseCommentHtml(commentElement.NextElementSibling.OuterHtml());

                        // 粗暴（
                        if (!commentsId.Contains(id))
                        {
                            comment.id = id;
                            comment.parentId = parentId;
                            comments.Add(comment);
                        }

                        commentsId.Add(id);
                        Element indentedElement = commentElement.NextElementSibling.NextElementSibling;
                        if (!(indentedElement is null) && indentedElement.ClassName().Contains("indented"))
                        {
                            AnalyseCommentsHtml(comments, commentsId, indentedElement.Html(), id);
                        }
                    }
                }
            }
            return comments;
        }
        public static Comment AnalyseCommentHtml(string htmlCode)
        {
            Comment comment = new Comment();
            string avatarUrlReg = "(?<=src=\").*?(?=\")";
            string dateReg = "[1-9]\\d{3}-(0[1-9]|1[0-2])-(0[1-9]|[1-2][0-9]|3[0-1])\\s+(20|21|22|23|[0-1]\\d):[0-5]\\d";
            string userIdReg = "(?<=href=\"/users/).*?(?=\")";

            Document mainDocument = NSoupClient.Parse(htmlCode);
            Elements commentElement = mainDocument.GetElementsByClass("comment");

            Document commentDocument = NSoupClient.Parse(commentElement[0].Html());

            UserShort userShort = new UserShort
            {
                userName = commentDocument.GetElementsByClass("username")[0].Text(),
                avatarUrl = Convert.ToString(Regex.Matches(commentDocument.GetElementsByClass("user-picture")[0].Html(), avatarUrlReg)[0]),
                userId = Convert.ToString(Regex.Matches(commentDocument.GetElementsByClass("user-picture")[0].Html(), userIdReg)[0])
            };
            comment.author = userShort;
            comment.date = Convert.ToString(Regex.Matches(commentDocument.GetElementsByClass("submitted")[0].Html(), dateReg)[0]);
            comment.content = commentDocument.GetElementsByClass("even")[0].Html();
            comment.parentId = -1;
            return comment;
        }
    }
}
