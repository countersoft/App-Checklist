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
    [AppType(AppTypeEnum.Widget),
    AppGuid("8FAC8849-04C0-418B-BC3B-E30A815A3E7A"),
    AppControlGuid("C8F0693D-129C-4D19-9865-9CED964F06AB"),
    AppAuthor("Countersoft"),
    AppKey("Checklist"), 
    AppName("Checklist"),
    AppDescription("Apply checklist to any item type"),
    AppRequiresConfigScreen(true)]
    [OutputCache(Duration = 0, NoStore = true, Location = System.Web.UI.OutputCacheLocation.None)]
    public class Checklist : BaseAppController
    {        
        private IssueWidgetData<List<ChecklistData>> GetData(IssueDto issue)
        {
            IssueWidgetData<List<ChecklistData>> result = new IssueWidgetData<List<ChecklistData>>() { Value = new List<ChecklistData>(), AppId = AppGuid, IssueId = issue.Id, ControlId = AppControlGuid };

            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);

            if (data == null || data.Value == null || data.Value.Data == null) return result;

            var templateData = data.Value.Data.Find(t => t.TemplateId == issue.Project.TemplateId);

            if (templateData == null) return result;

            IssueWidgetData<List<ChecklistData>> IssueData = GeminiContext.IssueWidgetStore.Get<List<ChecklistData>>(issue.Id, AppGuid, AppControlGuid);

            foreach(var item in templateData.Items)
            {
                ChecklistData existingItem = IssueData != null && IssueData.Value != null && IssueData.Value.Count > 0 ? IssueData.Value.Find(s => s.CheckId == item.Id) : null;

                if (existingItem == null)
                {
                    result.Value.Add(new ChecklistData { CheckId = item.Id, CheckTitle = item.Title });
                }
                else
                {
                    existingItem.CheckedDate = existingItem.CheckedDate.ToLocal(UserContext.User.TimeZone);

                    existingItem.CheckTitle = item.Title;
                    result.Value.Add(existingItem);                    
                }
            }

            return result;
        }

        public override WidgetResult Show(IssueDto issue = null)
        {
            var data = GetData(issue);

            WidgetResult result = new WidgetResult();

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
            IssueWidgetData<List<ChecklistData>> data = GeminiContext.IssueWidgetStore.Get<List<ChecklistData>>(issueId, AppGuid, AppControlGuid);

            int checkId = Request["checkId"].ToInt(0);

            bool checkedStatus = Request["checkedState"].ToBool();
            bool isNewItem = false;

            if (data == null)
            {
                data = new IssueWidgetData<List<ChecklistData>>();                
            }

            if (data.Value == null)
            {
                data.Value = new List<ChecklistData>();
            }

            ChecklistData item = data.Value.Find(i => i.CheckId == checkId);

            if (item == null)
            {
                item = new ChecklistData() { CheckId = checkId };
                isNewItem = true;
            }

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

            if (isNewItem)
            {
                data.Value.Add(item);
            }

            GeminiContext.IssueWidgetStore.Save(issueId, AppGuid, AppControlGuid, data.Value);

            return JsonSuccess(AppManager.Instance.ItemContentWidgetsOnShow(this, UserContext, GeminiContext, Cache, UserContext.Issue, AppGuid, AppControlGuid), "");
        }

        public override WidgetResult Configuration()
        {
            var result = new WidgetResult() { Success = true };
            
            ChecklistConfigModel model = GetConfigModel();

            result.Markup = new WidgetMarkup("views/settings.cshtml", model);

            return result;
        }

        [AppUrl("getpage")]
        public ActionResult GetPage(int templateId)
        {
            return JsonSuccess(RenderPartialViewToString(this, AppManager.Instance.GetAppUrl(AppGuid, "views/_settings.cshtml"), GetConfigModel(templateId)));
        }

        public ChecklistConfigModel GetConfigModel(int templateId = 0)
        {
            var templates = ProjectTemplateManager.GetAll();
            ChecklistConfigModel model = new ChecklistConfigModel();

            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);

            if (data != null && data.Value != null && data.Value.Data != null && templates.Count > 0)
            {
                templateId = templateId > 0 ? templateId : templates[0].Id;

                var item = data.Value.Data.Find(s => s.TemplateId == templateId);

                if (item != null)
                {
                    model.Items = item.Items;
                }
            }

            model.Templates = new SelectList(ProjectTemplateManager.GetAll(), "Id", "Name", templateId);

            return model;
        }

        [AppUrl("add")]
        public ActionResult Add(string title, int templateId)
        {
            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);
            ChecklistConfigTemplateData templateData = null;
            
            if (data == null)
            {
                data = new GlobalConfigurationWidgetData<ChecklistConfigDataModel>();
                data.AppId = AppGuid;
                data.Value = new ChecklistConfigDataModel();
                data.Value.Data = new List<ChecklistConfigTemplateData>();
            }
            else
            {
                templateData = data.Value.Data.Find(t => t.TemplateId == templateId); 
            }

            if (templateData == null)
            {
                templateData = new ChecklistConfigTemplateData();
                templateData.TemplateId = templateId;
                data.Value.Data.Add(templateData);
                data.Value.CurrentSequence = 1;
            }
            else
            {
                data.Value.CurrentSequence += 1;
            }

            templateData.Items.Add(new ChecklistConfigData() { Id = data.Value.CurrentSequence, Title = title });

            GeminiContext.GlobalConfigurationWidgetStore.Save(AppGuid, data.Value);

            return JsonSuccess();
        }

        [AppUrl("delete")]
        public ActionResult Delete(int templateId, int itemId)
        {
            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);

            if (data == null || data.Value == null || data.Value.Data == null) return JsonError();

            var templateData = data.Value.Data.Find(t => t.TemplateId == templateId);

            if (templateData == null) return JsonError();

            var itemIndex = templateData.Items.FindIndex(s => s.Id == itemId);

            if (itemIndex == -1) return JsonError();

            templateData.Items.RemoveAt(itemIndex);

            GeminiContext.GlobalConfigurationWidgetStore.Save(AppGuid, data.Value);

            return JsonSuccess();
        }

        [AppUrl("getproperty")]
        public ActionResult GetProperty(int templateId, int id, string property)
        {
            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);

            if (data == null || data.Value == null || data.Value.Data == null) return JsonError();

            var templateData = data.Value.Data.Find(t => t.TemplateId == templateId);

            if (templateData == null) return JsonError();

            var item = templateData.Items.Find(s => s.Id == id);

            if (item == null) return JsonError();

            return Content(item.Title);
        }

        [AppUrl("saveproperty")]
        public ActionResult SaveProperty(int templateId, int id, string property, string value)
        {
            GlobalConfigurationWidgetData<ChecklistConfigDataModel> data = GeminiContext.GlobalConfigurationWidgetStore.Get<ChecklistConfigDataModel>(AppGuid);

            if (data == null || data.Value == null || data.Value.Data == null) return Content(value);

            var templateData = data.Value.Data.Find(t => t.TemplateId == templateId);

            if (templateData == null) return Content(value);

            var itemIndex = templateData.Items.FindIndex(s => s.Id == id);

            if (itemIndex == -1) return Content(value);

            templateData.Items[itemIndex].Title = value;

            GeminiContext.GlobalConfigurationWidgetStore.Save(AppGuid, data.Value);

            return Content(value);
        }
    }
}
