using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Governance
{
    // <summary>
    /// Governance proposal model for web consumption
    /// Replaces ProposalModel
    /// Maps from GovernanceProposal entity
    /// </summary>
    public class GovernanceProposalModel
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        [Range(0, double.MaxValue)]
        public long VotesFor { get; set; }

        [Range(0, double.MaxValue)]
        public long VotesAgainst { get; set; }

        [Range(0, 100)]
        public decimal ParticipationRate { get; set; }

        public string Category { get; set; } = string.Empty;

        [EthereumAddress]
        public string ProposerAddress { get; set; } = string.Empty;

        [TransactionHash]
        public string? TransactionHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ExecutedAt { get; set; }
    }
}
