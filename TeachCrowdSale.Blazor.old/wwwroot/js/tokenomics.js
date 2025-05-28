// tokenomics.js - Chart visualization for token distribution
document.addEventListener('DOMContentLoaded', function() {
    // Tokenomics Chart Configuration
    const tokenomicsChartConfig = {
        type: 'doughnut',
        data: {
            labels: [
                'Platform Ecosystem (32%)',
                'Community Incentives (22%)',
                'Initial Liquidity (14%)',
                'Public Presale (10%)',
                'Team and Development (10%)',
                'Educational Partners (8%)',
                'Reserve (4%)'
            ],
            datasets: [{
                data: [32, 22, 14, 10, 10, 8, 4],
                backgroundColor: [
                    '#4e73df', // Primary blue
                    '#1cc88a', // Success green
                    '#36b9cc', // Info blue
                    '#f6c23e', // Warning yellow
                    '#e74a3b', // Danger red
                    '#fd7e14', // Orange
                    '#6f42c1'  // Purple
                ],
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutoutPercentage: 65,
            legend: {
                display: false
            },
            tooltips: {
                backgroundColor: "rgb(255,255,255)",
                bodyFontColor: "#858796",
                borderColor: '#dddfeb',
                borderWidth: 1,
                xPadding: 15,
                yPadding: 15,
                displayColors: false,
                caretPadding: 10,
                callbacks: {
                    label: function(tooltipItem, data) {
                        return data.labels[tooltipItem.index];
                    }
                }
            }
        }
    };

    // Initialize Tokenomics Chart
    if (document.getElementById('tokenomicsChart')) {
        const ctx = document.getElementById('tokenomicsChart').getContext('2d');
        window.tokenomicsChart = new Chart(ctx, tokenomicsChartConfig);
    }

    // Token Supply Information
    const tokenSupply = {
        total: 5_000_000_000,
        platformEcosystem: 1_600_000_000,
        communityIncentives: 1_100_000_000,
        initialLiquidity: 700_000_000,
        publicPresale: 500_000_000,
        teamAndDevelopment: 500_000_000,
        educationalPartners: 400_000_000,
        reserve: 200_000_000
    };

    // Token Utility Information
    const tokenUtility = [
        {
            name: "Governance",
            description: "TEACH token holders can participate in platform governance through the PlatformGovernance smart contract."
        },
        {
            name: "Marketplace",
            description: "TEACH tokens are used to purchase educational resources on the TeacherMarketplace platform."
        },
        {
            name: "Staking",
            description: "Holders can stake tokens to earn rewards through the TokenStaking contract with 50/50 split between users and schools."
        },
        {
            name: "Platform Stability",
            description: "The PlatformStabilityFund uses TEACH tokens to protect the platform from price volatility."
        },
        {
            name: "Teacher Rewards",
            description: "Teachers receive TEACH tokens as rewards based on their contributions and verification."
        }
    ];

    // Update Dynamic Token Information (if APIs are available)
    function updateTokenInfo() {
        fetch('/api/tokeninfo')
            .then(response => response.json())
            .then(data => {
                // Update token price
                if (document.getElementById('tokenPrice')) {
                    document.getElementById('tokenPrice').textContent = '$' + data.currentPrice;
                }

                // Update token circulation
                if (document.getElementById('tokenCirculation')) {
                    document.getElementById('tokenCirculation').textContent = numberWithCommas(data.circulatingSupply);
                }

                // Update market cap
                if (document.getElementById('marketCap')) {
                    document.getElementById('marketCap').textContent = '$' + numberWithCommas(data.marketCap);
                }

                // Update holders count
                if (document.getElementById('holdersCount')) {
                    document.getElementById('holdersCount').textContent = numberWithCommas(data.holdersCount);
                }
            })
            .catch(error => {
                console.error('Error fetching token information:', error);
            });
    }

    // Format numbers with commas for better readability
    function numberWithCommas(x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    // Initial update
    updateTokenInfo();

    // Update token info every 60 seconds
    setInterval(updateTokenInfo, 60000);

    // Add event listeners for tokenomics info tabs if needed
    const tokenomicsTabs = document.querySelectorAll('.tokenomics-tab');
    if (tokenomicsTabs.length > 0) {
        tokenomicsTabs.forEach(tab => {
            tab.addEventListener('click', function(e) {
                e.preventDefault();

                // Remove active class from all tabs
                tokenomicsTabs.forEach(t => t.classList.remove('active'));

                // Add active class to clicked tab
                this.classList.add('active');

                // Show corresponding content
                const target = this.getAttribute('data-target');
                document.querySelectorAll('.tokenomics-content').forEach(content => {
                    content.style.display = 'none';
                });
                document.querySelector(target).style.display = 'block';
            });
        });
    }
});