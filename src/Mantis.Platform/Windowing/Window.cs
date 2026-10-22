using Mantis.Core;
using Mantis.Core.Logging;
using Mantis.Core.Numerics;
using Mantis.Platform.Windowing.Events;

using SDL3;

namespace Mantis.Platform.Windowing;

public sealed class Window : IDisposable
{
	#region Events

	public event WindowVisibilityChangedHandler? VisibilityChanged;
	public event WindowPositionChangedHandler? PositionChanged;
	public event WindowSizeChangedHandler? SizeChanged;
	public event WindowPixelSizeChangedHandler? PixelSizeChanged;
	public event WindowStateChangedHandler? StateChanged;
	public event WindowMouseHoverChangedHandler? MouseHoverChanged;
	public event WindowFocusChangedHandler? FocusChanged;
	public event WindowCloseRequestedHandler? CloseRequested;
	public event WindowScreenChangedHandler? ScreenChanged;
	public event WindowScreenScaleChangedHandler? ScreenScaleChanged;
	public event WindowFullscreenChangedHandler? FullscreenChanged;
	public event WindowDisposedHandler? Disposed;

	#endregion

	private bool disposed;

	#region Properties

	private readonly nint handle;
	internal nint Handle
	{
		get
		{
			ObjectDisposedException.ThrowIf(disposed, this);
			return handle;
		}
	}

	internal uint Id { get; private init; }

	private string title;
	public string Title
	{
		get => title;
		set
		{
			bool result = SDL.SetWindowTitle(Handle, value);
			PlatformContext.ApiCall(result, LogCategory.Window);
			if (result)
			{
				title = value;
			}
		}
	}

	public Vector2I Position
	{
		get
		{
			bool result = SDL.GetWindowPosition(Handle, out int x, out int y);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return new(x, y);
		}
		set
		{
			bool result = SDL.SetWindowPosition(Handle, value.X, value.Y);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public Vector2I Size
	{
		get
		{
			bool result = SDL.GetWindowSize(Handle, out int x, out int y);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return new(x, y);
		}
		set
		{
			bool result = SDL.SetWindowSize(Handle, value.X, value.Y);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public Vector2I MinimumSize
	{
		get
		{
			bool result = SDL.GetWindowMinimumSize(Handle, out int x, out int y);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return new(x, y);
		}
		set
		{
			bool result = SDL.SetWindowMinimumSize(Handle, value.X, value.Y);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public Vector2I MaximumSize
	{
		get
		{
			bool result = SDL.GetWindowMaximumSize(Handle, out int x, out int y);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return new(x, y);
		}
		set
		{
			bool result = SDL.SetWindowMaximumSize(Handle, value.X, value.Y);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public float? AspectRatio
	{
		get
		{
			bool result = SDL.GetWindowAspectRatio(Handle, out float x, out float _);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return x == 0 ? null : x;
		}
		set
		{
			bool result = SDL.SetWindowAspectRatio(Handle, value ?? 0, value ?? 0);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public Vector2I PixelSize
	{
		get
		{
			bool result = SDL.GetWindowSizeInPixels(Handle, out int x, out int y);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return new(x, y);
		}
	}

	public WindowProgressState ProgressState
	{
		get;
		set
		{
			bool result = SDL.SetWindowProgressState(Handle, value.ToSdl());
			PlatformContext.ApiCall(result, LogCategory.Window);
			if (result)
			{
				field = value;
			}
		}
	}

	public float ProgressValue
	{
		get;
		set
		{
			bool result = SDL.SetWindowProgressValue(Handle, value);
			PlatformContext.ApiCall(result, LogCategory.Window);
			if (result)
			{
				field = value;
			}
		}
	}

	public float Opacity
	{
		get
		{
			float result = SDL.GetWindowOpacity(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window, -1f, false);
			return result;
		}
		set
		{
			bool result = SDL.SetWindowOpacity(Handle, value);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public bool Borderless
	{
		get => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.Borderless);
		set
		{
			bool result = SDL.SetWindowBordered(Handle, !value);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public bool Resizable
	{
		get => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.Resizable);
		set
		{
			bool result = SDL.SetWindowResizable(Handle, value);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public bool AlwaysOnTop
	{
		get => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.AlwaysOnTop);
		set
		{
			bool result = SDL.SetWindowAlwaysOnTop(Handle, value);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public WindowState State
	{
		get
		{
			var flags = SDL.GetWindowFlags(Handle);
			if (flags.HasFlag(SDL.WindowFlags.Minimized))
			{
				return WindowState.Minimized;
			}
			else if (flags.HasFlag(SDL.WindowFlags.Maximized))
			{
				return WindowState.Maximized;
			}
			else
			{
				return WindowState.Normal;
			}
		}
	}

	public float ScreenScale
	{
		get
		{
			float result = SDL.GetWindowDisplayScale(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return result;
		}
	}

	public float PixelDensity
	{
		get
		{
			float result = SDL.GetWindowPixelDensity(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
			return result;
		}
	}

	public Screen? Screen
	{
		get
		{
			uint displayId = SDL.GetDisplayForWindow(Handle);
			PlatformContext.ApiCall(displayId, LogCategory.Window);
			return Screens.TryGetScreen(displayId, out var screen) ? screen : null;
		}
	}

	public bool Visible => !SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.Hidden);
	public bool Fullscreen => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.Fullscreen);
	public bool Transparent => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.Transparent);
	public bool Focused => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.InputFocus);
	public bool Focusable => !SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.NotFocusable);
	public bool Hovered => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.MouseFocus);
	public bool RelativeMouse => SDL.GetWindowFlags(Handle).HasFlag(SDL.WindowFlags.MouseRelativeMode);

	#endregion

	public Window(in WindowCreationDescriptor descriptor)
	{
		Log.Debug(LogCategory.Window, $"Creating window [{descriptor.Title}]");

		var flags = CreateFlags(in descriptor);
		handle = SDL.CreateWindow(descriptor.Title, descriptor.Size.X, descriptor.Size.Y, flags);
		PlatformContext.ApiCallThrow(Handle, LogCategory.Window);
		title = descriptor.Title;

		Id = SDL.GetWindowID(Handle);
		PlatformContext.ApiCall(Id, LogCategory.Window);

		PlatformContext.AddWindow(this);
	}

	private static SDL.WindowFlags CreateFlags(in WindowCreationDescriptor descriptor)
	{
		SDL.WindowFlags result = default;
		if (descriptor.Fullscreen)
		{
			result |= SDL.WindowFlags.Fullscreen;
		}
		if (descriptor.Hidden)
		{
			result |= SDL.WindowFlags.Hidden;
		}
		if (descriptor.Borderless)
		{
			result |= SDL.WindowFlags.Borderless;
		}
		if (descriptor.Resizable)
		{
			result |= SDL.WindowFlags.Resizable;
		}
		if (descriptor.State != WindowState.Normal)
		{
			if (descriptor.State == WindowState.Minimized)
			{
				result |= SDL.WindowFlags.Minimized;
			}
			else if (descriptor.State == WindowState.Maximized)
			{
				result |= SDL.WindowFlags.Maximized;
			}
		}
		if (descriptor.Focus)
		{
			result |= SDL.WindowFlags.InputFocus;
		}
		if (descriptor.NonFocusable)
		{
			result |= SDL.WindowFlags.NotFocusable;
		}
		if (descriptor.AlwaysOnTop)
		{
			result |= SDL.WindowFlags.AlwaysOnTop;
		}
		if (descriptor.Transparent)
		{
			result |= SDL.WindowFlags.Transparent;
		}
		if (descriptor.HighPixelDensity)
		{
			result |= SDL.WindowFlags.HighPixelDensity;
		}
		return result;
	}

	#region Methods

	public void SetVisibility(bool visible)
	{
		if (visible)
		{
			bool result = SDL.ShowWindow(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
		else
		{
			bool result = SDL.HideWindow(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public void SetState(WindowState state)
	{
		if (state == WindowState.Minimized)
		{
			bool result = SDL.MinimizeWindow(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
		else if (state == WindowState.Maximized)
		{
			bool result = SDL.MaximizeWindow(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
		else
		{
			bool result = SDL.RestoreWindow(Handle);
			PlatformContext.ApiCall(result, LogCategory.Window);
		}
	}

	public void SetFullscreen(bool fullscreen)
	{
		bool result = SDL.SetWindowFullscreen(Handle, fullscreen);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	public void SetRelativeMouse(bool relativeMouse)
	{
		bool result = SDL.SetWindowRelativeMouseMode(Handle, relativeMouse);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	public void StartFlash(WindowFlashMode mode)
	{
		var sdlMode = mode == WindowFlashMode.Brief ? SDL.FlashOperation.Briefly : SDL.FlashOperation.UntilFocused;
		bool result = SDL.FlashWindow(Handle, sdlMode);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	public void StopFlash()
	{
		bool result = SDL.FlashWindow(Handle, SDL.FlashOperation.Cancel);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	public void Raise()
	{
		bool result = SDL.RaiseWindow(Handle);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	public void Sync()
	{
		bool result = SDL.SyncWindow(Handle);
		PlatformContext.ApiCall(result, LogCategory.Window);
	}

	#endregion

	#region Event handlers

	internal void OnVisibilityChanged(bool isVisible)
	{
		var mantisEvent = new WindowVisibilityChangedEvent()
		{
			Window = this,
			IsVisible = isVisible,
		};
		VisibilityChanged?.Invoke(in mantisEvent);
	}

	internal void OnPositionChanged(Vector2I position)
	{
		var mantisEvent = new WindowPositionChangedEvent()
		{
			Window = this,
			Position = position,
		};
		PositionChanged?.Invoke(in mantisEvent);
	}

	internal void OnSizeChanged(Vector2I size)
	{
		var mantisEvent = new WindowSizeChangedEvent()
		{
			Window = this,
			Size = size,
		};
		SizeChanged?.Invoke(in mantisEvent);
	}

	internal void OnPixelSizeChanged(Vector2I size)
	{
		var mantisEvent = new WindowPixelSizeChangedEvent()
		{
			Window = this,
			Size = size,
		};
		PixelSizeChanged?.Invoke(in mantisEvent);
	}

	internal void OnStateChanged(WindowState state)
	{
		var mantisEvent = new WindowStateChangedEvent()
		{
			Window = this,
			State = state,
		};
		StateChanged?.Invoke(in mantisEvent);
	}

	internal void OnMouseHoverChanged(bool isMouseOverWindow)
	{
		var mantisEvent = new WindowMouseHoverChangedEvent()
		{
			Window = this,
			IsMouseOverWindow = isMouseOverWindow,
		};
		MouseHoverChanged?.Invoke(in mantisEvent);
	}

	internal void OnFocusChanged(bool isFocused)
	{
		var mantisEvent = new WindowFocusChangedEvent()
		{
			Window = this,
			IsFocused = isFocused,
		};
		FocusChanged?.Invoke(in mantisEvent);
	}

	internal void OnCloseRequested()
	{
		var mantisEvent = new WindowCloseRequestedEvent()
		{
			Window = this,
		};
		CloseRequested?.Invoke(in mantisEvent);
	}

	internal void OnScreenChanged(in Screen screen)
	{
		var mantisEvent = new WindowScreenChangedEvent()
		{
			Window = this,
			Screen = screen,
		};
		ScreenChanged?.Invoke(in mantisEvent);
	}

	internal void OnScreenScaleChanged()
	{
		var mantisEvent = new WindowScreenScaleChangedEvent()
		{
			Window = this,
			ScreenScale = ScreenScale,
		};
		ScreenScaleChanged?.Invoke(in mantisEvent);
	}

	internal void OnFullscreenChanged(bool isInFullscreen)
	{
		var mantisEvent = new WindowFullscreenChangedEvent()
		{
			Window = this,
			IsInFullscreen = isInFullscreen,
		};
		FullscreenChanged?.Invoke(in mantisEvent);
	}

	#endregion

	public void Dispose()
	{
		if (disposed)
		{
			return;
		}
		disposed = true;

		Log.Debug(LogCategory.Window, $"Disposing window [{Title}]");
		try
		{
			Disposed?.Invoke(new() { Window = this });
		}
		catch (Exception ex)
		{
			Log.Error(LogCategory.Window, ex);
			Utility.Breakpoint();
		}

		SDL.DestroyWindow(handle);

		PlatformContext.RemoveWindow(this);

		GC.SuppressFinalize(this);
	}
}
