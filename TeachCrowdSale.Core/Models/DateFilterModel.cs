using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models
{
    /// <summary>
    /// Date filter model for timeline filtering
    /// </summary>
    public class DateFilterModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Preset { get; set; } = "all"; // all, current, upcoming, completed
        public List<DatePresetModel> Presets { get; set; } = new();
    }
}
