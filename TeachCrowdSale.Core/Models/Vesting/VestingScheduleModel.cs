using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Vesting
{
    /// <summary>
    /// Vesting schedule model
    /// </summary>
    public class VestingScheduleModel
    {
        public List<VestingCategoryModel> Categories { get; set; } = new();
        public List<VestingTimelineModel> Timeline { get; set; } = new();
        public VestingSummaryModel Summary { get; set; } = new();
    }
}
