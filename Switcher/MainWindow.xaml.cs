using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Switcher
{
    #region BLUR
    internal enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 10,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy
    {
        public AccentState accentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData
    {
        public WindowCompositionAttributes Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }


    internal enum WindowCompositionAttributes
    {
        WCA_ACCENT_POLICY = 19
    }

    #endregion
    public partial class MainWindow : Window
    {
        private bool Debug = false;
        #region GUI
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private const int WM_NCHITTEST = 0x84;
        private const int HT_CLIENT = 0x1;
        private const int HT_CAPTION = 0x2;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded_t(object sender, RoutedEventArgs e)
        {
            EnableBlur();
        }

        internal void EnableBlur()
        {
            var windowHelper = new WindowInteropHelper(this);

            var accent = new AccentPolicy();
            accent.accentState = AccentState.ACCENT_ENABLE_BLURBEHIND;

            var accentStructSize = Marshal.SizeOf(accent);

            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData();
            data.Attribute = WindowCompositionAttributes.WCA_ACCENT_POLICY;
            data.SizeOfData = accentStructSize;
            data.Data = accentPtr;

            SetWindowCompositionAttribute(windowHelper.Handle, ref data);

            Marshal.FreeHGlobal(accentPtr);
        }

        private void onClickClose(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void onMouseHoverClose(object sender, MouseEventArgs e)
        {
            Close_btn.Effect = new InvertEffect();
        }

        private void onMouseLeaveClose(object sender, MouseEventArgs e)
        {
            Close_btn.Effect = new DropShadowEffect();
        }

        private void onLoadClose(object sender, RoutedEventArgs e)
        {
            Close_btn.Effect = new DropShadowEffect();
        }

        private void onLoadHide(object sender, RoutedEventArgs e)
        {
            Min_btn.Effect = new DropShadowEffect();
        }

        private void onMinHover(object sender, MouseEventArgs e)
        {
            Min_btn.Effect = new InvertEffect();
        }

        private void onMinLeave(object sender, MouseEventArgs e)
        {
            Min_btn.Effect = new DropShadowEffect();
        }

        private void onClickMin(object sender, MouseButtonEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void onMouseDownNavbar(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && WindowState == WindowState.Normal)
                WindowState = WindowState.Maximized;
            else
            {
                if(WindowState == WindowState.Maximized)
                {
                    this.Left = GetMousePosition().X;
                    this.Top = GetMousePosition().Y;
                }
                WindowState = WindowState.Normal;
            }
               
            this.DragMove();
        }

        private void onLinkClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 4)
                Debug = true;
            else
            {
                Debug = false;
                Process.Start("https://gigamons.de");
            }
        }

        private void onLinkHover(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
            Link.Foreground = Brushes.HotPink;
            Link.Opacity = 100;
        }

        private void onLinkLeave(object sender, MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
            Link.Foreground = Brushes.Black;
            Link.Opacity = 50;
        }

        #endregion

        private void Switch_Btn_Click(object sender, RoutedEventArgs e)
        {
            swtch switcher = new swtch(Debug);
            switcher.Switch();
            Switch_Btn.Content = swtch.isSwitched() ? "Switch to Gigamons" : "Switch to Bancho";
        }

        private void onLoadButton(object sender, RoutedEventArgs e)
        {
            System.Threading.Thread.Sleep(500);
            Switch_Btn.Content = swtch.isSwitched() ? "Switch to Gigamons" : "Switch to Bancho";
        }
    }

    #region Shader
    class InvertEffect : ShaderEffect
    {
        private const string _kshaderAsBase64 =
    @"AAP///7/HwBDVEFCHAAAAE8AAAAAA///AQAAABwAAAAAAQAASAAAADAAAAADAAAAAQACADgAAAAAAAAAaW5wdXQAq6sEAAwAAQABAAEAAAAAAAAAcHNfM18wAE1pY3Jvc29mdCAoUikgSExTTCBTaGFkZXIgQ29tcGlsZXIgMTAuMQCrUQAABQAAD6AAAIA/AAAAAAAAAAAAAAAAHwAAAgUAAIAAAAOQHwAAAgAAAJAACA+gQgAAAwAAD4AAAOSQAAjkoAIAAAMAAAeAAADkgQAAAKAFAAADAAgHgAAA/4AAAOSAAQAAAgAICIAAAP+A//8AAA==";

        private static readonly PixelShader _shader;

        static InvertEffect()
        {
            _shader = new PixelShader();
            _shader.SetStreamSource(new MemoryStream(Convert.FromBase64String(_kshaderAsBase64)));
        }

        public InvertEffect()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(InvertEffect), 0);
    }
    #endregion
}
