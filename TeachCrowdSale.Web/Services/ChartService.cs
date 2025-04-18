using System.Text.Json;
using Microsoft.JSInterop;

namespace TeachTokenCrowdsale.Web.Services
{
    public class ChartService
    {
        private readonly IJSRuntime _jsRuntime;

        public ChartService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task CreateDoughnutChartAsync(string chartId, Dictionary<string, object> data, Dictionary<string, object> options)
        {
            await _jsRuntime.InvokeVoidAsync("createDoughnutChart", chartId, data, options);
        }

        public async Task CreateLineChartAsync(string chartId, Dictionary<string, object> data, Dictionary<string, object> options)
        {
            await _jsRuntime.InvokeVoidAsync("createLineChart", chartId, data, options);
        }

        public async Task CreateBarChartAsync(string chartId, Dictionary<string, object> data, Dictionary<string, object> options)
        {
            await _jsRuntime.InvokeVoidAsync("createBarChart", chartId, data, options);
        }

        public async Task UpdateChartDataAsync(string chartId, Dictionary<string, object> data)
        {
            await _jsRuntime.InvokeVoidAsync("updateChartData", chartId, data);
        }

        public async Task DestroyChartAsync(string chartId)
        {
            await _jsRuntime.InvokeVoidAsync("destroyChart", chartId);
        }

        public Dictionary<string, object> GetTokenomicsChartData()
        {
            // Tokenomics data
            return new Dictionary<string, object>
            {
                ["labels"] = new[]
                {
                    "Platform Ecosystem (32%)",
                    "Community Incentives (22%)",
                    "Initial Liquidity (14%)",
                    "Public Presale (10%)",
                    "Team and Development (10%)",
                    "Educational Partners (8%)",
                    "Reserve (4%)"
                },
                ["datasets"] = new[]
                {
                    new
                    {
                        data = new[] { 32, 22, 14, 10, 10, 8, 4 },
                        backgroundColor = new[]
                        {
                            "#4e73df", // Primary blue
                            "#1cc88a", // Success green
                            "#36b9cc", // Info blue
                            "#f6c23e", // Warning yellow
                            "#e74a3b", // Danger red
                            "#fd7e14", // Orange
                            "#6f42c1"  // Purple
                        },
                        borderWidth = 1
                    }
                }
            };
        }

        public Dictionary<string, object> GetTokenPriceChartData(List<DateTime> dates, List<decimal> prices)
        {
            // Format dates for Chart.js
            var formattedDates = dates.Select(d => d.ToString("MMM dd")).ToArray();
            
            return new Dictionary<string, object>
            {
                ["labels"] = formattedDates,
                ["datasets"] = new[]
                {
                    new
                    {
                        label = "Token Price (USD)",
                        data = prices.Select(p => (object)p).ToArray(),
                        backgroundColor = "rgba(78, 115, 223, 0.05)",
                        borderColor = "rgba(78, 115, 223, 1)",
                        pointRadius = 3,
                        pointBackgroundColor = "rgba(78, 115, 223, 1)",
                        pointBorderColor = "rgba(78, 115, 223, 1)",
                        pointHoverRadius = 5,
                        pointHoverBackgroundColor = "rgba(78, 115, 223, 1)",
                        pointHoverBorderColor = "rgba(78, 115, 223, 1)",
                        pointHitRadius = 10,
                        pointBorderWidth = 2,
                        lineTension = 0.3
                    }
                }
            };
        }

        public Dictionary<string, object> GetDefaultChartOptions()
        {
            return new Dictionary<string, object>
            {
                ["responsive"] = true,
                ["maintainAspectRatio"] = false,
                ["plugins"] = new
                {
                    legend = new
                    {
                        display = true,
                        position = "top"
                    },
                    tooltip = new
                    {
                        backgroundColor = "rgb(255,255,255)",
                        bodyFontColor = "#858796",
                        titleFontColor = "#6e707e",
                        borderColor = "#dddfeb",
                        borderWidth = 1,
                        xPadding = 15,
                        yPadding = 15,
                        displayColors = false,
                        caretPadding = 10
                    }
                }
            };
        }
    }
}