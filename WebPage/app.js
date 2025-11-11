// Firebase Configuration
const firebaseConfig = {
    databaseURL: "https://new-booking-system-46908-default-rtdb.firebaseio.com/"
};

// Initialize Firebase
firebase.initializeApp(firebaseConfig);
const database = firebase.database();

// Global variables
let currentBookings = [];
let currentVenues = [];

// Initialize the app
document.addEventListener('DOMContentLoaded', () => {
    loadTodaysSchedule();
    loadVenues();
    loadCurrentlyPlaying();
    updateStats();

    // Refresh data every 30 seconds
    setInterval(() => {
        loadTodaysSchedule();
        loadVenues();
        updateStats();
    }, 30000);
});

// Load today's schedule
async function loadTodaysSchedule() {
    try {
        const bookingsRef = database.ref('bookings');
        const snapshot = await bookingsRef.once('value');
        const bookings = [];

        snapshot.forEach((childSnapshot) => {
            const booking = childSnapshot.val();
            booking.id = childSnapshot.key;
            bookings.push(booking);
        });

        // Filter for today's bookings
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        currentBookings = bookings.filter(booking => {
            const bookingDate = new Date(booking.BookingDate);
            bookingDate.setHours(0, 0, 0, 0);
            return bookingDate.getTime() === today.getTime();
        });

        // Sort by time
        currentBookings.sort((a, b) => new Date(a.BookingDate) - new Date(b.BookingDate));

        displaySchedule(currentBookings);
    } catch (error) {
        console.error('Error loading schedule:', error);
        document.getElementById('scheduleList').innerHTML = '<p class="loading">Failed to load schedule</p>';
    }
}

// Display schedule
function displaySchedule(bookings) {
    const scheduleList = document.getElementById('scheduleList');

    if (bookings.length === 0) {
        scheduleList.innerHTML = '<p class="loading">No bookings scheduled for today</p>';
        return;
    }

    const now = new Date();

    scheduleList.innerHTML = bookings.map(booking => {
        const bookingTime = new Date(booking.BookingDate);
        const endTime = new Date(bookingTime.getTime() + (booking.DurationHours || 1) * 60 * 60 * 1000);
        const isLive = now >= bookingTime && now <= endTime;
        const isPending = now < bookingTime;

        let statusClass = 'status-confirmed';
        let statusText = 'Confirmed';

        if (isLive) {
            statusClass = 'status-live';
            statusText = '🔴 LIVE NOW';
        } else if (isPending) {
            statusClass = 'status-pending';
            statusText = 'Upcoming';
        } else if (booking.Status === 'Pending') {
            statusClass = 'status-pending';
            statusText = 'Pending';
        }

        return `
            <div class="schedule-item">
                <div class="schedule-time">
                    ${formatTime(bookingTime)}
                </div>
                <div class="schedule-details">
                    <h3>${booking.DJName}</h3>
                    <p>📍 ${booking.Venue}</p>
                    <p>⏱️ ${formatTime(bookingTime)} - ${formatTime(endTime)}</p>
                </div>
                <div class="schedule-status ${statusClass}">
                    ${statusText}
                </div>
            </div>
        `;
    }).join('');
}

// Load venues
async function loadVenues() {
    try {
        const venuesRef = database.ref('venues');
        const snapshot = await venuesRef.once('value');
        const venues = [];

        snapshot.forEach((childSnapshot) => {
            const venue = childSnapshot.val();
            venue.id = childSnapshot.key;
            venues.push(venue);
        });

        currentVenues = venues;
        displayVenues(venues);
        updateVenueFilter(venues);
    } catch (error) {
        console.error('Error loading venues:', error);
        document.getElementById('venuesList').innerHTML = '<p class="loading">Failed to load venues</p>';
    }
}

// Display venues
function displayVenues(venues) {
    const venuesList = document.getElementById('venuesList');

    if (venues.length === 0) {
        venuesList.innerHTML = '<p class="loading">No venues available</p>';
        return;
    }

    venuesList.innerHTML = venues.map(venue => `
        <div class="venue-card">
            <h3>${venue.RoomName}</h3>
            <p>${venue.RoomDescription}</p>
            <p><strong>Hours:</strong> ${venue.OpeningHours}</p>
            <span class="venue-status ${venue.IsOpen ? 'status-open' : 'status-closed'}">
                ${venue.IsOpen ? '✓ Open for Bookings' : '✗ Closed'}
            </span>
        </div>
    `).join('');
}

// Update venue filter
function updateVenueFilter(venues) {
    const venueFilter = document.getElementById('venueFilter');
    const currentValue = venueFilter.value;

    venueFilter.innerHTML = '<option value="all">All Venues</option>' +
        venues.map(venue => `<option value="${venue.RoomName}">${venue.RoomName}</option>`).join('');

    if (currentValue) {
        venueFilter.value = currentValue;
    }
}

// Filter schedule by venue
function filterSchedule() {
    const venueFilter = document.getElementById('venueFilter');
    const selectedVenue = venueFilter.value;

    if (selectedVenue === 'all') {
        displaySchedule(currentBookings);
    } else {
        const filtered = currentBookings.filter(booking => booking.Venue === selectedVenue);
        displaySchedule(filtered);
    }
}

// Load currently playing
function loadCurrentlyPlaying() {
    const now = new Date();
    const currentBooking = currentBookings.find(booking => {
        const bookingTime = new Date(booking.BookingDate);
        const endTime = new Date(bookingTime.getTime() + (booking.DurationHours || 1) * 60 * 60 * 1000);
        return now >= bookingTime && now <= endTime;
    });

    if (currentBooking) {
        document.getElementById('currentDJ').textContent = currentBooking.DJName;
        document.getElementById('currentVenue').textContent = `At ${currentBooking.Venue}`;

        const bookingTime = new Date(currentBooking.BookingDate);
        const endTime = new Date(bookingTime.getTime() + (currentBooking.DurationHours || 1) * 60 * 60 * 1000);
        document.getElementById('currentTime').textContent =
            `${formatTime(bookingTime)} - ${formatTime(endTime)}`;
    } else {
        document.getElementById('currentDJ').textContent = 'No DJ currently live';
        document.getElementById('currentVenue').textContent = 'Check the schedule for upcoming sets';
        document.getElementById('currentTime').textContent = '';
    }
}

// Update statistics
function updateStats() {
    // Simulate live listeners (you can connect this to actual analytics)
    document.getElementById('liveListeners').textContent = Math.floor(Math.random() * 100) + 50;
    document.getElementById('todayBookings').textContent = currentBookings.length;
    document.getElementById('totalVenues').textContent = currentVenues.filter(v => v.IsOpen).length;
}

// Load custom stream
function loadCustomStream() {
    const streamUrl = document.getElementById('streamUrl').value.trim();

    if (!streamUrl) {
        alert('Please enter a stream URL');
        return;
    }

    if (!streamUrl.startsWith('http://') && !streamUrl.startsWith('https://')) {
        alert('Please enter a valid URL starting with http:// or https://');
        return;
    }

    const audioPlayer = document.getElementById('audioPlayer');
    const audioSource = document.getElementById('audioSource');

    audioSource.src = streamUrl;
    audioPlayer.load();
    audioPlayer.play().catch(error => {
        alert('Failed to load stream. Please check the URL and try again.');
        console.error('Stream load error:', error);
    });
}

// Format time
function formatTime(date) {
    return date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    });
}

// Open desktop app
function openDesktopApp() {
    alert('To download the DJ Booking System desktop application:\n\n' +
          '1. Contact your administrator for the download link\n' +
          '2. Or visit our GitHub releases page\n\n' +
          'The desktop app allows you to:\n' +
          '- Book DJ slots\n' +
          '- Register venues\n' +
          '- Manage your schedule\n' +
          '- Stream live audio\n' +
          '- And much more!');
}

// Smooth scrolling for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({
                behavior: 'smooth',
                block: 'start'
            });
        }
    });
});
