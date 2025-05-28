// timer.js - Countdown timer for the presale stages
document.addEventListener('DOMContentLoaded', function() {
    // Initialize countdown timer
    const countdownElement = document.getElementById('countdown');
    if (!countdownElement) return;

    // Fetch current tier end time from API
    function fetchTierEndTime() {
        return fetch('/api/presale/current-tier')
            .then(response => response.json())
            .then(data => {
                return new Date(data.endTime).getTime();
            })
            .catch(error => {
                console.error('Error fetching tier end time:', error);
                // Fallback to a fixed end date if API fails
                const fallbackDate = new Date();
                fallbackDate.setDate(fallbackDate.getDate() + 14); // 14 days from now
                return fallbackDate.getTime();
            });
    }

    // Initialize with static end date until API responds
    let countdownEndTime = new Date();
    countdownEndTime.setDate(countdownEndTime.getDate() + 14); // Default 14 days

    // Update countdown every second
    const countdownTimer = setInterval(updateCountdown, 1000);

    // Update countdown end time from API
    fetchTierEndTime().then(endTime => {
        countdownEndTime = endTime;
    });

    // Update countdown display
    function updateCountdown() {
        // Get current time
        const now = new Date().getTime();

        // Find the distance between now and the countdown end time
        const distance = countdownEndTime - now;

        // Time calculations for days, hours, minutes and seconds
        const days = Math.floor(distance / (1000 * 60 * 60 * 24));
        const hours = Math.floor((distance % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
        const minutes = Math.floor((distance % (1000 * 60 * 60)) / (1000 * 60));
        const seconds = Math.floor((distance % (1000 * 60)) / 1000);

        // Format display with leading zeros
        document.getElementById('days').textContent = days.toString().padStart(2, '0');
        document.getElementById('hours').textContent = hours.toString().padStart(2, '0');
        document.getElementById('minutes').textContent = minutes.toString().padStart(2, '0');
        document.getElementById('seconds').textContent = seconds.toString().padStart(2, '0');

        // If the countdown is finished, clear interval and update UI
        if (distance < 0) {
            clearInterval(countdownTimer);
            document.getElementById('days').textContent = '00';
            document.getElementById('hours').textContent = '00';
            document.getElementById('minutes').textContent = '00';
            document.getElementById('seconds').textContent = '00';

            // Show tier ended message
            const countdownParent = document.getElementById('countdown').parentElement;
            const endedMessage = document.createElement('div');
            endedMessage.className = 'alert alert-warning mt-3';
            endedMessage.textContent = 'This tier has ended. Please check current active tier.';
            countdownParent.appendChild(endedMessage);

            // Fetch new tier information
            fetchPresaleStatus();
        }
    }

    // Fetch and update presale status (progress, raised amount, etc.)
    function fetchPresaleStatus() {
        fetch('/api/presale/status')
            .then(response => response.json())
            .then(data => {
                // Update total raised
                if (document.getElementById('totalRaised')) {
                    document.getElementById('totalRaised').textContent = numberWithCommas(data.totalRaised);
                }

                // Update progress bar
                if (document.querySelector('.progress-bar')) {
                    const progressPercent = (data.totalRaised / data.fundingGoal) * 100;
                    document.querySelector('.progress-bar').style.width = progressPercent + '%';
                    document.querySelector('.progress-bar').textContent = Math.round(progressPercent) + '%';
                    document.querySelector('.progress-bar').setAttribute('aria-valuenow', progressPercent);
                }

                // Update current tier information
                if (document.getElementById('currentTier')) {
                    document.getElementById('currentTier').textContent = data.currentTier.name;
                }

                // If tier has changed, update the end time and restart countdown
                if (new Date(data.currentTier.endTime).getTime() !== countdownEndTime) {
                    countdownEndTime = new Date(data.currentTier.endTime).getTime();

                    // Clear any tier ended message
                    const endedMessage = document.querySelector('.alert.alert-warning');
                    if (endedMessage) {
                        endedMessage.remove();
                    }

                    // Restart countdown if it was cleared
                    if (!countdownTimer) {
                        countdownTimer = setInterval(updateCountdown, 1000);
                    }
                }
            })
            .catch(error => {
                console.error('Error fetching presale status:', error);
            });
    }

    // Format numbers with commas for better readability
    function numberWithCommas(x) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
    }

    // Initial fetch of presale status
    fetchPresaleStatus();

    // Update presale status every 60 seconds
    setInterval(fetchPresaleStatus, 60000);
});