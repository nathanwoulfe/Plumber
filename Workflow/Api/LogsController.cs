using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Web;
using System.Web.Http;
using log4net;
using log4net.Appender;
using Umbraco.Web.WebApi;
using Workflow.Helpers;
using Logger = log4net.Repository.Hierarchy.Logger;

namespace Workflow.Api
{
    [RoutePrefix("umbraco/backoffice/api/workflow/logs")]
    public class LogsController : UmbracoAuthorizedApiController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Reverses an array of log entry lines, preserving multiline entries
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        private static string[] ReverseLog(string[] log)
        {
            List<string> resp = new List<string>();

            while (log.Length != 0)
            {
                int index = Array.FindLastIndex(log, x => char.IsDigit(x[0]));
                for (int i = index; i < log.Length; i += 1)
                {
                    resp.Add(log[i]);
                }

                log = log.Take(index).ToArray();
            }

            return resp.ToArray();
        }

        /// <summary>
        /// Get a list of all dated log files for Plumber
        /// Fetches the current log file, and uses the name to get the rest
        /// </summary>
        /// <returns></returns>
        [Route("datelist")]
        public IHttpActionResult GetDateList()
        {
            try
            {
                List<string> dates = new List<string>();

                ILog log = LogManager.GetLogger("Workflow");
                var logger = (Logger)log.Logger;
                var appender = (FileAppender)logger.GetAppender("WorkflowLogAppender");
                string filePath = appender.File;

                string logDir = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileName(filePath);

                if (logDir == null) return Json(dates, ViewHelpers.CamelCase);
                string[] logFiles = Directory.GetFiles(logDir, $"{fileName}.*");

                if (logFiles.Any())
                {
                    dates = logFiles.Select(l => l.Substring(l.LastIndexOf('.') + 1)).ToList();
                }

                return Json(dates, ViewHelpers.CamelCase);
            }
            catch (Exception ex)
            {
                const string error = "Could not get log dates";
                Log.Error(error, ex);
                return Content(HttpStatusCode.InternalServerError, ViewHelpers.ApiException(ex, error));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [Route("get/{logdate?}")]
        public HttpResponseMessage GetLog(string logdate = "")
        {
            var response = new HttpResponseMessage();

            try
            {
                // read the contents of the log file
                ILog log = LogManager.GetLogger("Workflow");
                var logger = (Logger)log.Logger;
                var appender = (FileAppender)logger.GetAppender("WorkflowLogAppender");
                string filename = appender.File;

                if (!string.IsNullOrEmpty(logdate))
                {
                    filename += "." + logdate;
                }

                if (!File.Exists(filename))
                {
                    response.Content = new StringContent($"No log file found for {logdate}");
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");

                    return response;
                }

                string logText = HttpUtility.HtmlEncode(File.ReadAllText(filename));

                if (!string.IsNullOrEmpty(logText))
                {
                    string[] splitLog = logText.Split(new[] { Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);

                    splitLog = ReverseLog(splitLog);

                    var html = "";
                    var currentType = "";

                    foreach (string line in splitLog)
                    {
                        // really naive parsing of log string into basic html
                        // we know the structure based on the logger patten, so can split relatively confidently
                        string lineClass = char.IsDigit(line[0]) ? "row" : "detail";
                        string[] splitLine = line.Split(' ');

                        html += $"<span class=\"log-{lineClass} {(lineClass == "row" ? splitLine[3].ToLower() : currentType)}\">";

                        if (lineClass == "row")
                        {
                            currentType = splitLine[3].ToLower();

                            // date/time
                            DateTime date = DateTime.ParseExact($"{splitLine[0]} {splitLine[1]}", "yyyy-MM-dd HH:mm:ss,fff", CultureInfo.CurrentCulture);
                            html += $"<span class=\"log-date\">{date.ToString("MMM dd yyyy, h:mmtt: ", CultureInfo.CurrentCulture)}</span> ";

                            // thread
                            html += $"<span class=\"log-thread\">{splitLine[2]}</span> ";

                            // type
                            html += $"<span class=\"log-type {currentType}\">{splitLine[3]}</span> ";

                            // class
                            html += $"<span class=\"log-class\">{splitLine[4]}</span> {splitLine[5]} ";

                            // class
                            html += $"<span class=\"log-message\">{line.Substring(line.IndexOf("- ", StringComparison.Ordinal) + 2)}</span> ";
                        }
                        else
                        {
                            // class
                            html += $"<span class=\"log-message\">{line}</span> ";
                        }

                        // close it all
                        html += "</span>";

                    }

                    logText = html;
                }
                else
                {
                    logText = "*** Log file is currently empty ***";
                }

                response.Content = new StringContent(logText);

            }
            catch (Exception ex)
            {
                const string error = "Could not get workflow log";
                Log.Error(error, ex);
                response.Content = new StringContent(error);

            }

            response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
            return response;
        }
    }
}
