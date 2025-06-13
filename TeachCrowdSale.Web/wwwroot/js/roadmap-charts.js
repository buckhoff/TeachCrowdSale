// TeachCrowdSale.Web/wwwroot/js/roadmap-charts.js
// Phase 3: Syncfusion Chart Integration for Roadmap Dashboard
// Following established Material3Dark theme and design patterns

// ================================================
// ROADMAP CHART CONFIGURATION
// ================================================
const ROADMAP_CHART_CONFIG = {
    THEME: 'Material3Dark',
    COLORS: {
        PRIMARY: '#4f46e5',
        SECONDARY: '#06b6d4',
        SUCCESS: '#10b981',
        WARNING: '#f59e0b',
        DANGER: '#ef4444',
        INFO: '#8b5cf6',
        GRADIENT: ['#4f46e5', '#06b6d4', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899']
    },
    ANIMATION: {
        DURATION: 1000,
        DELAY: 100
    }
};

// ================================================
// ROADMAP CHART STATE MANAGEMENT
// ================================================
const RoadmapChartState = {
    charts: {
        developmentTimeline: null,
        milestoneProgress: null,
        categoryDistribution: null,
        progressOverTime: null,
        taskCompletion: null,
        developmentVelocity: null
    },
    data: {
        milestones: [],
        tasks: [],
        githubStats: {},
        progressHistory: []
    },
    filters: {
        dateRange: '6M',
        status: 'all',
        category: 'all'
    }
};

// ================================================
// ROADMAP CHART MANAGER
// ================================================
const RoadmapChartManager = {
    // Initialize all roadmap charts
    async initializeCharts() {
        try {
            console.log('Initializing roadmap charts...');

            // Load data first
            await this.loadChartData();

            // Create charts in parallel for better performance
            await Promise.all([
                this.createDevelopmentTimelineChart(),
                this.createMilestoneProgressChart(),
                this.createCategoryDistributionChart(),
                this.createProgressOverTimeChart(),
                this.createTaskCompletionChart(),
                this.createDevelopmentVelocityChart()
            ]);

            // Setup chart interactions
            this.setupChartInteractions();

            console.log('All roadmap charts initialized successfully');
        } catch (error) {
            console.error('Error initializing roadmap charts:', error);
            this.handleChartError(error);
        }
    },

    // Load chart data from API
    async loadChartData() {
        try {
            const response = await fetch('/Roadmap/GetProgressData');
            if (response.ok) {
                const data = await response.json();
                RoadmapChartState.data = { ...RoadmapChartState.data, ...data };
            } else {
                throw new Error('Failed to load chart data');
            }
        } catch (error) {
            console.warn('Using fallback chart data:', error);
            RoadmapChartState.data = this.getFallbackData();
        }
    },

    // Development Timeline Gantt Chart
    async createDevelopmentTimelineChart() {
        const container = document.getElementById('developmentTimelineChart');
        if (!container) return;

        const data = RoadmapChartState.data.timelineData || this.getFallbackTimelineData();

        const chart = new ej.gantt.Gantt({
            dataSource: data,
            taskFields: {
                id: 'TaskID',
                name: 'TaskName',
                startDate: 'StartDate',
                endDate: 'EndDate',
                duration: 'Duration',
                progress: 'Progress',
                dependency: 'Predecessor',
                child: 'subtasks'
            },
            height: '400px',
            theme: ROADMAP_CHART_CONFIG.THEME,
            columns: [
                { field: 'TaskName', headerText: 'Milestone', width: '250' },
                { field: 'StartDate', headerText: 'Start Date', width: '120' },
                { field: 'EndDate', headerText: 'End Date', width: '120' },
                { field: 'Progress', headerText: 'Progress', width: '100' }
            ],
            timelineSettings: {
                timelineViewMode: 'Month',
                topTier: {
                    unit: 'Month',
                    format: 'MMM yyyy'
                },
                bottomTier: {
                    unit: 'Week',
                    format: 'dd'
                }
            },
            gridLines: 'Both',
            labelSettings: {
                leftLabel: 'TaskName',
                rightLabel: 'Progress'
            },
            projectStartDate: new Date('2024-01-01'),
            projectEndDate: new Date('2025-12-31'),
            includeWeekend: true,
            allowSelection: true,
            allowSorting: true,
            allowFiltering: true,
            toolbar: ['ZoomIn', 'ZoomOut', 'ZoomToFit', 'ExcelExport', 'PdfExport'],
            splitterSettings: {
                position: "28%"
            }
        });

        chart.appendTo(container);
        RoadmapChartState.charts.developmentTimeline = chart;
    },

    // Milestone Progress Donut Charts
    async createMilestoneProgressChart() {
        const container = document.getElementById('milestoneProgressChart');
        if (!container) return;

        const data = RoadmapChartState.data.milestoneProgress || this.getFallbackMilestoneData();

        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'milestone',
                yName: 'progress',
                innerRadius: '50%',
                radius: '90%',
                dataLabel: {
                    visible: true,
                    name: 'milestone',
                    position: 'Outside',
                    connectorStyle: {
                        color: '#ffffff',
                        width: 2
                    },
                    font: {
                        color: '#ffffff',
                        size: '12px',
                        fontWeight: '500'
                    }
                },
                palettes: ROADMAP_CHART_CONFIG.COLORS.GRADIENT,
                animation: {
                    enable: true,
                    duration: ROADMAP_CHART_CONFIG.ANIMATION.DURATION
                }
            }],
            theme: ROADMAP_CHART_CONFIG.THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Bottom',
                textStyle: { color: '#ffffff' },
                padding: 15
            },
            tooltip: {
                enable: true,
                format: '${point.x}: ${point.y}% Complete',
                textStyle: { color: '#ffffff' }
            },
            center: { x: '50%', y: '50%' },
            enableAnimation: true
        });

        chart.appendTo(container);
        RoadmapChartState.charts.milestoneProgress = chart;
    },

    // Category Distribution Pie Chart
    async createCategoryDistributionChart() {
        const container = document.getElementById('categoryDistributionChart');
        if (!container) return;

        const data = RoadmapChartState.data.categoryDistribution || this.getFallbackCategoryData();

        const chart = new ej.charts.AccumulationChart({
            series: [{
                dataSource: data,
                xName: 'category',
                yName: 'count',
                radius: '85%',
                dataLabel: {
                    visible: true,
                    name: 'category',
                    position: 'Outside',
                    connectorStyle: {
                        color: '#ffffff',
                        width: 2
                    },
                    font: {
                        color: '#ffffff',
                        size: '11px',
                        fontWeight: '500'
                    }
                },
                palettes: ROADMAP_CHART_CONFIG.COLORS.GRADIENT,
                animation: {
                    enable: true,
                    duration: ROADMAP_CHART_CONFIG.ANIMATION.DURATION,
                    delay: ROADMAP_CHART_CONFIG.ANIMATION.DELAY
                }
            }],
            theme: ROADMAP_CHART_CONFIG.THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Right',
                textStyle: { color: '#ffffff' },
                padding: 10
            },
            tooltip: {
                enable: true,
                format: '${point.x}: ${point.y} items',
                textStyle: { color: '#ffffff' }
            },
            enableAnimation: true
        });

        chart.appendTo(container);
        RoadmapChartState.charts.categoryDistribution = chart;
    },

    // Progress Over Time Line Chart
    async createProgressOverTimeChart() {
        const container = document.getElementById('progressOverTimeChart');
        if (!container) return;

        const data = RoadmapChartState.data.progressHistory || this.getFallbackProgressHistory();

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelFormat: 'MMM yyyy',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' },
                intervalType: 'Months'
            },
            primaryYAxis: {
                labelFormat: '{value}%',
                maximum: 100,
                minimum: 0,
                interval: 20,
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'Line',
                dataSource: data.overall,
                xName: 'date',
                yName: 'progress',
                name: 'Overall Progress',
                width: 3,
                fill: ROADMAP_CHART_CONFIG.COLORS.PRIMARY,
                marker: {
                    visible: true,
                    width: 8,
                    height: 8,
                    fill: ROADMAP_CHART_CONFIG.COLORS.PRIMARY,
                    border: { width: 2, color: '#ffffff' }
                }
            }, {
                type: 'Line',
                dataSource: data.frontend,
                xName: 'date',
                yName: 'progress',
                name: 'Frontend',
                width: 2,
                fill: ROADMAP_CHART_CONFIG.COLORS.SECONDARY,
                marker: {
                    visible: true,
                    width: 6,
                    height: 6,
                    fill: ROADMAP_CHART_CONFIG.COLORS.SECONDARY
                }
            }, {
                type: 'Line',
                dataSource: data.backend,
                xName: 'date',
                yName: 'progress',
                name: 'Backend',
                width: 2,
                fill: ROADMAP_CHART_CONFIG.COLORS.SUCCESS,
                marker: {
                    visible: true,
                    width: 6,
                    height: 6,
                    fill: ROADMAP_CHART_CONFIG.COLORS.SUCCESS
                }
            }],
            theme: ROADMAP_CHART_CONFIG.THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Top',
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                format: '${series.name}<br/>Date: ${point.x}<br/>Progress: ${point.y}%',
                textStyle: { color: '#ffffff' }
            },
            crosshair: {
                enable: true,
                lineType: 'Both',
                line: { color: '#ffffff', width: 1 }
            },
            zoomSettings: {
                enableSelectionZooming: true,
                enablePinchZooming: true
            }
        });

        chart.appendTo(container);
        RoadmapChartState.charts.progressOverTime = chart;
    },

    // Task Completion Bar Chart
    async createTaskCompletionChart() {
        const container = document.getElementById('taskCompletionChart');
        if (!container) return;

        const data = RoadmapChartState.data.taskCompletion || this.getFallbackTaskData();

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'Category',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '{value}',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'Column',
                dataSource: data,
                xName: 'category',
                yName: 'completed',
                name: 'Completed',
                fill: ROADMAP_CHART_CONFIG.COLORS.SUCCESS,
                cornerRadius: { topLeft: 4, topRight: 4 },
                columnWidth: 0.7
            }, {
                type: 'Column',
                dataSource: data,
                xName: 'category',
                yName: 'remaining',
                name: 'Remaining',
                fill: ROADMAP_CHART_CONFIG.COLORS.WARNING,
                cornerRadius: { topLeft: 4, topRight: 4 },
                columnWidth: 0.7
            }],
            theme: ROADMAP_CHART_CONFIG.THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Top',
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                format: '${series.name}: ${point.y} tasks',
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        RoadmapChartState.charts.taskCompletion = chart;
    },

    // Development Velocity Metrics
    async createDevelopmentVelocityChart() {
        const container = document.getElementById('developmentVelocityChart');
        if (!container) return;

        const data = RoadmapChartState.data.velocityMetrics || this.getFallbackVelocityData();

        const chart = new ej.charts.Chart({
            primaryXAxis: {
                valueType: 'DateTime',
                labelFormat: 'MMM dd',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' }
            },
            primaryYAxis: {
                labelFormat: '{value}',
                majorGridLines: { color: '#333333' },
                lineStyle: { color: '#444444' },
                labelStyle: { color: '#ffffff' }
            },
            series: [{
                type: 'Column',
                dataSource: data.commits,
                xName: 'date',
                yName: 'count',
                name: 'Daily Commits',
                fill: ROADMAP_CHART_CONFIG.COLORS.INFO,
                cornerRadius: { topLeft: 2, topRight: 2 },
                columnWidth: 0.8
            }],
            theme: ROADMAP_CHART_CONFIG.THEME,
            background: 'transparent',
            legendSettings: {
                visible: true,
                position: 'Top',
                textStyle: { color: '#ffffff' }
            },
            tooltip: {
                enable: true,
                format: 'Date: ${point.x}<br/>Commits: ${point.y}',
                textStyle: { color: '#ffffff' }
            }
        });

        chart.appendTo(container);
        RoadmapChartState.charts.developmentVelocity = chart;
    },

    // Chart interaction setup
    setupChartInteractions() {
        // Chart filter controls
        this.setupFilterControls();

        // Chart export functionality
        this.setupExportControls();

        // Chart refresh functionality
        this.setupRefreshControls();
    },

    // Setup filter controls for charts
    setupFilterControls() {
        const timeRangeButtons = document.querySelectorAll('.chart-time-filter');
        timeRangeButtons.forEach(button => {
            button.addEventListener('click', async (e) => {
                e.preventDefault();

                // Update active state
                timeRangeButtons.forEach(btn => btn.classList.remove('active'));
                button.classList.add('active');

                // Update filter and refresh charts
                RoadmapChartState.filters.dateRange = button.dataset.range;
                await this.refreshChartsWithFilters();
            });
        });

        const statusFilters = document.querySelectorAll('.status-filter');
        statusFilters.forEach(filter => {
            filter.addEventListener('change', async () => {
                RoadmapChartState.filters.status = filter.value;
                await this.refreshChartsWithFilters();
            });
        });
    },

    // Setup export controls
    setupExportControls() {
        const exportBtn = document.getElementById('exportChartsBtn');
        if (exportBtn) {
            exportBtn.addEventListener('click', () => {
                this.exportAllCharts();
            });
        }
    },

    // Setup refresh controls
    setupRefreshControls() {
        const refreshBtn = document.getElementById('refreshChartsBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', async () => {
                refreshBtn.disabled = true;
                refreshBtn.textContent = 'Refreshing...';

                await this.loadChartData();
                await this.refreshAllCharts();

                refreshBtn.disabled = false;
                refreshBtn.textContent = 'Refresh';
            });
        }
    },

    // Refresh charts with current filters
    async refreshChartsWithFilters() {
        try {
            const filteredData = await this.getFilteredData();
            RoadmapChartState.data = { ...RoadmapChartState.data, ...filteredData };
            await this.refreshAllCharts();
        } catch (error) {
            console.error('Error refreshing charts with filters:', error);
        }
    },

    // Get filtered data based on current filters
    async getFilteredData() {
        const params = new URLSearchParams(RoadmapChartState.filters);
        const response = await fetch(`/Roadmap/GetProgressData?${params}`);

        if (response.ok) {
            return await response.json();
        }

        return this.getFallbackData();
    },

    // Refresh all charts
    async refreshAllCharts() {
        Object.values(RoadmapChartState.charts).forEach(chart => {
            if (chart && typeof chart.refresh === 'function') {
                chart.refresh();
            }
        });
    },

    // Export all charts
    exportAllCharts() {
        Object.entries(RoadmapChartState.charts).forEach(([name, chart]) => {
            if (chart && typeof chart.export === 'function') {
                chart.export('PNG', `roadmap-${name}-chart`);
            }
        });
    },

    // Handle chart errors
    handleChartError(error) {
        console.error('Chart error:', error);

        // Show user-friendly error message
        const errorContainer = document.getElementById('chartErrorMessage');
        if (errorContainer) {
            errorContainer.innerHTML = `
                <div class="alert alert-warning">
                    <strong>Charts temporarily unavailable</strong><br>
                    Using cached data. Please try refreshing the page.
                </div>
            `;
            errorContainer.style.display = 'block';
        }
    },

    // Fallback data methods
    getFallbackData() {
        return {
            timelineData: this.getFallbackTimelineData(),
            milestoneProgress: this.getFallbackMilestoneData(),
            categoryDistribution: this.getFallbackCategoryData(),
            progressHistory: this.getFallbackProgressHistory(),
            taskCompletion: this.getFallbackTaskData(),
            velocityMetrics: this.getFallbackVelocityData()
        };
    },

    getFallbackTimelineData() {
        return [
            {
                TaskID: 1,
                TaskName: 'Phase 1: Core Infrastructure',
                StartDate: new Date('2024-01-01'),
                EndDate: new Date('2024-03-31'),
                Duration: 90,
                Progress: 95,
                subtasks: [
                    { TaskID: 2, TaskName: 'API Development', StartDate: new Date('2024-01-01'), EndDate: new Date('2024-02-15'), Duration: 45, Progress: 100 },
                    { TaskID: 3, TaskName: 'Database Setup', StartDate: new Date('2024-02-01'), EndDate: new Date('2024-03-01'), Duration: 28, Progress: 100 },
                    { TaskID: 4, TaskName: 'Authentication System', StartDate: new Date('2024-02-15'), EndDate: new Date('2024-03-31'), Duration: 44, Progress: 85 }
                ]
            },
            {
                TaskID: 5,
                TaskName: 'Phase 2: Frontend Development',
                StartDate: new Date('2024-03-01'),
                EndDate: new Date('2024-06-30'),
                Duration: 120,
                Progress: 75,
                subtasks: [
                    { TaskID: 6, TaskName: 'Home Page', StartDate: new Date('2024-03-01'), EndDate: new Date('2024-04-01'), Duration: 31, Progress: 100 },
                    { TaskID: 7, TaskName: 'Analytics Dashboard', StartDate: new Date('2024-04-01'), EndDate: new Date('2024-05-15'), Duration: 44, Progress: 90 },
                    { TaskID: 8, TaskName: 'Roadmap Page', StartDate: new Date('2024-05-01'), EndDate: new Date('2024-06-30'), Duration: 60, Progress: 60 }
                ]
            },
            {
                TaskID: 9,
                TaskName: 'Phase 3: Integration & Testing',
                StartDate: new Date('2024-06-01'),
                EndDate: new Date('2024-09-30'),
                Duration: 120,
                Progress: 35,
                subtasks: [
                    { TaskID: 10, TaskName: 'Blockchain Integration', StartDate: new Date('2024-06-01'), EndDate: new Date('2024-08-01'), Duration: 61, Progress: 45 },
                    { TaskID: 11, TaskName: 'Testing & QA', StartDate: new Date('2024-07-15'), EndDate: new Date('2024-09-15'), Duration: 62, Progress: 25 },
                    { TaskID: 12, TaskName: 'Security Audit', StartDate: new Date('2024-08-15'), EndDate: new Date('2024-09-30'), Duration: 46, Progress: 0 }
                ]
            }
        ];
    },

    getFallbackMilestoneData() {
        return [
            { milestone: 'Infrastructure', progress: 95 },
            { milestone: 'Frontend', progress: 75 },
            { milestone: 'API Development', progress: 90 },
            { milestone: 'Testing', progress: 35 },
            { milestone: 'Security', progress: 15 },
            { milestone: 'Documentation', progress: 60 }
        ];
    },

    getFallbackCategoryData() {
        return [
            { category: 'Frontend', count: 25 },
            { category: 'Backend', count: 18 },
            { category: 'Infrastructure', count: 12 },
            { category: 'Testing', count: 15 },
            { category: 'Documentation', count: 8 },
            { category: 'Security', count: 6 }
        ];
    },

    getFallbackProgressHistory() {
        const dates = [];
        const overall = [];
        const frontend = [];
        const backend = [];

        for (let i = 6; i >= 0; i--) {
            const date = new Date();
            date.setMonth(date.getMonth() - i);
            dates.push(date);

            overall.push({ date: new Date(date), progress: 20 + (6 - i) * 12 });
            frontend.push({ date: new Date(date), progress: 15 + (6 - i) * 10 });
            backend.push({ date: new Date(date), progress: 25 + (6 - i) * 11 });
        }

        return { overall, frontend, backend };
    },

    getFallbackTaskData() {
        return [
            { category: 'Infrastructure', completed: 18, remaining: 2 },
            { category: 'Frontend', completed: 15, remaining: 8 },
            { category: 'Backend', completed: 12, remaining: 6 },
            { category: 'Testing', completed: 5, remaining: 10 },
            { category: 'Documentation', completed: 8, remaining: 4 }
        ];
    },

    getFallbackVelocityData() {
        const commits = [];
        for (let i = 30; i >= 0; i--) {
            const date = new Date();
            date.setDate(date.getDate() - i);
            commits.push({
                date: new Date(date),
                count: Math.floor(Math.random() * 10) + 1
            });
        }
        return { commits };
    }
};

// ================================================
// INITIALIZATION
// ================================================
document.addEventListener('DOMContentLoaded', async () => {
    // Wait for Syncfusion libraries to load
    if (typeof ej === 'undefined') {
        console.warn('Syncfusion libraries not loaded, retrying...');
        setTimeout(() => {
            if (typeof ej !== 'undefined') {
                RoadmapChartManager.initializeCharts();
            }
        }, 1000);
    } else {
        await RoadmapChartManager.initializeCharts();
    }
});

// Export for global access
window.RoadmapChartManager = RoadmapChartManager;
window.RoadmapChartState = RoadmapChartState;