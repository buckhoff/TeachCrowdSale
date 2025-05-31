using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Models.Governance
{
    public class ProposalModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public long VotesFor { get; set; }
        public long VotesAgainst { get; set; }
        public decimal ParticipationRate { get; set; }
        public string Category { get; set; } = string.Empty;
        public string ProposerAddress { get; set; } = string.Empty;
    }
}
