﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MahApps.Metro.Controls.Dialogs
{
    [TemplatePart(Name = nameof(PART_AffirmativeButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(PART_NegativeButton), Type = typeof(Button))]
    [TemplatePart(Name = nameof(PART_TextBox), Type = typeof(TextBox))]
    public class InputDialog : BaseMetroDialog
    {
        private CancellationTokenRegistration cancellationTokenRegistration;

        #region Controls

        private Button? PART_AffirmativeButton;
        private Button? PART_NegativeButton;
        private TextBox? PART_TextBox;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            this.PART_AffirmativeButton = this.GetTemplateChild(nameof(this.PART_AffirmativeButton)) as Button;
            this.PART_NegativeButton = this.GetTemplateChild(nameof(this.PART_NegativeButton)) as Button;
            this.PART_TextBox = this.GetTemplateChild(nameof(this.PART_TextBox)) as TextBox;
        }

        #endregion Controls

        #region DependencyProperties

        /// <summary>Identifies the <see cref="Message"/> dependency property.</summary>
        public static readonly DependencyProperty MessageProperty
            = DependencyProperty.Register(nameof(Message),
                                          typeof(string),
                                          typeof(InputDialog),
                                          new PropertyMetadata(default(string)));

        public string? Message
        {
            get => (string?)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }

        /// <summary>Identifies the <see cref="Input"/> dependency property.</summary>
        public static readonly DependencyProperty InputProperty
            = DependencyProperty.Register(nameof(Input),
                                          typeof(string),
                                          typeof(InputDialog),
                                          new PropertyMetadata(default(string)));

        public string? Input
        {
            get => (string?)this.GetValue(InputProperty);
            set => this.SetValue(InputProperty, value);
        }

        /// <summary>Identifies the <see cref="AffirmativeButtonText"/> dependency property.</summary>
        public static readonly DependencyProperty AffirmativeButtonTextProperty
            = DependencyProperty.Register(nameof(AffirmativeButtonText),
                                          typeof(string),
                                          typeof(InputDialog),
                                          new PropertyMetadata("OK"));

        public string AffirmativeButtonText
        {
            get => (string)this.GetValue(AffirmativeButtonTextProperty);
            set => this.SetValue(AffirmativeButtonTextProperty, value);
        }

        /// <summary>Identifies the <see cref="NegativeButtonText"/> dependency property.</summary>
        public static readonly DependencyProperty NegativeButtonTextProperty
            = DependencyProperty.Register(nameof(NegativeButtonText),
                                          typeof(string),
                                          typeof(InputDialog),
                                          new PropertyMetadata("Cancel"));

        public string NegativeButtonText
        {
            get => (string)this.GetValue(NegativeButtonTextProperty);
            set => this.SetValue(NegativeButtonTextProperty, value);
        }

        #endregion DependencyProperties

        #region Constructor

        internal InputDialog()
            : this(null)
        {
        }

        internal InputDialog(MetroWindow? parentWindow)
            : this(parentWindow, null)
        {
        }

        internal InputDialog(MetroWindow? parentWindow, MetroDialogSettings? settings)
            : base(parentWindow, settings)
        {
            this.SetCurrentValue(AffirmativeButtonTextProperty, this.DialogSettings.AffirmativeButtonText);
            this.SetCurrentValue(NegativeButtonTextProperty, this.DialogSettings.NegativeButtonText);
        }

        static InputDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(InputDialog), new FrameworkPropertyMetadata(typeof(InputDialog)));
        }

        #endregion Constructor

        private RoutedEventHandler? negativeHandler = null;
        private KeyEventHandler? negativeKeyHandler = null;
        private RoutedEventHandler? affirmativeHandler = null;
        private KeyEventHandler? affirmativeKeyHandler = null;
        private KeyEventHandler? escapeKeyHandler = null;

        internal Task<string?> WaitForButtonPressAsync()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.Focus();
                    if (this.PART_TextBox is not null)
                    {
                        this.PART_TextBox.Focus();
                    }
                }));

            var tcs = new TaskCompletionSource<string?>();

            void CleanUpHandlers()
            {
                if (this.PART_TextBox is not null)
                {
                    this.PART_TextBox.KeyDown -= this.affirmativeKeyHandler;
                }

                this.KeyDown -= this.escapeKeyHandler;

                if (this.PART_NegativeButton is not null)
                {
                    this.PART_NegativeButton.Click -= this.negativeHandler;
                }

                if (this.PART_AffirmativeButton is not null)
                {
                    this.PART_AffirmativeButton.Click -= this.affirmativeHandler;
                }

                if (this.PART_NegativeButton is not null)
                {
                    this.PART_NegativeButton.KeyDown -= this.negativeKeyHandler;
                }

                if (this.PART_AffirmativeButton is not null)
                {
                    this.PART_AffirmativeButton.KeyDown -= this.affirmativeKeyHandler;
                }

                this.cancellationTokenRegistration.Dispose();
            }

            this.cancellationTokenRegistration = this.DialogSettings
                                                     .CancellationToken
                                                     .Register(() =>
                                                         {
                                                             this.BeginInvoke(() =>
                                                                 {
                                                                     CleanUpHandlers();
                                                                     tcs.TrySetResult(null!);
                                                                 });
                                                         });

            this.escapeKeyHandler = (_, e) =>
                {
                    if (e.Key == Key.Escape || (e.Key == Key.System && e.SystemKey == Key.F4))
                    {
                        CleanUpHandlers();

                        tcs.TrySetResult(null!);
                    }
                };

            this.negativeKeyHandler = (_, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        CleanUpHandlers();

                        tcs.TrySetResult(null!);
                    }
                };

            this.affirmativeKeyHandler = (_, e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        CleanUpHandlers();

                        tcs.TrySetResult(this.Input!);
                    }
                };

            this.negativeHandler = (_, e) =>
                {
                    CleanUpHandlers();

                    tcs.TrySetResult(null!);

                    e.Handled = true;
                };

            this.affirmativeHandler = (_, e) =>
                {
                    CleanUpHandlers();

                    tcs.TrySetResult(this.Input!);

                    e.Handled = true;
                };

            if (this.PART_NegativeButton is not null)
            {
                this.PART_NegativeButton.KeyDown += this.negativeKeyHandler;
            }

            if (this.PART_AffirmativeButton is not null)
            {
                this.PART_AffirmativeButton.KeyDown += this.affirmativeKeyHandler;
            }

            if (this.PART_TextBox is not null)
            {
                this.PART_TextBox.KeyDown += this.affirmativeKeyHandler;
            }

            this.KeyDown += this.escapeKeyHandler;

            if (this.PART_NegativeButton is not null)
            {
                this.PART_NegativeButton.Click += this.negativeHandler;
            }

            if (this.PART_AffirmativeButton is not null)
            {
                this.PART_AffirmativeButton.Click += this.affirmativeHandler;
            }

            return tcs.Task;
        }
    }
}