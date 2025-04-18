using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using TeachCrowdSale.Web.Models;

namespace TeachTokenCrowdsale.Web.Services
{
    public class FaqService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<FaqService> _logger;
        private readonly IConfiguration _configuration;

        public FaqService(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<FaqService> logger,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<List<FaqCategory>> GetCategoriesAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("faq_categories", out List<FaqCategory> categories))
            {
                return categories;
            }

            try
            {
                // For now, we'll return mock data
                // In a real application, this would come from an API
                var mockCategories = new List<FaqCategory>
                {
                    new FaqCategory { Id = "general", Name = "General", Icon = "fas fa-info-circle" },
                    new FaqCategory { Id = "token", Name = "Token", Icon = "fas fa-coins" },
                    new FaqCategory { Id = "presale", Name = "Presale", Icon = "fas fa-tags" },
                    new FaqCategory { Id = "platform", Name = "Platform", Icon = "fas fa-building" },
                    new FaqCategory { Id = "technical", Name = "Technical", Icon = "fas fa-cogs" }
                };

                // Cache for 1 hour
                _cache.Set("faq_categories", mockCategories, TimeSpan.FromHours(1));

                return mockCategories;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FAQ categories");
                return new List<FaqCategory>();
            }
        }

        public async Task<List<FaqItemModel>> GetFaqItemsAsync()
        {
            // Try to get from cache first
            if (_cache.TryGetValue("faq_items", out List<FaqItemModel> items))
            {
                return items;
            }

            try
            {
                // For now, we'll return mock data
                // In a real application, this would come from an API
                var mockItems = new List<FaqItemModel>
                {
                    // General FAQs
                    new FaqItemModel
                    {
                        Id = "faq-1",
                        Category = "general",
                        Question = "What is TeachToken?",
                        Answer =
                            "<p>TeachToken is a revolutionary blockchain platform designed to empower educators worldwide by providing financial support, resource sharing, and community governance. Our platform connects teachers directly with supporters, eliminating intermediaries and maximizing the impact of every contribution.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-2",
                        Category = "general",
                        Question = "How does TeachToken benefit educators?",
                        Answer =
                            "<p>TeachToken benefits educators in multiple ways:</p><ul><li>Direct financial support through staking and rewards</li><li>Platform for sharing and monetizing educational resources</li><li>Governance participation in platform decisions</li><li>Community recognition and reputation building</li><li>Access to educational partnerships and opportunities</li></ul>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-3",
                        Category = "general",
                        Question = "When will the TeachToken platform launch?",
                        Answer =
                            "<p>The TeachToken platform development follows a phased approach:</p><ul><li>Phase 1 (Q3 2025): Token launch and initial staking</li><li>Phase 2 (Q4 2025): Marketplace and reward system</li><li>Phase 3 (Q1 2026): Governance and stability mechanisms</li><li>Phase 4 (Q2 2026): Full ecosystem integration</li></ul><p>Follow our <a href='#roadmap'>roadmap</a> for detailed timelines.</p>"
                    },

                    // Token FAQs
                    new FaqItemModel
                    {
                        Id = "faq-4",
                        Category = "token",
                        Question = "What is the total supply of TEACH tokens?",
                        Answer =
                            "<p>The total supply of TEACH tokens is capped at 5 billion (5,000,000,000) tokens. This fixed supply ensures the token's scarcity and potential for value appreciation over time.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-5",
                        Category = "token",
                        Question = "How is the token distributed?",
                        Answer =
                            "<p>TEACH tokens are distributed as follows:</p><ul><li>Platform Ecosystem: 32% (1,600,000,000 tokens)</li><li>Community Incentives: 22% (1,100,000,000 tokens)</li><li>Initial Liquidity: 14% (700,000,000 tokens)</li><li>Public Presale: 10% (500,000,000 tokens)</li><li>Team and Development: 10% (500,000,000 tokens)</li><li>Educational Partners: 8% (400,000,000 tokens)</li><li>Reserve: 4% (200,000,000 tokens)</li></ul>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-6",
                        Category = "token",
                        Question = "What are the utilities of TEACH token?",
                        Answer =
                            "<p>TEACH token has multiple utilities within the ecosystem:</p><ul><li>Governance participation through the PlatformGovernance contract</li><li>Purchasing educational resources on the TeacherMarketplace</li><li>Staking to earn rewards through the TokenStaking contract</li><li>Platform stability through the PlatformStabilityFund</li><li>Rewarding verified teachers through the TeacherReward system</li></ul>"
                    },

                    // Presale FAQs
                    new FaqItemModel
                    {
                        Id = "faq-7",
                        Category = "presale",
                        Question = "How can I participate in the presale?",
                        Answer =
                            "<p>To participate in the presale:</p><ol><li>Connect your wallet (MetaMask, Trust Wallet, etc.) using the Connect Wallet button</li><li>Ensure you have USDC in your wallet on the Polygon network</li><li>Choose the presale tier you want to participate in</li><li>Enter the amount of USDC you want to spend</li><li>Approve the transaction and confirm the purchase</li></ol>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-8",
                        Category = "presale",
                        Question = "What is the vesting schedule for presale tokens?",
                        Answer =
                            "<p>Presale tokens follow a tiered vesting schedule:</p><ul><li>Tier 1: 10% at TGE, 18 months vesting</li><li>Tier 2: 15% at TGE, 15 months vesting</li><li>Tier 3: 20% at TGE, 12 months vesting</li><li>Tier 4: 20% at TGE, 9 months vesting</li><li>Tier 5: 25% at TGE, 6 months vesting</li><li>Tier 6: 30% at TGE, 4 months vesting</li><li>Tier 7: 40% at TGE, 3 months vesting</li></ul><p>After TGE, the remaining tokens vest linearly over the specified period.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-9",
                        Category = "presale",
                        Question = "What payment methods are accepted?",
                        Answer =
                            "<p>The presale accepts USDC on the Polygon network. Make sure you have USDC in your wallet on Polygon before participating.</p>"
                    },

                    // Platform FAQs
                    new FaqItemModel
                    {
                        Id = "faq-10",
                        Category = "platform",
                        Question = "How does the TeachToken platform work?",
                        Answer =
                            "<p>The TeachToken platform is built on a robust architecture of interconnected smart contracts:</p><ul><li>TeachToken: Core ERC20 token contract</li><li>TokenCrowdSale: Multi-tier presale contract</li><li>PlatformStabilityFund: Price stability and volatility protection</li><li>TokenStaking: Staking and rewards distribution</li><li>PlatformGovernance: Decentralized governance</li><li>PlatformMarketplace: Educational resource marketplace</li><li>TeacherReward: Incentives for educators</li><li>ContractRegistry: Central coordination hub</li></ul>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-11",
                        Category = "platform",
                        Question = "How does the staking system work?",
                        Answer =
                            "<p>The TokenStaking contract offers multiple staking pools with different parameters. What makes our staking system unique is the 50/50 reward split between users and schools. When you stake TEACH tokens, you choose a school beneficiary that receives half of your staking rewards, creating direct support for educational institutions.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-12",
                        Category = "platform",
                        Question = "What is the PlatformStabilityFund?",
                        Answer =
                            "<p>The PlatformStabilityFund is a price stability and volatility protection system that helps maintain the value of TEACH tokens. It works by:</p><ul><li>Maintaining a reserve ratio of stable coins</li><li>Dynamically adjusting platform fees based on token price</li><li>Providing a price floor through automatic token buybacks</li><li>Protecting against flash loan attacks and market manipulation</li></ul>"
                    },

                    // Technical FAQs
                    new FaqItemModel
                    {
                        Id = "faq-13",
                        Category = "technical",
                        Question = "Which blockchain network does TeachToken use?",
                        Answer =
                            "<p>TeachToken is deployed on the Polygon (MATIC) network, which offers fast transactions, low fees, and Ethereum compatibility. Polygon is an ideal solution for our platform's needs, providing scalability without compromising on security.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-14",
                        Category = "technical",
                        Question = "Are TeachToken smart contracts audited?",
                        Answer =
                            "<p>Yes, all TeachToken smart contracts undergo thorough auditing by multiple independent security firms, including [Audit Firm 1] and [Audit Firm 2]. Audit reports are publicly available on our website, and we maintain a bug bounty program for additional security assurance.</p>"
                    },
                    new FaqItemModel
                    {
                        Id = "faq-15",
                        Category = "technical",
                        Question = "How can I add TEACH token to my wallet?",
                        Answer =
                            "<p>To add TEACH token to your wallet:</p><ol><li>Open your wallet (e.g., MetaMask)</li><li>Click \"Add Token\" or similar option</li><li>Select \"Custom Token\"</li><li>Enter the TEACH token contract address: <code>0xabcdef123456789abcdef123456789abcdef1234</code></li><li>The token symbol (TEACH) and decimals (18) should auto-fill</li><li>Click \"Add\" to complete</li></ol>"
                    }
                };

                // Cache for 1 hour
                _cache.Set("faq_items", mockItems, TimeSpan.FromHours(1));

                return mockItems;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching FAQ items");
                return new List<FaqItemModel>();
            }
        }
    }


}