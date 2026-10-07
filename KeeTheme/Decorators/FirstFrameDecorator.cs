using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KeeTheme.Decorators
{
    // Keep initial native child paints off screen; restore the original opacity
    // only after the synchronous paint and the theme's after-paint hooks return.
    internal sealed class FirstFrameDecorator : IDisposable
    {
        private readonly Form _form;
        private readonly double _opacity;
        private bool _armed;

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr window, IntPtr rectangle, IntPtr region, uint flags);

        internal FirstFrameDecorator(Form form)
        {
            _form = form;
            _opacity = form.Opacity;
            if (form.Visible) return;
            _armed = true;
            form.Shown += OnShown;
            form.Opacity = 0;
        }

        private void OnShown(object sender, EventArgs e)
        {
            _form.Shown -= OnShown;
            try
            {
                // INVALIDATE | ERASE | ALLCHILDREN | UPDATENOW.
                // No timers, sleeps, fades or repeated repaint loops.
                if (!_form.IsDisposed) RedrawWindow(_form.Handle, IntPtr.Zero, IntPtr.Zero, 0x0185);
            }
            finally { Restore(); }
        }

        private void Restore()
        {
            if (!_armed) return;
            _armed = false;
            if (!_form.IsDisposed && !_form.Disposing) _form.Opacity = _opacity;
        }

        public void Dispose()
        {
            _form.Shown -= OnShown;
            Restore();
        }
    }
}
