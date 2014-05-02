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
    public class ChecklistConfigModel
    {
        public IEnumerable<SelectListItem> Templates { get; set; }
        public List<ChecklistConfigData> Items { get; set; }

        public ChecklistConfigModel()
        {
            Items = new List<ChecklistConfigData>();
        }
    }

    public class ChecklistConfigDataModel
    {        
        public int CurrentSequence { get; set; }
        public List<ChecklistConfigTemplateData> Data { get; set; }    
    }

    public class ChecklistConfigTemplateData
    {
        public int TemplateId { get; set; }
        public List<ChecklistConfigData> Items { get; set; }

        public ChecklistConfigTemplateData()
        {
            Items = new List<ChecklistConfigData>();
        }
    }

    public class ChecklistConfigData
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class ChecklistData
    {
        public int CheckId { get; set; }
        public string CheckTitle { get; set; }
        public bool Checked { get; set; }
        public string CheckedBy { get; set; }
        public DateTime CheckedDate { get; set; }
    }
}
