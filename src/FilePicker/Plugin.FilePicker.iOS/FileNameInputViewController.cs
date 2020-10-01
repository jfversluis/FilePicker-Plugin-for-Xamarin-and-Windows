using System;
using System.Drawing;
using System.IO;
using CoreFoundation;
using CoreGraphics;
using EventKitUI;
using UIKit;
using Foundation;

namespace Plugin.FilePicker
{
    public class FileNameInputViewController : UIViewController
    {
        private UITextField _fileNameField;
        private UILabel _extensionField;
        private readonly string[] _extensions;

        public FileNameInputViewController(string[] extensions)
        {
            this._extensions = extensions;
        }

        public string FileName { get; private set; }

        public event EventHandler OnViewDidDisappear;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Perform any additional setup after loading the view
            if (View == null)
                return;

            View.BackgroundColor = UIColor.White.ColorWithAlpha(0.8f);

            nfloat elementHeight = 32f;
            nfloat elementWidth = View.Bounds.Width < View.Bounds.Height ? View.Bounds.Width - 48 : View.Bounds.Height - 48;
            nfloat spacing = 8;
            nfloat height = 0;

            bool hasExtension = _extensions != null && _extensions.Length > 0;
            bool hasSegmentedControl = _extensions != null && _extensions.Length > 1;

            _fileNameField = new UITextField(new CGRect(0, 0, elementWidth - 60, elementHeight))
            {
                Placeholder = "Enter New File Name",
                BorderStyle = UITextBorderStyle.RoundedRect,
            };

            _extensionField = null;

            if (hasExtension)
            {
                _extensionField = new UILabel(new CGRect(0, 0, 60, elementHeight))
                {
                    Text = _extensions[0],
                    TextAlignment = UITextAlignment.Center,
                };
            }

            var filenameStackView = new UIStackView(new CGRect(0, 0, elementWidth, elementHeight))
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.FillProportionally,
                Spacing = 2,
            };

            height += elementHeight + spacing;

            filenameStackView.AddArrangedSubview(_fileNameField);
            if(_extensionField != null) filenameStackView.AddArrangedSubview(_extensionField);

            UISegmentedControl segmentedControl = null;

            if (hasSegmentedControl)
            {
                segmentedControl = new UISegmentedControl
                {
                    Frame = new CGRect(0, 0, elementWidth, elementHeight),
                    BackgroundColor = UIColor.FromRGB(0.8f, 0.8f, 0.8f),
                };

                for (var i = 0; i < _extensions.Length; i++)
                {
                    segmentedControl.InsertSegment(_extensions[i], i, false);
                }

                segmentedControl.SelectedSegment = 0;

                segmentedControl.ValueChanged += (sender, args) =>
                {
                    _extensionField.Text = _extensions[segmentedControl.SelectedSegment];
                };

                height += elementHeight + spacing;
            }

            var submitButton = UIButton.FromType(UIButtonType.RoundedRect);
            submitButton.Frame = new CGRect(0, 0, elementWidth / 2, elementHeight);
            submitButton.SetTitle("Create", UIControlState.Normal);
            submitButton.BackgroundColor = UIColor.White;
            submitButton.Layer.CornerRadius = 5f;

            submitButton.TouchUpInside += (sender, args) =>
            {
                var name = _fileNameField.Text;
                var extension = hasExtension ? _extensions[segmentedControl?.SelectedSegment ?? 0] : "";

                if (string.IsNullOrEmpty(name))
                    return;

                FileName = name.EndsWith(extension) ? name : name + extension;
                
                DismissViewController(false, null);
            };

            var cancelButton = UIButton.FromType(UIButtonType.RoundedRect);
            cancelButton.Frame = new CGRect(0, 0, elementWidth / 2, elementHeight);
            cancelButton.SetTitle("Cancel", UIControlState.Normal);
            cancelButton.BackgroundColor = UIColor.White;
            cancelButton.Layer.CornerRadius = 5f;

            cancelButton.TouchUpInside += delegate
            {
                DismissViewController(false, null);
            };

            var buttonStackView = new UIStackView(new CGRect(0, 0, elementWidth, elementHeight))
            {
                Axis = UILayoutConstraintAxis.Horizontal,
                Distribution = UIStackViewDistribution.FillEqually,
                AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
                Spacing = spacing,
            };

            buttonStackView.AddArrangedSubview(submitButton);
            buttonStackView.AddArrangedSubview(cancelButton);

            height += elementHeight;

            var width = elementWidth;
            var x = View.Bounds.Width / 2 - width / 2;
            var y = View.Bounds.Height / 2 - height / 2;

            var mainStack = new UIStackView(new CGRect(x, y, width, height))
            {
                AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
                Distribution = UIStackViewDistribution.EqualCentering,
                Axis = UILayoutConstraintAxis.Vertical,
                Spacing = spacing,
            };

            mainStack.AddArrangedSubview(filenameStackView);
            if(segmentedControl != null) mainStack.AddArrangedSubview(segmentedControl);
            mainStack.AddArrangedSubview(buttonStackView);

            spacing *= 2;
            x -= spacing;
            width += spacing * 2;
            y -= spacing;
            height += spacing * 2;

            var background = new UIView(new CGRect(x, y, width, height))
            {
                BackgroundColor = UIColor.Gray,
                AutoresizingMask = UIViewAutoresizing.FlexibleMargins,
            };

            background.Layer.CornerRadius = spacing;
            background.Layer.MasksToBounds = true;

            View.AddSubviews(new []{ background, mainStack });
        }

        public override void ViewDidDisappear(bool animated)
        {   
            base.ViewDidDisappear(animated);

            OnViewDidDisappear?.Invoke(this, null);
        }
    }
}