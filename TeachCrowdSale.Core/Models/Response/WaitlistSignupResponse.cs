using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Response
{
    /// <summary>
    /// Response model for waitlist signup
    /// </summary>
    public class WaitlistSignupResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid? WaitlistId { get; set; }
        public string? ErrorCode { get; set; }
        public DateTime? SignupDate { get; set; }
    }
}
