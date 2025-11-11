using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace DJBookingSystem
{
    public partial class SplashScreen : Window
    {
        private Random _random = new Random();
        private double _currentProgress = 0;
        private DateTime _startTime;
        private const int TOTAL_LOADING_TIME_MS = 12000; // 12 seconds
        private const int MESSAGE_CHANGE_INTERVAL_MS = 3000; // 3 seconds

        private readonly List<string> _humorousMessages = new List<string>
        {
            // Innuendo Collection
            "Polishing the knob...",
            "Stroking the database...",
            "Massaging the backend...",
            "Tickling the server's fancy...",
            "Getting it up and running...",
            "Warming up the hard drive...",
            "Lubricating the gears...",
            "Penetrating the firewall...",
            "Mounting the drive...",
            "Inserting the floppy...",
            "Expanding your package...",
            "Releasing the payload...",
            "Pumping up the bandwidth...",
            "Satisfying your request...",
            "Coming to a server near you...",

            // Playful Tech Humor
            "Teaching hamsters to run faster...",
            "Bribing the loading bar...",
            "Convincing pixels to cooperate...",
            "Negotiating with the CPU...",
            "Begging the RAM for mercy...",
            "Threatening the cache...",
            "Seducing the server...",
            "Making sweet love to your data...",
            "Giving the motherboard a massage...",
            "Whispering sweet nothings to the GPU...",
            "Flirting with the firewall...",
            "Romancing the router...",
            "Caressing the cables...",
            "Fondling the files...",
            "Spanking the slow processes...",

            // Roasting the User
            "This is taking longer than your last relationship...",
            "Still faster than your ex getting ready...",
            "Loading... unlike your commitment issues...",
            "Buffering like your dating life...",
            "Processing... still quicker than your response time...",
            "Almost there... that's what she said...",
            "Hang tight... like your jeans after Thanksgiving...",
            "Please wait... we're not as fast as you in bed...",
            "Loading... slower than your metabolism...",
            "Buffering... faster than your last performance...",
            "Processing... unlike your brain on Monday...",
            "Almost done... said no one ever...",
            "Hang on... we're coming... eventually...",
            "Please wait... good things come to those who wait... sometimes...",
            "Loading... taking longer than expected... story of your life...",

            // Dance & Movement
            "Shaking what the mama gave us...",
            "Twerking for your entertainment...",
            "Doing the horizontal loading dance...",
            "Getting freaky with the data...",
            "Making it rain... data packets...",
            "Dropping it low... the latency, that is...",
            "Grinding those gears...",
            "Bumping and grinding the cache...",
            "Getting down and dirty with the code...",
            "Doing the nasty... network stuff...",
            "Getting jiggy with the JavaScript...",
            "Shaking that ASCII...",
            "Bouncing those bits...",
            "Wiggling the widgets...",
            "Jiggling the Java...",

            // Modern Dating References
            "Swipe right on patience...",
            "Netflix and chill... but make it loading...",
            "Sliding into your DMs... slowly...",
            "Swiping through your data...",
            "Matching you with content...",
            "Finding your perfect match... of pixels...",
            "Setting the mood... lighting...",
            "Dimming the lights... on your screen...",
            "Position 69: Loading...",
            "Getting you in the mood... for content...",
            "Foreplay complete, now loading...",
            "Warming you up...",
            "Getting you excited...",
            "Building anticipation...",
            "Teasing you with progress...",

            // More Innuendo
            "Erecting the framework...",
            "Raising the bar... and other things...",
            "Getting a rise out of the server...",
            "Standing at attention...",
            "Saluting your patience...",
            "Pitching a tent... in the memory...",
            "Popping the cork...",
            "Uncorking the bottle...",
            "Releasing the pressure...",
            "Letting off steam...",
            "Blowing off steam...",
            "Venting the frustration...",
            "Relieving the tension...",
            "Easing the load...",
            "Handling your package with care...",

            // Connection Humor
            "Plugging it in...",
            "Making the connection...",
            "Establishing a firm handshake...",
            "Achieving full penetration... of the network...",
            "Going deep... into the database...",
            "Drilling down... into the data...",
            "Digging deeper...",
            "Getting to the bottom of it...",
            "Reaching the climax... of loading...",
            "Finishing strong...",
            "Wrapping it up...",
            "Pulling out... the data...",
            "Withdrawing from cache...",
            "Ejecting the disc...",
            "Dismounting the drive...",

            // Tech Puns
            "Byte me... we're loading...",
            "RAM it in there...",
            "Hard drive? More like hardly driving...",
            "Floppy? Not anymore...",
            "Getting your bits in a row...",
            "Sorting your junk... files...",
            "Cleaning your pipes... cache pipes...",
            "Flushing the system...",
            "Draining the buffer...",
            "Emptying the load...",
            "Dumping the cache...",
            "Purging the memory...",
            "Wiping it clean...",
            "Scrubbing the data...",
            "Polishing the output...",

            // Patience Humor
            "Hold your horses... and other things...",
            "Keep it in your pants... the impatience...",
            "Don't get your panties in a twist...",
            "Untwist those knickers...",
            "Calm your tits... we're loading...",
            "Keep your shirt on... for now...",
            "Don't blow your load... of patience...",
            "Save it for later...",
            "Hold that thought... and position...",
            "Maintain that posture...",
            "Stay in position...",
            "Don't move a muscle... yet...",
            "Hold still... we're almost there...",
            "Keep holding... it...",
            "Don't let go... of your patience...",

            // Spicy References
            "Getting saucy with the source code...",
            "Spicing things up...",
            "Adding some flavor...",
            "Making it juicy...",
            "Getting steamy...",
            "Heating things up...",
            "Turning up the heat...",
            "Making it hot...",
            "Bringing the fire...",
            "Igniting the passion... for loading...",

            // Almost There
            "Almost there... don't fake it now...",
            "So close... you can almost taste it...",
            "Nearly finished... unlike your last date...",
            "Just the tip... of the loading bar left...",
            "One more second... that's all we need... promise...",

            // Browser History Jokes
            "Loading... like your browser history, but slower...",
            "Buffering... clearing your shame cache...",
            "Processing... deleting your questionable searches...",
            "Compiling your excuses...",
            "Generating your alibi...",
            "Fabricating your story...",
            "Constructing your lies...",
            "Building your defense...",
            "Preparing your explanation...",
            "Crafting your excuse..."
        };

        public SplashScreen()
        {
            InitializeComponent();
            Loaded += SplashScreen_Loaded;
        }

        private async void SplashScreen_Loaded(object sender, RoutedEventArgs e)
        {
            _startTime = DateTime.Now;

            // Start both tasks simultaneously
            var loadingTask = SimulateLoading();
            var messageTask = RotateMessages();

            await Task.WhenAll(loadingTask, messageTask);
        }

        private async Task RotateMessages()
        {
            // Show 4 messages over 12 seconds (one every 3 seconds)
            var usedMessages = new HashSet<int>();

            for (int i = 0; i < 4; i++)
            {
                // Pick a random message we haven't used yet
                int messageIndex;
                do
                {
                    messageIndex = _random.Next(_humorousMessages.Count);
                } while (usedMessages.Contains(messageIndex) && usedMessages.Count < _humorousMessages.Count);

                usedMessages.Add(messageIndex);

                // Update message on UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    HumorousMessageTextBlock.Text = _humorousMessages[messageIndex];
                });

                // Wait 3 seconds before changing message
                await Task.Delay(MESSAGE_CHANGE_INTERVAL_MS);
            }
        }

        private async Task SimulateLoading()
        {
            while (_currentProgress < 100)
            {
                // Calculate how much time has elapsed
                var elapsed = (DateTime.Now - _startTime).TotalMilliseconds;

                // Calculate what percentage we should be at based on time
                double targetProgress = (elapsed / TOTAL_LOADING_TIME_MS) * 100;

                // Add some randomness but gradually catch up to target
                if (_currentProgress < targetProgress - 5)
                {
                    // If we're behind, catch up quickly
                    _currentProgress += _random.Next(3, 8);
                }
                else if (_currentProgress < targetProgress)
                {
                    // If we're slightly behind, catch up slowly
                    _currentProgress += _random.Next(1, 4);
                }
                else
                {
                    // If we're on target, increment slowly
                    _currentProgress += _random.Next(1, 3);
                }

                if (_currentProgress > 100)
                    _currentProgress = 100;

                // Update UI
                await Dispatcher.InvokeAsync(() =>
                {
                    // Update percentage text
                    PercentageTextBlock.Text = $"{(int)_currentProgress}%";

                    // Calculate progress bar width
                    double maxWidth = this.Width - 100 - 4; // Container width minus margins and borders
                    double newWidth = (maxWidth * _currentProgress) / 100;
                    ProgressBarFill.Width = newWidth;
                });

                // Update every 100ms for smooth animation
                await Task.Delay(100);

                // If we've reached the time limit, force 100%
                if ((DateTime.Now - _startTime).TotalMilliseconds >= TOTAL_LOADING_TIME_MS)
                {
                    _currentProgress = 100;
                }
            }

            // Show "LET'S PARTY!" message
            await Dispatcher.InvokeAsync(() =>
            {
                LoadingTextBlock.Text = "LET'S PARTY!";
                LoadingTextBlock.Foreground = System.Windows.Media.Brushes.Gold;
                HumorousMessageTextBlock.Text = "Time to get this DJ party started! 🎵";
                HumorousMessageTextBlock.Foreground = System.Windows.Media.Brushes.LightGreen;
            });

            // Wait 1 second before fading
            await Task.Delay(1000);

            // Fade out animation
            await Dispatcher.InvokeAsync(() =>
            {
                var fadeOut = (Storyboard)FindResource("FadeOutAnimation");
                fadeOut.Completed += (s, args) =>
                {
                    DialogResult = true;
                    Close();
                };
                fadeOut.Begin(this);
            });
        }
    }
}
