using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeachCrowdSale.Core.Data.Entities
{
    public class GovernanceVote
    {
        public int Id { get; set; }
        public int ProposalId { get; set; }
        public string VoterAddress { get; set; } = string.Empty;
        public bool VoteFor { get; set; }
        public long VotingPower { get; set; }
        public DateTime VoteDate { get; set; }
        public string TransactionHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
