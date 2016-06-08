using System;
using System.Windows.Input;
using RangeSliderView.Common;
using Xamarin.Forms;

namespace RangeSliderView.Component
{
	public class RangeSliderView : AbsoluteLayout
	{
		private const double PADDING = 20.0;
		private const double SLIDER_PATH_HEIGHT = 5.0;
		private const double BUTTON_SIZE = 30.0;
		private const double BUTTON_START_X = PADDING - (BUTTON_SIZE / 2.0);
		private const double BUBBLE_WIDTH = 64.0;
		private const double BUBBLE_HEIGHT = 45.0;

		// ------------------------------------------------------------

		#region Private Members

		private Size _viewSize;
		private BoxView _activeBar;
		private BoxView _inActiveLeftBar;
		private BoxView _inActiveRightBar;
		private Image _leftButton;
		private Image _rightButton;
		private RangeSliderBubble _leftBubble;
		private RangeSliderBubble _rightBubble;
		private double _leftButtonPanGesturePosX = 0.0;
		private double _rightButtonPanGesturePosX = 0.0;

		#endregion

		// ------------------------------------------------------------

		#region Constructors

		public RangeSliderView() 
		{
			// Active Bar
			_activeBar = new BoxView();

			_activeBar.SetBinding(BoxView.ColorProperty, new Binding(path: "ActiveBarColor", source: this));

			AbsoluteLayout.SetLayoutFlags(_activeBar, AbsoluteLayoutFlags.None);

			this.Children.Add(_activeBar);

			// Inactive Left Bar
			_inActiveLeftBar = new BoxView();

			_inActiveLeftBar.SetBinding(BoxView.ColorProperty, new Binding(path: "InactiveBarColor", source: this));

			AbsoluteLayout.SetLayoutFlags(_inActiveLeftBar, AbsoluteLayoutFlags.None);

			this.Children.Add(_inActiveLeftBar);

			// Inactive Right Bar
			_inActiveRightBar = new BoxView();

			_inActiveRightBar.SetBinding(BoxView.ColorProperty, new Binding(path: "InactiveBarColor", source: this));

			AbsoluteLayout.SetLayoutFlags(_inActiveRightBar, AbsoluteLayoutFlags.None);

			this.Children.Add(_inActiveRightBar);

			// Left Button
			_leftButton = new Image();

			AbsoluteLayout.SetLayoutFlags(_leftButton, AbsoluteLayoutFlags.None);

			var leftButtonPanGesture = new PanGestureRecognizer();

			leftButtonPanGesture.PanUpdated += LeftButtonPanGesture;

			_leftButton.GestureRecognizers.Add(leftButtonPanGesture);

			_leftButton.SetBinding(Image.SourceProperty, new Binding(path: "HandleImageSource", source: this));

			this.Children.Add(_leftButton);

			// Right Button
			_rightButton = new Image();

			AbsoluteLayout.SetLayoutFlags(_rightButton, AbsoluteLayoutFlags.None);

			var rightButtonPanGesture = new PanGestureRecognizer();

			rightButtonPanGesture.PanUpdated += RightButtonPanGesture;

			_rightButton.GestureRecognizers.Add(rightButtonPanGesture);

			_rightButton.SetBinding(Image.SourceProperty, new Binding(path: "HandleImageSource", source: this));

			this.Children.Add(_rightButton);

			// Left Bubble
			_leftBubble = new RangeSliderBubble() { IsVisible = this.ShowBubbles, Text = this.LeftValue.ToString("#0") };

			AbsoluteLayout.SetLayoutFlags(_leftBubble, AbsoluteLayoutFlags.None);

			_leftBubble.SetBinding(RangeSliderBubble.SourceProperty, new Binding(path: "BubbleImageSource", source: this));
			_leftBubble.SetBinding(RangeSliderBubble.FontFamilyProperty, new Binding(path: "FontFamily", source: this));
			_leftBubble.SetBinding(RangeSliderBubble.FontSizeProperty, new Binding(path: "FontSize", source: this));
			_leftBubble.SetBinding(RangeSliderBubble.TextColorProperty, new Binding(path: "TextColor", source: this));

			this.Children.Add(_leftBubble);

			// Right Bubble
			_rightBubble = new RangeSliderBubble() { IsVisible = this.ShowBubbles, Text = this.RightValue.ToString("#0") };

			AbsoluteLayout.SetLayoutFlags(_rightBubble, AbsoluteLayoutFlags.None);

			_rightBubble.SetBinding(RangeSliderBubble.SourceProperty, new Binding(path: "BubbleImageSource", source: this));
			_rightBubble.SetBinding(RangeSliderBubble.FontFamilyProperty, new Binding(path: "FontFamily", source: this));
			_rightBubble.SetBinding(RangeSliderBubble.FontSizeProperty, new Binding(path: "FontSize", source: this));
			_rightBubble.SetBinding(RangeSliderBubble.TextColorProperty, new Binding(path: "TextColor", source: this));

			this.Children.Add(_rightBubble);
		}

		#endregion

		// ------------------------------------------------------------

		#region Overrides

		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			_viewSize = new Size(width, height);

			UpdateViews();
		}

		protected override void OnPropertyChanged(string propertyName)
		{
			base.OnPropertyChanged(propertyName);

			if(propertyName == RangeSliderView.MinValueProperty.PropertyName ||
				propertyName == RangeSliderView.MaxValueProperty.PropertyName ||
				propertyName == RangeSliderView.LeftValueProperty.PropertyName ||
				propertyName == RangeSliderView.RightValueProperty.PropertyName ||
				propertyName == RangeSliderView.StepProperty.PropertyName)
			{
				UpdateViews();
			}

			if(propertyName == RangeSliderView.LeftValueProperty.PropertyName)
			{
				NotifyLeftValueChanged();
			}

			if(propertyName == RangeSliderView.RightValueProperty.PropertyName)
			{
				NotifyRightValueChanged();
			}

			if(propertyName == RangeSliderView.ShowBubblesProperty.PropertyName)
			{
				_leftBubble.IsVisible = this.ShowBubbles;
				_rightBubble.IsVisible = this.ShowBubbles;
			}
		}

		#endregion

		// ------------------------------------------------------------

		#region Events

		public event EventHandler<EventArgs<double>> LeftValueChanged;
		public event EventHandler<EventArgs<double>> RightValueChanged;
		public event EventHandler<EventArgs> ValueChangeCompleted;

		#endregion

		// ------------------------------------------------------------

		#region Commands

		public static readonly BindableProperty LeftValueChangedCommandProperty = BindableProperty.Create(nameof(LeftValueChangedCommand), typeof(ICommand), typeof(RangeSliderView), null);
		public ICommand LeftValueChangedCommand {
			get { return (ICommand)GetValue(LeftValueChangedCommandProperty); }
			set { SetValue(LeftValueChangedCommandProperty, value); }
		}

		public static readonly BindableProperty RightValueChangedCommandProperty = BindableProperty.Create(nameof(RightValueChangedCommand), typeof(ICommand), typeof(RangeSliderView), null);
		public ICommand RightValueChangedCommand {
			get { return (ICommand)GetValue(RightValueChangedCommandProperty); }
			set { SetValue(RightValueChangedCommandProperty, value); }
		}

		public static readonly BindableProperty ValueChangeCompletedCommandProperty = BindableProperty.Create(nameof(ValueChangeCompletedCommand), typeof(ICommand), typeof(RangeSliderView), null);
		public ICommand ValueChangeCompletedCommand {
			get { return (ICommand)GetValue(ValueChangeCompletedCommandProperty); }
			set { SetValue(ValueChangeCompletedCommandProperty, value); }
		}

		#endregion

		// ------------------------------------------------------------

		#region Public Properties

		[TypeConverter(typeof(ImageSourceConverter))]
		public static readonly BindableProperty BubbleImageSourceProperty = BindableProperty.Create(nameof(BubbleImageSource), typeof(ImageSource), typeof(RangeSliderView), null);
		public ImageSource BubbleImageSource
		{
			get { return (ImageSource)GetValue(BubbleImageSourceProperty); }
			set { SetValue(BubbleImageSourceProperty, value); }
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public static readonly BindableProperty HandleImageSourceProperty = BindableProperty.Create(nameof(HandleImageSource), typeof(ImageSource), typeof(RangeSliderView), null);
		public ImageSource HandleImageSource
		{
			get { return (ImageSource)GetValue(HandleImageSourceProperty); }
			set { SetValue(HandleImageSourceProperty, value); }
		}

		public static readonly BindableProperty ActiveBarColorProperty = BindableProperty.Create(nameof(ActiveBarColor), typeof(Color), typeof(RangeSliderView), Color.Black);
		public Color ActiveBarColor
		{
			get { return (Color)GetValue(ActiveBarColorProperty); }
			set { SetValue(ActiveBarColorProperty, value); }
		}

		public static readonly BindableProperty InactiveBarColorProperty = BindableProperty.Create(nameof(InactiveBarColor), typeof(Color), typeof(RangeSliderView), Color.Black);
		public Color InactiveBarColor
		{
			get { return (Color)GetValue(InactiveBarColorProperty); }
			set { SetValue(InactiveBarColorProperty, value); }
		}

		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(RangeSliderView), string.Empty);
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(RangeSliderView), 10.0);
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(RangeSliderView), Color.Black);
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}

		public static readonly BindableProperty MinValueProperty = BindableProperty.Create(nameof(MinValue), typeof(double), typeof(RangeSliderView), 0.0);
		public double MinValue {
			get { return (double)GetValue(MinValueProperty); }
			set { SetValue(MinValueProperty, value); }
		}

		public static readonly BindableProperty MaxValueProperty = BindableProperty.Create(nameof(MaxValue), typeof(double), typeof(RangeSliderView), 0.0);
		public double MaxValue {
			get { return (double)GetValue(MaxValueProperty); }
			set { SetValue(MaxValueProperty, value); }
		}

		public static readonly BindableProperty LeftValueProperty = BindableProperty.Create(nameof(LeftValue), typeof(double), typeof(RangeSliderView), 0.0);
		public double LeftValue {
			get { return (double)GetValue(LeftValueProperty); }
			set { SetValue(LeftValueProperty, value); }
		}

		public static readonly BindableProperty RightValueProperty = BindableProperty.Create(nameof(RightValue), typeof(double), typeof(RangeSliderView), 0.0);
		public double RightValue {
			get { return (double)GetValue(RightValueProperty); }
			set { SetValue(RightValueProperty, value); }
		}

		public static readonly BindableProperty StepProperty = BindableProperty.Create(nameof(Step), typeof(double), typeof(RangeSliderView), 0.0);
		public double Step {
			get { return (double)GetValue(StepProperty); }
			set { SetValue(StepProperty, value); }
		}

		public static readonly BindableProperty ShowBubblesProperty = BindableProperty.Create(nameof(ShowBubbles), typeof(bool), typeof(RangeSliderView), false);
		public bool ShowBubbles {
			get { return (bool)GetValue(ShowBubblesProperty); }
			set { SetValue(ShowBubblesProperty, value); }
		}

		#endregion

		// ------------------------------------------------------------

		#region Private Methods

		private void NotifyLeftValueChanged()
		{
			_leftBubble.Text = this.LeftValue.ToString("#0");

			if (this.LeftValueChanged != null) 
			{
				this.LeftValueChanged(this, new EventArgs<double>(this.LeftValue));
			}

			if (this.LeftValueChangedCommand != null && this.LeftValueChangedCommand.CanExecute(this.LeftValue)) 
			{
				this.LeftValueChangedCommand.Execute(this.LeftValue);
			}
		}

		private void NotifyRightValueChanged()
		{
			_rightBubble.Text = this.RightValue.ToString("#0");

			if (this.RightValueChanged != null) 
			{
				this.RightValueChanged(this, new EventArgs<double>(this.RightValue));
			}

			if (this.RightValueChangedCommand != null && this.RightValueChangedCommand.CanExecute(this.RightValue)) 
			{
				this.RightValueChangedCommand.Execute(this.RightValue);
			}
		}

		private void ChangeValueFinished() 
		{
			if(this.ValueChangeCompleted != null)
			{
				this.ValueChangeCompleted(this, new EventArgs());
			}

			if (this.ValueChangeCompletedCommand != null && this.ValueChangeCompletedCommand.CanExecute(null)) 
			{
				this.ValueChangeCompletedCommand.Execute(null);
			}
		}

		private void UpdateViews()
		{
			if(_viewSize.Width < 1 || _viewSize.Height < 1 || this.Step < 1 || this.Step > (this.MaxValue - this.MinValue) || this.MinValue == this.MaxValue)
			{
				return;
			}

			// Calculate Total Bar Width
			var totalBarWidth = _viewSize.Width - (PADDING * 2.0);

			// Calculate Steps
			var numberOfSteps = (this.MaxValue - this.MinValue) / this.Step;
			var pixelsPerStep = totalBarWidth / numberOfSteps;

			// Position Left Button
			var leftButtonX = BUTTON_START_X + ((this.LeftValue / this.Step) * pixelsPerStep);

			_leftButton.Layout(new Rectangle(leftButtonX, (_viewSize.Height - BUTTON_SIZE) / 2.0, BUTTON_SIZE, BUTTON_SIZE));

			// Position Right Button
			var rightButtonX = BUTTON_START_X + ((this.RightValue / this.Step) * pixelsPerStep);

			_rightButton.Layout(new Rectangle(rightButtonX, (_viewSize.Height - BUTTON_SIZE) / 2.0, BUTTON_SIZE, BUTTON_SIZE));

			// Position Left Bubble
			var bubbleY = ((_viewSize.Height - BUTTON_SIZE) / 2.0) + BUTTON_SIZE + 5.0;

			_leftBubble.Layout(new Rectangle(leftButtonX + (BUTTON_SIZE / 2.0) - (BUBBLE_WIDTH / 2.0), bubbleY, BUBBLE_WIDTH, BUBBLE_HEIGHT));

			// Position Right Bubble
			_rightBubble.Layout(new Rectangle(rightButtonX + (BUTTON_SIZE / 2.0) - (BUBBLE_WIDTH / 2.0), bubbleY, BUBBLE_WIDTH, BUBBLE_HEIGHT));

			// Position Bars
			_activeBar.Layout(new Rectangle(leftButtonX + (BUTTON_SIZE / 2.0), (_viewSize.Height - SLIDER_PATH_HEIGHT) / 2.0, rightButtonX - leftButtonX, SLIDER_PATH_HEIGHT));
			_inActiveLeftBar.Layout(new Rectangle(PADDING, (_viewSize.Height - SLIDER_PATH_HEIGHT) / 2.0, leftButtonX - PADDING, SLIDER_PATH_HEIGHT));
			_inActiveRightBar.Layout(new Rectangle(rightButtonX + (BUTTON_SIZE / 2.0), (_viewSize.Height - SLIDER_PATH_HEIGHT) / 2.0, totalBarWidth - rightButtonX + (BUTTON_SIZE / 2.0), SLIDER_PATH_HEIGHT));
		}

		private void LeftButtonPanGesture(object sender, PanUpdatedEventArgs e)
		{
			var totalBarWidth = _viewSize.Width - (PADDING * 2.0);
			var numberOfSteps = (this.MaxValue - this.MinValue) / this.Step;
			var pixelsPerStep = totalBarWidth / numberOfSteps;

			switch (e.StatusType)
			{
				case GestureStatus.Started:
					_leftButtonPanGesturePosX = _leftButton.X;
					break;
				case GestureStatus.Running:
					var leftButtonX = _leftButtonPanGesturePosX + e.TotalX;

					var newLeftValue = ((leftButtonX - BUTTON_START_X) / pixelsPerStep) * this.Step;

					newLeftValue = AdjustLeftValue(newLeftValue);

					if(newLeftValue != this.LeftValue)
					{
						this.LeftValue = newLeftValue;
					}

					break;

				case GestureStatus.Completed:
					_leftButtonPanGesturePosX = 0.0;
					ChangeValueFinished();
					break;
			}
		}

		private void RightButtonPanGesture(object sender, PanUpdatedEventArgs e)
		{
			var sliderPathWidth = _viewSize.Width - (PADDING * 2.0);
			var numberOfSteps = (this.MaxValue - this.MinValue) / this.Step;
			var pixelsPerStep = sliderPathWidth / numberOfSteps;

			switch (e.StatusType)
			{
				case GestureStatus.Started:
					_rightButtonPanGesturePosX = _rightButton.X;
					break;
				case GestureStatus.Running:
					var rightButtonX = _rightButtonPanGesturePosX + e.TotalX;
					var newRightValue = ((rightButtonX - BUTTON_START_X) / pixelsPerStep) * this.Step;

					newRightValue = AdjustRightValue(newRightValue);

					if(newRightValue != this.RightValue)
					{
						this.RightValue = newRightValue;
					}

					break;

				case GestureStatus.Completed:
					_rightButtonPanGesturePosX = 0.0;
					ChangeValueFinished();
					break;
			}
		}

		private double AdjustLeftValue(double leftValue)
		{
			var adjustedValue = leftValue;

			if(leftValue < this.MinValue)
			{
				adjustedValue = this.MinValue;
			}

			if(leftValue > this.RightValue)
			{
				adjustedValue = this.RightValue;
			}

			return Math.Round(adjustedValue / this.Step) * this.Step;
		}

		private double AdjustRightValue(double rightValue)
		{
			var adjustedValue = rightValue;

			if(rightValue > this.MaxValue)
			{
				adjustedValue = this.MaxValue;
			}

			if(rightValue < this.LeftValue)
			{
				adjustedValue = this.LeftValue;
			}

			return Math.Round(adjustedValue / this.Step) * this.Step;
		}

		#endregion
	}

	public class RangeSliderBubble : AbsoluteLayout
	{
		private Image _image;
		private Label _label;

		public RangeSliderBubble()
		{
			// Init
			this.BackgroundColor = Color.Transparent;

			// Image
			_image = new Image();

			AbsoluteLayout.SetLayoutFlags(_image, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(_image, new Rectangle(0f, 0f, 1f, 1f));

			_image.SetBinding(Image.SourceProperty, new Binding(path: "Source", source: this));

			this.Children.Add(_image);

			// Label
			_label = new Label() { VerticalTextAlignment = TextAlignment.Center, HorizontalTextAlignment = TextAlignment.Center };

			AbsoluteLayout.SetLayoutFlags(_label, AbsoluteLayoutFlags.All);
			AbsoluteLayout.SetLayoutBounds(_label, new Rectangle(0.5f, 0.75f, 0.8f, 0.5f));

			_label.SetBinding(Label.TextProperty, new Binding(path: "Text", source: this));
			_label.SetBinding(Label.FontFamilyProperty, new Binding(path: "FontFamily", source: this));
			_label.SetBinding(Label.FontSizeProperty, new Binding(path: "FontSize", source: this));
			_label.SetBinding(Label.TextColorProperty, new Binding(path: "TextColor", source: this));

			this.Children.Add(_label);
		}

		[TypeConverter(typeof(ImageSourceConverter))]
		public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(ImageSource), typeof(RangeSliderBubble), null);
		public ImageSource Source
		{
			get { return (ImageSource)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(RangeSliderBubble), string.Empty);
		public string Text
		{
			get { return (string)GetValue(TextProperty); }
			set { SetValue(TextProperty, value); }
		}

		public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(RangeSliderBubble), string.Empty);
		public string FontFamily
		{
			get { return (string)GetValue(FontFamilyProperty); }
			set { SetValue(FontFamilyProperty, value); }
		}

		public static BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(RangeSliderBubble), 10.0);
		public double FontSize
		{
			get { return (double)GetValue(FontSizeProperty); }
			set { SetValue(FontSizeProperty, value); }
		}

		public static BindableProperty TextColorProperty = BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(RangeSliderBubble), Color.Black);
		public Color TextColor
		{
			get { return (Color)GetValue(TextColorProperty); }
			set { SetValue(TextColorProperty, value); }
		}
	}
}