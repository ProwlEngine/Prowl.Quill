﻿// SilkWindow.cs - Simplified
using Common;
using Prowl.Quill;
using Prowl.Scribe;
using Prowl.Scribe.Internal;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Vector2 = Prowl.Vector.Vector2;

namespace SilkExample
{
    public class SilkWindow : IDisposable
    {
        // Core components
        private IWindow _window;
        private GL _gl;
        private IInputContext _input;
        private Canvas _canvas;
        private List<IDemo> _demos;
        private int _currentDemoIndex;
        private SilkNetRenderer _renderer;

        // View properties
        private Vector2 _viewOffset = Vector2.zero;
        private double _zoom = 1.0;
        private double _rotation = 0.0;

        // Resources
        private TextureSilk _whiteTexture;
        private TextureSilk _demoTexture;
        private FontFile RobotoFont;
        private FontFile AlamakFont;
        
        // Input tracking
        private bool _isDragging = false;
        private System.Numerics.Vector2 _lastMousePos;

        public SilkWindow(WindowOptions options)
        {
            _window = Window.Create(options);
            
            // Set up window events
            _window.Load += OnLoad;
            _window.Render += OnRender;
            _window.Update += OnUpdate;
            _window.Resize += OnResize;
            _window.Closing += OnClosing;
        }

        public void Run() => _window.Run();

        private void OnLoad()
        {
            // Initialize core components
            _gl = _window.CreateOpenGL();
            _input = _window.CreateInput();
            SetupInputHandlers();
            
            // Load textures
            _whiteTexture = TextureSilk.LoadFromFile(_gl, "Textures/white.png");
            _demoTexture = TextureSilk.LoadFromFile(_gl, "Textures/wall.png");
            
            // Set up renderer and canvas
            _gl.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
            _renderer = new SilkNetRenderer(_gl);
            _renderer.Initialize((int)_window.Size.X, (int)_window.Size.Y, _whiteTexture);
            _canvas = new Canvas(_renderer, new FontAtlasSettings());

            // Load fonts
            RobotoFont = new FontFile("Fonts/Roboto.ttf");
            AlamakFont = new FontFile("Fonts/Alamak.ttf");

            // Initialize demos
            _demos = new List<IDemo>
            {
                new CanvasDemo(_canvas, (int)_window.Size.X, (int)_window.Size.Y, _demoTexture, RobotoFont, AlamakFont),
                new SVGDemo(_canvas, (int)_window.Size.X, (int)_window.Size.Y),
                new BenchmarkScene(_canvas, RobotoFont, (int)_window.Size.X, (int)_window.Size.Y),
            };
        }

        private void SetupInputHandlers()
        {
            // Set up keyboard handlers
            var keyboard = _input.Keyboards[0];
            keyboard.KeyDown += (kb, key, scancode) => {
                if (key == Key.Escape) _window.Close();
                if (key == Key.Left)
                    _currentDemoIndex = _currentDemoIndex - 1 < 0 ? _demos.Count - 1 : _currentDemoIndex - 1;
                if (key == Key.Right)
                    _currentDemoIndex = _currentDemoIndex + 1 == _demos.Count ? 0 : _currentDemoIndex + 1;
                if (key == Key.Space)
                    if (_demos[_currentDemoIndex] is SVGDemo svgDemo)
                        svgDemo.ParseSVG();
            };

            // Set up mouse handlers
            var mouse = _input.Mice[0];
            mouse.MouseDown += (_, button) => {
                if (button == MouseButton.Left) {
                    _isDragging = true;
                    _lastMousePos = mouse.Position;
                }
            };
            
            mouse.MouseUp += (_, button) => {
                if (button == MouseButton.Left) _isDragging = false;
            };
            
            mouse.MouseMove += (_, position) => {
                if (_isDragging) {
                    var delta = position - _lastMousePos;
                    _viewOffset.x += delta.X * (1.0 / _zoom);
                    _viewOffset.y += delta.Y * (1.0 / _zoom);
                    _lastMousePos = position;
                }
            };
            
            mouse.Scroll += (_, scrollWheel) => {
                _zoom = Math.Max(0.1, _zoom + scrollWheel.Y * 0.1);
            };
        }

        private void OnRender(double deltaTime)
        {
            // Clear screen
            _gl.Clear(ClearBufferMask.ColorBufferBit);
            
            // Clear and prepare canvas
            _canvas.Clear();
            
            // Render demo content
            _demos[_currentDemoIndex].RenderFrame(deltaTime, _viewOffset, _zoom, _rotation);
            
            // Draw canvas to screen
            _canvas.Render();
        }
        
        private void OnUpdate(double deltaTime)
        {
            // Handle keyboard rotation
            var keyboard = _input.Keyboards[0];
            if (keyboard.IsKeyPressed(Key.Q)) _rotation += 10.0 * deltaTime;
            if (keyboard.IsKeyPressed(Key.E)) _rotation -= 10.0 * deltaTime;
        }
        
        private void OnResize(Silk.NET.Maths.Vector2D<int> newSize)
        {
            _gl.Viewport(0, 0, (uint)newSize.X, (uint)newSize.Y);
            _renderer.UpdateProjection(newSize.X, newSize.Y);
        }

        private void OnClosing()
        {
            _demoTexture?.Dispose();
            _whiteTexture?.Dispose();
            _renderer?.Cleanup();
        }

        public void Dispose()
        {
            _gl?.Dispose();
            _input?.Dispose();
            _window?.Dispose();
        }
    }
}