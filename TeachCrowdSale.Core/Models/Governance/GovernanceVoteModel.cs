using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeachCrowdSale.Core.Attributes;

namespace TeachCrowdSale.Core.Models.Governance
{
    /// <summary>
    /// Governance vote model for web consumption
    /// Maps from GovernanceVote entity
    /// </summary>
    public class GovernanceVoteModel
    {
        public int Id { get; set; }

        public int ProposalId { get; set; }

        [EthereumAddress]
        public string VoterAddress { get; set; } = string.Empty;

        public bool VoteFor { get; set; }

        [Range(0, double.MaxValue)]
        public long VotingPower { get; set; }

        public DateTime VoteDate { get; set; }

        [TransactionHash]
        public string TransactionHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
