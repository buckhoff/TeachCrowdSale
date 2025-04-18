// chartjs-interop.js - JavaScript interop functions for Chart.js

// Store charts so they can be referenced later
const charts = {};

// Create a doughnut chart
window.createDoughnutChart = (chartId, data, options) => {
    // Destroy existing chart if it exists
    if (charts[chartId]) {
        charts[chartId].destroy();
    }

    // Get the canvas element
    const ctx = document.getElementById(chartId);
    if (!ctx) {
        console.error(`Canvas element with ID ${chartId} not found`);
        return;
    }

    // Create the chart
    charts[chartId] = new Chart(ctx, {
        type: 'doughnut',
        data: data,
        options: options
    });

    return true;
};

// Create a line chart
window.createLineChart = (chartId, data, options) => {
    // Destroy existing chart if it exists
    if (charts[chartId]) {
        charts[chartId].destroy();
    }

    // Get the canvas element
    const ctx = document.getElementById(chartId);
    if (!ctx) {
        console.error(`Canvas element with ID ${chartId} not found`);
        return;
    }

    // Create the chart
    charts[chartId] = new Chart(ctx, {
        type: 'line',
        data: data,
        options: options
    });

    return true;
};

// Create a bar chart
window.createBarChart = (chartId, data, options) => {
    // Destroy existing chart if it exists
    if (charts[chartId]) {
        charts[chartId].destroy();
    }

    // Get the canvas element
    const ctx = document.getElementById(chartId);
    if (!ctx) {
        console.error(`Canvas element with ID ${chartId} not found`);
        return;
    }

    // Create the chart
    charts[chartId] = new Chart(ctx, {
        type: 'bar',
        data: data,
        options: options
    });

    return true;
};

// Update chart data
window.updateChartData = (chartId, data) => {
    if (!charts[chartId]) {
        console.error(`Chart with ID ${chartId} not found`);
        return;
    }

    charts[chartId].data = data;
    charts[chartId].update();
    return true;
};

// Destroy a chart
window.destroyChart = (chartId) => {
    if (!charts[chartId]) {
        console.error(`Chart with ID ${chartId} not found`);
        return;
    }

    charts[chartId].destroy();
    delete charts[chartId];
    return true;
};

// Resize all charts when window is resized
window.addEventListener('resize', () => {
    for (const chartId in charts) {
        charts[chartId].resize();
    }
});