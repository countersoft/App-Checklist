using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using Countersoft.Foundation.Commons.Extensions;
using Countersoft.Gemini.Commons.Dto;
using Countersoft.Gemini.Commons.Entity;
using Countersoft.Gemini.Extensibility.Apps;
using Countersoft.Gemini.Infrastructure;
using Countersoft.Gemini.Infrastructure.Apps;

namespace Checklist
{
    internal static class Constants
    {
        public static string AppId = "8FAC8849-04C0-418B-BC3B-E30A815A3E7A";
        public static string ControlId = "C8F0693D-129C-4D19-9865-9CED964F06AB";
    }

    public class ChecklistData
    {
        public int CheckId { get; set; }
        public string CheckName { get; set; }
        public bool Checked { get; set; }
        public string CheckedBy { get; set; }
        public DateTime CheckedDate { get; set; }
    }

    [AppType(AppTypeEnum.Widget),
    AppGuid("8FAC8849-04C0-418B-BC3B-E30A815A3E7A"),
    AppControlGuid("C8F0693D-129C-4D19-9865-9CED964F06AB"),
    AppAuthor("Countersoft"),
    AppKey("Checklist"), 
    AppName("Item Checklist"),
    AppDescription("Apply checklist to any item type")]
    public class Checklist : BaseAppController
    {        
        private IssueWidgetData<List<ChecklistData>> GetDefaultData()
        {
            IssueWidgetData<List<ChecklistData>> data = new IssueWidgetData<List<ChecklistData>>();

            data.Value = new List<ChecklistData>();

            data.Value.Add(new ChecklistData { CheckId = 1, CheckName = "Code Committed" });

            data.Value.Add(new ChecklistData { CheckId = 2, CheckName = "Documentation Updated" });

            data.Value.Add(new ChecklistData { CheckId = 3, CheckName = "Release Notes Compiled" });

            data.Value.Add(new ChecklistData { CheckId = 4, CheckName = "PDF Generated" });

            data.Value.Add(new ChecklistData { CheckId = 5, CheckName = "Paperwork Submitted" });

            return data;
        }

        public override WidgetResult Show(IssueDto issue = null)
        {
            WidgetResult result = new WidgetResult();

            IssueWidgetData<List<ChecklistData>> data = GeminiContext.IssueWidgetStore.Get<List<ChecklistData>>(issue.Id, AppGuid, AppControlGuid);

            if (data == null || data.Value == null || data.Value.Count == 0)
            {
                data = GetDefaultData();

                data.IssueId = issue.Id;

                GeminiContext.IssueWidgetStore.Save(issue.Id, AppGuid, AppControlGuid, data.Value);
            }

            result.Markup = new WidgetMarkup("views\\checklist.cshtml", data);

            result.Success = true;

            return result;
        }

        public override WidgetResult Caption(IssueDto issue = null)
        {
            WidgetResult result = new WidgetResult();

            result.Success = true;

            result.Markup.Html = "Checklist";

            return result;
        }

        [AppUrl("action/{issueid}")]
        public ContentResult Save(int issueId)
        {
            IssueWidgetData<List<ChecklistData>> data = GeminiContext.IssueWidgetStore.Get<List<ChecklistData>>(issueId, Constants.AppId, Constants.ControlId);

            int checkId = Request["checkId"].ToInt(0);

            bool checkedStatus = Request["checkedState"].ToBool();

            ChecklistData item = data.Value.Find(i => i.CheckId == checkId);

            if (item != null && item.CheckId == checkId)
            {
                item.Checked = checkedStatus;

                item.CheckedBy = CurrentUser.Fullname;

                item.CheckedDate = DateTime.UtcNow;

                if (!item.Checked)
                {
                    item.CheckedBy = string.Empty;
                }
            }

            GeminiContext.IssueWidgetStore.Save(issueId, Constants.AppId, Constants.ControlId, data.Value);

            return JsonSuccess(AppManager.Instance.ItemContentWidgetsOnShow(this, UserContext, GeminiContext, Cache, UserContext.Issue, Constants.AppId, Constants.ControlId), "");
        }
    }
}
