using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using HtmlAgilityPack;
using OKCupid.Web.Helpers;
using OKCupid.Web.Models.Context;
using OKCupid.Web.Models.Entitites;

namespace OKCupid.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly OKCupidContext _okCupidContext;
        private readonly ExportHelper _exportHelper;

        public HomeController(OKCupidContext okCupidContext, ExportHelper exportHelper)
        {
            _okCupidContext = okCupidContext;
            _exportHelper = exportHelper;
        }

        public ActionResult Index()
        {
            return View();
        }

        public JsonResult Import()
        {
            string messagesPage;

            try
            {
                messagesPage = _exportHelper.GetPage("http://www.okcupid.com/messages");
            }
            catch(Exception e)
            {
                throw e;
            }
            
            var doc = new HtmlDocument();
            doc.LoadHtml(messagesPage);

            var threadIds = _okCupidContext.MessageThreads.Select(t => t.OKCupidMessageThreadId).ToList();
            var messageIds = _okCupidContext.Messages.Select(m => m.OKCupidMessageId).ToList();

            foreach(var thread in doc.DocumentNode.SelectNodes("//ul[@id='messages']//li"))
            {
                var okCupidMessageThreadId = thread.GetAttributeValue("id", "").Split(new string[] {"_"}, StringSplitOptions.None)[1];
                var messageThread = threadIds.Any(t => t.Equals(okCupidMessageThreadId))
                                        ? _okCupidContext.MessageThreads.Single(t => t.OKCupidMessageThreadId == okCupidMessageThreadId)
                                        : new MessageThread
                                            {
                                                OKCupidMessageThreadId = okCupidMessageThreadId,
                                                Username = thread.SelectSingleNode(".//a[@class='subject']").InnerText
                                            };

                var threadUrl = thread.SelectSingleNode("p[@onclick]").GetAttributeValue("onclick", "");
                var threadPieces = threadUrl.Split(new[] {"window.location='"}, StringSplitOptions.None);
                threadUrl = threadPieces[1].Split(new[] { "'" }, StringSplitOptions.None)[0];
                var threadPage = _exportHelper.GetPage("http://www.okcupid.com" + Server.HtmlDecode(threadUrl));
                
                var threadDoc = new HtmlDocument();
                threadDoc.LoadHtml(threadPage);
                foreach (var msg in threadDoc.DocumentNode.SelectNodes("//ul[@id='thread']//li[@class]"))
                {
                    var id = msg.GetAttributeValue("id", "");
                    if (id.Contains("message"))
                    {
                        var okCupidMessageId = id.Split(new string[] {"_"}, StringSplitOptions.None)[1];
                        if (!messageIds.Any(m => m.Equals(okCupidMessageId)))
                        {
                            var message = new Message
                                {
                                    OKCupidMessageId = id.Split(new string[] {"_"}, StringSplitOptions.None)[1],
                                    Received = msg.GetAttributeValue("class", "").Contains("to_me"),
                                    DateSent = DateTime.Parse(msg.SelectSingleNode(".//span[@class='fancydate']").InnerText.Replace("&ndash; ", "")),
                                    Text = msg.SelectSingleNode(".//div[@class='message_body']").InnerText
                                };
                            messageThread.Messages.Add(message);
                        }
                    }
                }
                if (messageThread.MessageThreadId == 0)
                {
                    _okCupidContext.MessageThreads.Add(messageThread);
                }
            }
            
            _okCupidContext.SaveChanges();
            return Json(HttpStatusCode.OK, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImportSent()
        {
            string sentDocHtml;
            try
            {
                sentDocHtml = _exportHelper.GetPage("http://www.okcupid.com/messages?folder=2");
            }
            catch(Exception e)
            {
                throw e;
            }

            var sentDoc = new HtmlDocument();
            sentDoc.LoadHtml(sentDocHtml);

            // find the pager, determine if there are more than one page
            var pageLinks = sentDoc.DocumentNode.SelectNodes("//div[contains(@class,'pages')]/ul/li[not(@class)]/a").Select(n=>n.GetAttributeValue("href", ""));

            // get the first page of messages
            GetMessagesFromSentPage(sentDoc);

            // iterate through remaining pages, and get those messages too
            foreach(var link in pageLinks)
            {
                var threadPage = _exportHelper.GetPage("http://www.okcupid.com" + Server.HtmlDecode(link));
                sentDocHtml = _exportHelper.GetPage(threadPage);
                sentDoc.LoadHtml(sentDocHtml);
                GetMessagesFromSentPage(sentDoc);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        private void GetMessagesFromSentPage(HtmlDocument htmlDocument)
        {
            var threadIds = _okCupidContext.MessageThreads.Select(t => t.OKCupidMessageThreadId).ToList();
            var messageIds = _okCupidContext.Messages.Select(m => m.OKCupidMessageId).ToList();


            foreach (var thread in htmlDocument.DocumentNode.SelectNodes("//ul[@id='messages']//li"))
            {
                var username = thread.SelectSingleNode(".//a[@class='subject']").InnerText;
                var okCupidMessageId = thread.GetAttributeValue("id", "").Split(new string[] {"_"}, StringSplitOptions.None)[1];
                if (!messageIds.Contains(okCupidMessageId))
                {

                    var threadUrl = thread.SelectSingleNode("p[@onclick]").GetAttributeValue("onclick", "");
                    var threadPieces = threadUrl.Split(new[] { "window.location='" }, StringSplitOptions.None);
                    threadUrl = threadPieces[1].Split(new[] { "'" }, StringSplitOptions.None)[0];
                    var threadPage = _exportHelper.GetPage("http://www.okcupid.com" + Server.HtmlDecode(threadUrl));
                    var threadDoc = new HtmlDocument();
                    threadDoc.LoadHtml(threadPage);
                    var messageThread = new MessageThread
                        {
                            OKCupidMessageThreadId = threadDoc.DocumentNode.SelectSingleNode("//form[contains(@class,'flag_form']/input[@name='objectid']").GetAttributeValue("value", ""),
                            Username = username
                        };
                    foreach (var msg in threadDoc.DocumentNode.SelectNodes("//ul[@id='thread']//li[@class]"))
                    {
                        var id = msg.GetAttributeValue("id", "");
                        if (id.Contains("message"))
                        {
                            var message = new Message
                                {
                                    OKCupidMessageId = id.Split(new string[] {"_"}, StringSplitOptions.None)[1],
                                    Received = msg.GetAttributeValue("class", "").Contains("to_me"),
                                    DateSent = DateTime.Parse(msg.SelectSingleNode(".//span[@class='fancydate']").InnerText.Replace("&ndash; ", "")),
                                    Text = msg.SelectSingleNode(".//div[@class='message_body']").InnerText
                                };
                            messageThread.Messages.Add(message);
                        }
                    }
                    if (messageThread.MessageThreadId == 0)
                    {
                        _okCupidContext.MessageThreads.Add(messageThread);
                    }
                }
            }
        }

    }
}
