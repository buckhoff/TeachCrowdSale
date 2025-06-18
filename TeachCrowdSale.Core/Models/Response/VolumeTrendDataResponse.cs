using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response 
{ 
      /// <summary>
      /// Volume trend data point for API response
      /// </summary>
        public class VolumeTrendDataResponse
    {
        public DateTime Date { get; set; }
        public decimal Volume { get; set; }
        public decimal Change24h { get; set; }
        public decimal ChangePercentage { get; set; }
        public int TransactionCount { get; set; }
    }
}
